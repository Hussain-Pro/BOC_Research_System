using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using BOC.Application.Common.Interfaces;

namespace BOC.Application.Features.Profile.Queries;

public record GetProfileQuery(Guid UserId) : IRequest<UserProfileDto>;

public class UserProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Theme { get; set; } = "Light";
    public string Language { get; set; } = "AR";
}

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserProfileDto>
{
    private readonly IBOCDbContext _context;

    public GetProfileQueryHandler(IBOCDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        // Placeholder
        return new UserProfileDto();
    }
}
