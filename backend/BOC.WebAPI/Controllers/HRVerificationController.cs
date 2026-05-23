using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BOC.Application.Features.HRVerification.Queries;
using BOC.Application.Features.HRVerification.Commands;

namespace BOC.WebAPI.Controllers;

[Authorize(Roles = "HR Admin")] // Example role based on blueprint
public class HRVerificationController : ApiControllerBase
{
    [HttpGet("queue")]
    public async Task<ActionResult<List<HRVerificationItemDto>>> GetQueue()
    {
        return await Mediator.Send(new GetHRVerificationQueueQuery());
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult> Approve(Guid id, [FromBody] ApproveHRVerificationCommand command)
    {
        if (id != command.VerificationId) return BadRequest();
        await Mediator.Send(command);
        return NoContent();
    }
}
