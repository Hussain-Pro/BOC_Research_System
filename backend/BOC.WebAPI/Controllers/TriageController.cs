using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BOC.Application.Features.Research.Commands;
using BOC.Application.Features.Triage.Queries;

namespace BOC.WebAPI.Controllers;

public class TriageController : ApiControllerBase
{
    [HttpGet("papers")]
    public async Task<ActionResult<List<TriagePaperDto>>> GetTriagePapers()
    {
        return await Mediator.Send(new GetTriagePapersQuery());
    }

    [HttpGet("evaluators")]
    public async Task<ActionResult<List<EligibleEvaluatorDto>>> GetEligibleEvaluators([FromQuery] Guid researchId)
    {
        return await Mediator.Send(new GetEligibleEvaluatorsQuery(researchId));
    }

    [HttpPost("assign")]
    public async Task<ActionResult<bool>> Assign(TriageAssignCommand command)
    {
        return await Mediator.Send(command);
    }
}
