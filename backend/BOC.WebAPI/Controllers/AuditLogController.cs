using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BOC.Application.Features.Admin.Queries;

namespace BOC.WebAPI.Controllers;

[Authorize(Roles = "System Administrator")]
public class AuditLogController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AuditLogDto>>> GetLogs()
    {
        return await Mediator.Send(new GetAuditLogsQuery());
    }
}
