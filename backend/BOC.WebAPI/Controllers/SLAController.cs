using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BOC.Application.Features.Sla.Queries;

namespace BOC.WebAPI.Controllers;

[Authorize(Roles = "System Administrator, Chairman, HR Admin")]
public class SLAController : ApiControllerBase
{
    [HttpGet("violations")]
    public async Task<ActionResult<List<SlaViolationDto>>> GetViolations()
    {
        return await Mediator.Send(new GetSLADashboardQuery());
    }
}
