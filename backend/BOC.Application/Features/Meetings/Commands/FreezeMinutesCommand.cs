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
using BOC.Domain.Events;
using BOC.Domain.Exceptions;

namespace BOC.Application.Features.Meetings.Commands;

public record FreezeMinutesCommand(
    Guid MinutesId,
    Guid FrozenById) : IRequest<bool>;

public class FreezeMinutesCommandValidator : AbstractValidator<FreezeMinutesCommand>
{
    public FreezeMinutesCommandValidator()
    {
        RuleFor(x => x.MinutesId).NotEmpty();
        RuleFor(x => x.FrozenById).NotEmpty();
    }
}

public class FreezeMinutesCommandHandler : IRequestHandler<FreezeMinutesCommand, bool>
{
    private readonly IBOCDbContext _context;

    public FreezeMinutesCommandHandler(IBOCDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(FreezeMinutesCommand request, CancellationToken cancellationToken)
    {
        var minutes = await _context.MeetingMinutes
            .Include(m => m.ResearchPapers)
            .FirstOrDefaultAsync(m => m.Id == request.MinutesId, cancellationToken)
            ?? throw new ValidationException("Meeting minutes not found.");

        if (minutes.Status == MeetingMinutesStatus.Minutes_Frozen)
        {
            throw new FrozenMinutesException(minutes.Id);
        }

        var user = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Id == request.FrozenById, cancellationToken)
            ?? throw new ValidationException("User performing freeze not found.");

        var now = DateTime.UtcNow;

        // 1. Log Freeze Event
        var freezeEvent = new FreezeEvent
        {
            Id = Guid.NewGuid(),
            MeetingMinutesId = minutes.Id,
            FrozenById = user.Id,
            FrozenAt = now,
            CreatedAt = now
        };
        _context.FreezeEvents.Add(freezeEvent);

        // 2. Update Minutes Status
        minutes.Status = MeetingMinutesStatus.Minutes_Frozen;
        minutes.ModifiedAt = now;

        // 3. Archive associated research papers
        foreach (var paper in minutes.ResearchPapers)
        {
            if (paper.State == ResearchState.Pass_Approved || paper.State == ResearchState.Fail_Rejected)
            {
                paper.State = ResearchState.Archived;
                paper.ModifiedAt = now;
            }
        }

        // Add domain event which will be captured by OutboxInterceptor
        minutes.AddDomainEvent(new MinutesFrozenEvent(minutes.Id, user.Id));

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
