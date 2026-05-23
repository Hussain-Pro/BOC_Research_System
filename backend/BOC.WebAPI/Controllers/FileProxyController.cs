using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BOC.Application.Features.Files.Queries;

namespace BOC.WebAPI.Controllers;

[AllowAnonymous] // Uses token in URL for streaming access
public class FileProxyController : ApiControllerBase
{
    [HttpGet("stream/{token}")]
    public async Task<IActionResult> StreamFile(string token)
    {
        var result = await Mediator.Send(new GetFileStreamQuery(token));
        
        if (result == null) return NotFound();

        return File(result.Stream, result.ContentType, result.FileName);
    }
}
