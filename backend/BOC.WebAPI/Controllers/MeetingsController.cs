using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BOC.Application.Features.Meetings.Commands;
using BOC.Application.Features.Meetings.Queries;

namespace BOC.WebAPI.Controllers;

public class MeetingsController : ApiControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<MeetingDetailsDto>> GetMeetingDetails(Guid id)
    {
        return await Mediator.Send(new GetMeetingDetailsQuery(id));
    }

    [HttpPost("vote")]
    public async Task<ActionResult<bool>> CastVote(CastVoteCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPost("grade")]
    public async Task<ActionResult<bool>> SubmitGrade(SubmitChairmanScoreCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPost("freeze-minutes")]
    public async Task<ActionResult<bool>> FreezeMinutes(FreezeMinutesCommand command)
    {
        return await Mediator.Send(command);
    }
}
