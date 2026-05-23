using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using BOC.Application.Common.Interfaces;

namespace BOC.Application.Features.Evaluators.Queries;

public record GetEvaluatorRosterQuery : IRequest<List<EvaluatorRosterDto>>;

public class EvaluatorRosterDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int ActiveAssignments { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class GetEvaluatorRosterQueryHandler : IRequestHandler<GetEvaluatorRosterQuery, List<EvaluatorRosterDto>>
{
    private readonly IBOCDbContext _context;
    public GetEvaluatorRosterQueryHandler(IBOCDbContext context) => _context = context;

    public async Task<List<EvaluatorRosterDto>> Handle(GetEvaluatorRosterQuery request, CancellationToken cancellationToken)
    {
        return new List<EvaluatorRosterDto>(); // Placeholder
    }
}
