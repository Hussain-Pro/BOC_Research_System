using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using BOC.Application.Common.Interfaces;

namespace BOC.Application.Features.Admin.Queries;

public record GetAuditLogsQuery : IRequest<List<AuditLogDto>>;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, List<AuditLogDto>>
{
    private readonly IBOCDbContext _context;
    public GetAuditLogsQueryHandler(IBOCDbContext context) => _context = context;

    public async Task<List<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        return new List<AuditLogDto>(); // Placeholder
    }
}
