using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BOC.Application.Features.Analytics.Queries;

namespace BOC.WebAPI.Controllers;

[Authorize(Roles = "Chairman, System Administrator")]
public class AnalyticsController : ApiControllerBase
{
    [HttpGet("executive")]
    public async Task<ActionResult<AnalyticsDashboardDto>> GetExecutiveDashboard()
    {
        return await Mediator.Send(new GetExecutiveAnalyticsQuery());
    }
}
