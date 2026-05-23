using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BOC.Application.Common.Interfaces;
using BOC.Domain.Enums;

namespace BOC.Application.Features.Triage.Queries;

public record EligibleEvaluatorDto(
    Guid Id,
    string FullName,
    string EmployeeID,
    int Tier,
    int ActiveLoad,
    DateTime? LastAssignedDate,
    string SpecializationName
);

public record GetEligibleEvaluatorsQuery(Guid ResearchId) : IRequest<List<EligibleEvaluatorDto>>;

public class GetEligibleEvaluatorsQueryHandler : IRequestHandler<GetEligibleEvaluatorsQuery, List<EligibleEvaluatorDto>>
{
    private readonly IBOCDbContext _context;

    public GetEligibleEvaluatorsQueryHandler(IBOCDbContext context)
    {
        _context = context;
    }

    public async Task<List<EligibleEvaluatorDto>> Handle(GetEligibleEvaluatorsQuery request, CancellationToken cancellationToken)
    {
        var paper = await _context.ResearchPapers
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.ResearchId, cancellationToken)
            ?? throw new ValidationException("Research paper not found.");

        if (paper.CategoryId == null)
        {
            return new List<EligibleEvaluatorDto>();
        }

        var category = await _context.ResearchCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == paper.CategoryId, cancellationToken);

        if (category == null)
        {
            return new List<EligibleEvaluatorDto>();
        }

        var specId = category.SpecializationId;
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // Fetch candidates with assignments and specializations
        var candidates = await _context.AppUsers
            .AsNoTracking()
            .Include(u => u.Assignments)
            .Include(u => u.EvaluatorSpecializations)
                .ThenInclude(es => es.Specialization)
            .Where(u => u.EvaluatorStatus == EvaluatorStatus.Active &&
                        u.AccountStatus == AccountStatus.Active &&
                        u.Id != paper.ResearcherId &&
                        u.DepartmentId != paper.DepartmentId &&
                        u.DirectorateId != paper.DirectorateId &&
                        u.EvaluatorSpecializations.Any(es => es.SpecializationId == specId))
            .ToListAsync(cancellationToken);

        var list = new List<EligibleEvaluatorDto>();

        foreach (var user in candidates)
        {
            // 1. Retirement check (age < 63)
            if (user.BirthDate.HasValue)
            {
                var age = today.Year - user.BirthDate.Value.Year;
                if (user.BirthDate.Value > today.AddYears(-age)) age--;
                if (age >= 63)
                {
                    continue; // Skip retired
                }
            }

            var totalAssignments = user.Assignments.Count;
            var activeLoad = user.Assignments.Count(a => a.Status == AssignmentStatus.Active && a.SubmittedDate == null);
            var lastAssignedDate = totalAssignments > 0 ? user.Assignments.Max(a => a.AssignedDate) : (DateTime?)null;

            int tier;
            if (totalAssignments == 0)
            {
                tier = 1; // Tier 1: Never assigned
            }
            else if (activeLoad == 0)
            {
                tier = 2; // Tier 2: Active load = 0, sorted by oldest assign date
            }
            else
            {
                tier = 3; // Tier 3: Active load > 0, sorted by load count ascending
            }

            var specName = user.EvaluatorSpecializations
                .FirstOrDefault(es => es.SpecializationId == specId)?
                .Specialization?.Name ?? "N/A";

            list.Add(new EligibleEvaluatorDto(
                user.Id,
                user.FullName,
                user.EmployeeID,
                tier,
                activeLoad,
                lastAssignedDate,
                specName
            ));
        }

        // Sort by:
        // 1. Tier (1 -> 2 -> 3)
        // 2. For Tier 2, oldest assign date (ascending)
        // 3. For Tier 3, active load count (ascending)
        return list
            .OrderBy(e => e.Tier)
            .ThenBy(e => e.Tier == 2 ? (e.LastAssignedDate ?? DateTime.MaxValue) : DateTime.MinValue)
            .ThenBy(e => e.Tier == 3 ? e.ActiveLoad : 0)
            .ToList();
    }
}
