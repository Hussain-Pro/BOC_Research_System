using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using BOC.Application.Common.Interfaces;

namespace BOC.Application.Features.Admin.Queries;

public record GetSystemConfigQuery : IRequest<SystemConfigDto>;

public class SystemConfigDto
{
    public Dictionary<string, string> Preferences { get; set; } = new();
}

public class GetSystemConfigQueryHandler : IRequestHandler<GetSystemConfigQuery, SystemConfigDto>
{
    private readonly IBOCDbContext _context;
    public GetSystemConfigQueryHandler(IBOCDbContext context) => _context = context;

    public async Task<SystemConfigDto> Handle(GetSystemConfigQuery request, CancellationToken cancellationToken)
    {
        return new SystemConfigDto(); // Placeholder
    }
}
