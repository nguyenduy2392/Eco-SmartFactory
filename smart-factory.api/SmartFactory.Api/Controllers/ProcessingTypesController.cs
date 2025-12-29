using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFactory.Application.Queries.ProcessingTypes;

namespace SmartFactory.Api.Controllers;

[Authorize]
[Route("api/processing-types")]
public class ProcessingTypesController : BaseApiController
{
    /// <summary>
    /// Get all active Processing Types
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllProcessingTypesQuery();
        var result = await Mediator.Send(query);
        return Ok(result);
    }
}

