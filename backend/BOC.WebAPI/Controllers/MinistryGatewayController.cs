using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BOC.Application.Features.Ministry.Queries;

namespace BOC.WebAPI.Controllers;

[Authorize(Roles = "Chairman, System Administrator")]
public class MinistryGatewayController : ApiControllerBase
{
    [HttpGet("batches")]
    public async Task<ActionResult<List<MinistryBatchDto>>> GetBatches()
    {
        return await Mediator.Send(new GetMinistryBatchesQuery());
    }
}
