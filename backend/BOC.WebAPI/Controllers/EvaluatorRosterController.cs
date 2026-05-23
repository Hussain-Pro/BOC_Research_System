using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BOC.Application.Features.Evaluators.Queries;

namespace BOC.WebAPI.Controllers;

[Authorize(Roles = "Chairman, Deputy Chairman, Secretary")]
public class EvaluatorRosterController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<EvaluatorRosterDto>>> GetRoster()
    {
        return await Mediator.Send(new GetEvaluatorRosterQuery());
    }
}
