using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BOC.Application.Features.Research.Commands;
using BOC.Application.Features.Research.Queries;

namespace BOC.WebAPI.Controllers;

public class ResearchController : ApiControllerBase
{
    [HttpGet("{id}/document")]
    public async Task<IActionResult> GetDocument(Guid id)
    {
        var result = await Mediator.Send(new GetResearchDocumentQuery(id));
        return File(result.Stream, result.ContentType, result.FileName);
    }

    [HttpPost("submit")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Guid>> Submit(
        [FromForm] string title,
        [FromForm] string? @abstract,
        [FromForm] Guid? categoryId,
        [FromForm] Guid researcherId,
        [FromForm] Guid departmentId,
        [FromForm] Guid directorateId,
        [FromForm] bool submitImmediately,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        using var stream = file.OpenReadStream();
        var command = new SubmitResearchCommand(
            title,
            @abstract,
            categoryId,
            researcherId,
            departmentId,
            directorateId,
            file.FileName,
            stream,
            file.ContentType,
            submitImmediately);

        return await Mediator.Send(command);
    }
}
