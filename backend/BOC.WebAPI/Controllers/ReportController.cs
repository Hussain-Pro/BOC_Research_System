using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BOC.Application.Features.Reports.Queries;

namespace BOC.WebAPI.Controllers;

[Authorize(Roles = "Chairman, System Administrator, Secretary")]
public class ReportController : ApiControllerBase
{
    [HttpGet("generate")]
    public async Task<IActionResult> GenerateReport([FromQuery] GenerateReportQuery query)
    {
        var result = await Mediator.Send(query);
        if (result == null) return NotFound();

        return File(result.FileStream, result.ContentType, result.FileName);
    }
}
