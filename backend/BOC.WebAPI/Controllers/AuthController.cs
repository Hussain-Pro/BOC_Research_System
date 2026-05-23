using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BOC.Application.Features.Auth.Commands;

namespace BOC.WebAPI.Controllers;

public class AuthController : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<Guid>> Register(RegisterUserCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResultDto>> Login(LoginUserCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPost("verify-2fa")]
    public async Task<ActionResult<VerifyTwoFactorResultDto>> Verify2Fa(VerifyTwoFactorCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<RefreshTokenResultDto>> RefreshToken(RefreshTokenCommand command)
    {
        return await Mediator.Send(command);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword(ChangePasswordCommand command)
    {
        // Security check: ensure user is changing their own password
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || command.UserId.ToString() != userId)
        {
            return Forbid();
        }

        await Mediator.Send(command);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword(ResetPasswordCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }
}
