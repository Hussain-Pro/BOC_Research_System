using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BOC.Application.Common.Interfaces;
using BOC.Domain.Entities;
using BOC.Domain.Enums;
using BOC.Domain.Exceptions;
using BOC.Domain.Fsm;
using BOC.Domain.Services;

namespace BOC.Application.Features.Meetings.Commands;

public record SubmitChairmanScoreCommand(
    Guid ResearchId,
    Guid ChairmanId,
    Guid MeetingMinutesId,
    decimal Score,
    string? Comments) : IRequest<bool>;

public class SubmitChairmanScoreCommandValidator : AbstractValidator<SubmitChairmanScoreCommand>
{
    public SubmitChairmanScoreCommandValidator()
    {
        RuleFor(x => x.ResearchId).NotEmpty();
        RuleFor(x => x.ChairmanId).NotEmpty();
        RuleFor(x => x.MeetingMinutesId).NotEmpty();
        RuleFor(x => x.Score).InclusiveBetween(0, 30);
    }
}

public class SubmitChairmanScoreCommandHandler : IRequestHandler<SubmitChairmanScoreCommand, bool>
{
    private readonly IBOCDbContext _context;

    public SubmitChairmanScoreCommandHandler(IBOCDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(SubmitChairmanScoreCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if minutes are frozen
        var minutes = await _context.MeetingMinutes
            .FirstOrDefaultAsync(m => m.Id == request.MeetingMinutesId, cancellationToken)
            ?? throw new ValidationException("Meeting minutes not found.");

        if (minutes.Status == MeetingMinutesStatus.Minutes_Frozen)
        {
            throw new FrozenMinutesException(minutes.Id);
        }

        // 2. Validate paper and chairman
        var paper = await _context.ResearchPapers
            .Include(p => p.EvaluatorAssignments)
                .ThenInclude(a => a.Evaluations)
            .FirstOrDefaultAsync(p => p.Id == request.ResearchId, cancellationToken)
            ?? throw new ValidationException("Research paper not found.");

        var chairman = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Id == request.ChairmanId, cancellationToken)
            ?? throw new ValidationException("Chairman not found.");

        // 3. Find or create Chairman Score
        var existingScore = await _context.ChairmanScores
            .FirstOrDefaultAsync(cs => cs.ResearchId == request.ResearchId &&
                                      cs.MeetingMinutesId == request.MeetingMinutesId, cancellationToken);

        var now = DateTime.UtcNow;

        if (existingScore != null)
        {
            existingScore.Score = request.Score;
            existingScore.Comments = request.Comments;
            existingScore.SubmittedAt = now;
        }
        else
        {
            var newScore = new ChairmanScore
            {
                Id = Guid.NewGuid(),
                ResearchId = request.ResearchId,
                ChairmanId = request.ChairmanId,
                MeetingMinutesId = request.MeetingMinutesId,
                Score = request.Score,
                Comments = request.Comments,
                SubmittedAt = now,
                CreatedAt = now
            };
            _context.ChairmanScores.Add(newScore);
        }

        // 4. Calculate Final Score using Domain Service
        var evaluatorScores = paper.EvaluatorAssignments
            .SelectMany(a => a.Evaluations)
            .Select(e => e.Score)
            .ToList();

        var scoringService = new ResearchScoringService();
        var finalScore = scoringService.CalculateFinalScore(evaluatorScores, request.Score);

        paper.FinalScore = finalScore;
        paper.ModifiedAt = now;

        // 5. Determine State Transition
        var minPassingScoreConfig = await _context.SystemConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ConfigKey == "Min_Passing_Score", cancellationToken);
        var minPassingScore = decimal.TryParse(minPassingScoreConfig?.ConfigValue, out var score) ? score : 70m;

        var nextState = finalScore >= minPassingScore ? ResearchState.Pass_Approved : ResearchState.Fail_Rejected;

        // Force paper state to Pending_Chairman_Grading to allow transition if needed,
        // or validate transition directly from current state.
        // During live committee meetings, the paper state might be Pending_Chairman_Grading or Dispatched_To_Evaluators.
        if (paper.State == ResearchState.Dispatched_To_Evaluators)
        {
            paper.State = ResearchState.Pending_Chairman_Grading;
        }

        ResearchStateMachine.ValidateTransition(paper.State, nextState);
        paper.State = nextState;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
