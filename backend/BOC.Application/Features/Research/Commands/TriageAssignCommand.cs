using System;
using System.Collections.Generic;
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
using BOC.Domain.Fsm;

namespace BOC.Application.Features.Research.Commands;

public record TriageAssignCommand(
    Guid ResearchId,
    Guid MappedById,
    List<Guid> EvaluatorIds,
    List<Guid> MemberIds) : IRequest<bool>;

public class TriageAssignCommandValidator : AbstractValidator<TriageAssignCommand>
{
    public TriageAssignCommandValidator()
    {
        RuleFor(x => x.ResearchId).NotEmpty();
        RuleFor(x => x.MappedById).NotEmpty();
        RuleFor(x => x.EvaluatorIds).NotNull();
        RuleFor(x => x.MemberIds).NotNull();
    }
}

public class TriageAssignCommandHandler : IRequestHandler<TriageAssignCommand, bool>
{
    private readonly IBOCDbContext _context;

    public TriageAssignCommandHandler(IBOCDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(TriageAssignCommand request, CancellationToken cancellationToken)
    {
        var paper = await _context.ResearchPapers
            .FirstOrDefaultAsync(p => p.Id == request.ResearchId, cancellationToken)
            ?? throw new ValidationException("Research paper not found.");

        // Enforce FSM State Check
        ResearchStateMachine.ValidateTransition(paper.State, ResearchState.Dispatched_To_Evaluators);

        var assigner = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Id == request.MappedById, cancellationToken)
            ?? throw new ValidationException("Triage administrator not found.");

        // Read SLA default days from configurations
        var slaConfig = await _context.SystemConfigurations
            .FirstOrDefaultAsync(c => c.ConfigKey == "SLA_DefaultDays", cancellationToken);
        var slaDays = int.TryParse(slaConfig?.ConfigValue, out var days) ? days : 14;

        var now = DateTime.UtcNow;
        var dueDate = now.AddDays(slaDays);

        // Fetch all assigned users (evaluators & members)
        var allUserIds = request.EvaluatorIds.Concat(request.MemberIds).Distinct().ToList();
        var assignedUsers = await _context.AppUsers
            .Where(u => allUserIds.Contains(u.Id))
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(now);

        foreach (var user in assignedUsers)
        {
            // Rule 1: Evaluator status check
            if (user.EvaluatorStatus != EvaluatorStatus.Active || user.AccountStatus != AccountStatus.Active)
            {
                throw new ValidationException($"User {user.FullName} is not an active/verified evaluator.");
            }

            // Rule 2: 63-year retirement age check
            if (user.BirthDate.HasValue)
            {
                var age = today.Year - user.BirthDate.Value.Year;
                if (user.BirthDate.Value > today.AddYears(-age)) age--;
                if (age >= 63)
                {
                    throw new ValidationException($"User {user.FullName} has reached retirement age (63) and cannot be assigned.");
                }
            }

            // Rule 3: Smart Anti-Conflict of Interest Filter
            if (user.Id == paper.ResearcherId)
            {
                throw new ValidationException($"User {user.FullName} is the researcher of this paper and cannot be assigned.");
            }

            if (user.DepartmentId == paper.DepartmentId || user.DirectorateId == paper.DirectorateId)
            {
                throw new ValidationException($"Conflict of interest detected: User {user.FullName} is in the same department/directorate as the researcher.");
            }
        }

        // 1. Create TriageMappings (IsFinalized = false until vote lock, or set true if finalizing)
        foreach (var evalId in request.EvaluatorIds)
        {
            var mapping = new TriageMapping
            {
                Id = Guid.NewGuid(),
                ResearchId = paper.Id,
                MappedById = assigner.Id,
                EvaluatorId = evalId,
                MemberId = null,
                IsFinalized = false,
                MappedAt = now,
                CreatedAt = now
            };
            _context.TriageMappings.Add(mapping);

            // 2. Create Evaluator Assignment
            var assignment = new EvaluatorAssignment
            {
                Id = Guid.NewGuid(),
                ResearchId = paper.Id,
                EvaluatorId = evalId,
                AssignedById = assigner.Id,
                AssignedDate = now,
                DueDate = dueDate,
                Status = AssignmentStatus.Active,
                IsSLABreached = false,
                CreatedAt = now
            };
            _context.EvaluatorAssignments.Add(assignment);

            // Raise domain event for notifications
            paper.AddDomainEvent(new EvaluatorAssignedEvent(assignment.Id, paper.Id, evalId, dueDate));
        }

        foreach (var memberId in request.MemberIds)
        {
            var mapping = new TriageMapping
            {
                Id = Guid.NewGuid(),
                ResearchId = paper.Id,
                MappedById = assigner.Id,
                EvaluatorId = null,
                MemberId = memberId,
                IsFinalized = false,
                MappedAt = now,
                CreatedAt = now
            };
            _context.TriageMappings.Add(mapping);
        }

        // 3. Move paper state
        paper.State = ResearchState.Dispatched_To_Evaluators;
        paper.ModifiedAt = now;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
