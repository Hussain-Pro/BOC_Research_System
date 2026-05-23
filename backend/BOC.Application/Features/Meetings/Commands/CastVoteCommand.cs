using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BOC.Application.Common.Interfaces;
using BOC.Domain.Entities;
using BOC.Domain.Enums;
using BOC.Domain.Exceptions;

namespace BOC.Application.Features.Meetings.Commands;

public record CastVoteCommand(
    Guid MeetingId,
    Guid ResearchId,
    Guid MemberId,
    VoteValue VoteValue) : IRequest<bool>;

public class CastVoteCommandValidator : AbstractValidator<CastVoteCommand>
{
    public CastVoteCommandValidator()
    {
        RuleFor(x => x.MeetingId).NotEmpty();
        RuleFor(x => x.ResearchId).NotEmpty();
        RuleFor(x => x.MemberId).NotEmpty();
    }
}

public class CastVoteCommandHandler : IRequestHandler<CastVoteCommand, bool>
{
    private readonly IBOCDbContext _context;

    public CastVoteCommandHandler(IBOCDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CastVoteCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if minutes are frozen
        var minutes = await _context.MeetingMinutes
            .FirstOrDefaultAsync(m => m.MeetingId == request.MeetingId, cancellationToken);

        if (minutes != null && minutes.Status == MeetingMinutesStatus.Minutes_Frozen)
        {
            throw new FrozenMinutesException(minutes.Id);
        }

        // 2. Validate meeting and paper existence
        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken)
            ?? throw new ValidationException("Meeting not found.");

        var paper = await _context.ResearchPapers
            .FirstOrDefaultAsync(p => p.Id == request.ResearchId, cancellationToken)
            ?? throw new ValidationException("Research paper not found.");

        var member = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Id == request.MemberId, cancellationToken)
            ?? throw new ValidationException("Committee member not found.");

        // 3. Find existing vote or add new
        var existingVote = await _context.Votes
            .FirstOrDefaultAsync(v => v.MeetingId == request.MeetingId &&
                                      v.ResearchId == request.ResearchId &&
                                      v.MemberId == request.MemberId, cancellationToken);

        var now = DateTime.UtcNow;

        if (existingVote != null)
        {
            existingVote.VoteValue = request.VoteValue;
            existingVote.VotedAt = now;
        }
        else
        {
            var vote = new Vote
            {
                Id = Guid.NewGuid(),
                MeetingId = request.MeetingId,
                ResearchId = request.ResearchId,
                MemberId = request.MemberId,
                VoteValue = request.VoteValue,
                VotedAt = now,
                CreatedAt = now
            };
            _context.Votes.Add(vote);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
