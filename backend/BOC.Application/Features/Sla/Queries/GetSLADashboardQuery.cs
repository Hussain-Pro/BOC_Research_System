using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using BOC.Application.Common.Interfaces;

namespace BOC.Application.Features.Sla.Queries;

public record GetSLADashboardQuery : IRequest<List<SlaViolationDto>>;

public class SlaViolationDto
{
    public Guid PaperId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DaysOverdue { get; set; }
    public string EvaluatorName { get; set; } = string.Empty;
    public string WarningLevel { get; set; } = string.Empty; // e.g. "Warning", "Critical"
}

public class GetSLADashboardQueryHandler : IRequestHandler<GetSLADashboardQuery, List<SlaViolationDto>>
{
    private readonly IBOCDbContext _context;
    public GetSLADashboardQueryHandler(IBOCDbContext context) => _context = context;

    public async Task<List<SlaViolationDto>> Handle(GetSLADashboardQuery request, CancellationToken cancellationToken)
    {
        return new List<SlaViolationDto>(); // Placeholder
    }
}
