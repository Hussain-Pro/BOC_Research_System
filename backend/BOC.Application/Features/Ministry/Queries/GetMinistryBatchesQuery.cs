using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using BOC.Application.Common.Interfaces;

namespace BOC.Application.Features.Ministry.Queries;

public record GetMinistryBatchesQuery : IRequest<List<MinistryBatchDto>>;

public class MinistryBatchDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

public class GetMinistryBatchesQueryHandler : IRequestHandler<GetMinistryBatchesQuery, List<MinistryBatchDto>>
{
    private readonly IBOCDbContext _context;
    public GetMinistryBatchesQueryHandler(IBOCDbContext context) => _context = context;

    public async Task<List<MinistryBatchDto>> Handle(GetMinistryBatchesQuery request, CancellationToken cancellationToken)
    {
        return new List<MinistryBatchDto>(); // Placeholder
    }
}
