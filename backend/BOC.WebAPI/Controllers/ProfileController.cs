using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BOC.Application.Features.Profile.Queries;
using BOC.Application.Features.Profile.Commands;

namespace BOC.WebAPI.Controllers;

[Authorize]
public class ProfileController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        return await Mediator.Send(new GetProfileQuery(userId));
    }

    [HttpPut]
    public async Task<ActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId) || command.UserId != userId) return Forbid();

        await Mediator.Send(command);
        return NoContent();
    }
}
