using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BOC.Application.Features.Admin.Queries;

namespace BOC.WebAPI.Controllers;

[Authorize(Roles = "System Administrator")]
public class SystemConfigController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SystemConfigDto>> GetConfig()
    {
        return await Mediator.Send(new GetSystemConfigQuery());
    }
}
