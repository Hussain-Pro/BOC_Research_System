using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BOC.Application.Common.Interfaces;
using BOC.Domain.Enums;

namespace BOC.Application.Features.Triage.Queries;

public record TriagePaperDto(
    Guid Id,
    string TrackingNumber,
    string Title,
    string? Abstract,
    Guid ResearcherId,
    string ResearcherName,
    Guid DepartmentId,
    string DepartmentName,
    Guid DirectorateId,
    string DirectorateName,
    string CategoryName,
    DateTime CreatedAt
);

public record GetTriagePapersQuery : IRequest<List<TriagePaperDto>>;

public class GetTriagePapersQueryHandler : IRequestHandler<GetTriagePapersQuery, List<TriagePaperDto>>
{
    private readonly IBOCDbContext _context;

    public GetTriagePapersQueryHandler(IBOCDbContext context)
    {
        _context = context;
    }

    public async Task<List<TriagePaperDto>> Handle(GetTriagePapersQuery request, CancellationToken cancellationToken)
    {
        return await _context.ResearchPapers
            .AsNoTracking()
            .Where(p => p.State == ResearchState.Incoming_Triage_Queue)
            .Select(p => new TriagePaperDto(
                p.Id,
                p.TrackingNumber,
                p.Title,
                p.Abstract,
                p.ResearcherId,
                p.Researcher.FullName,
                p.DepartmentId,
                p.Department.Name,
                p.DirectorateId,
                p.Directorate.Name,
                p.Category != null ? p.Category.Name : "N/A",
                p.CreatedAt
            ))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
