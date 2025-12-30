using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFactory.Application.Commands.ProcessBOM;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Api.Controllers;

[Authorize]
public class ProcessBOMController : BaseApiController
{
    /// <summary>
    /// Get all Process BOMs with optional filters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? partId, [FromQuery] string? processingType, [FromQuery] string? status)
    {
        var query = new GetAllProcessBOMQuery
        {
            PartId = partId,
            ProcessingType = processingType,
            Status = status
        };
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create new Process BOM
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProcessBOMRequest request)
    {
        var command = new CreateProcessBOMCommand
        {
            PartId = request.PartId,
            ProcessingTypeId = request.ProcessingTypeId,
            EffectiveDate = request.EffectiveDate,
            Name = request.Name,
            Notes = request.Notes,
            Details = request.Details
        };

        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get Process BOM by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetProcessBOMByIdQuery { Id = id };
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Get ACTIVE BOM for a (Part + ProcessingType)
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveBOM([FromQuery] Guid partId, [FromQuery] Guid processingTypeId)
    {
        if (partId == Guid.Empty || processingTypeId == Guid.Empty)
        {
            return BadRequest(new { message = "PartId and ProcessingTypeId are required" });
        }

        var query = new GetActiveBOMByPartAndTypeQuery
        {
            PartId = partId,
            ProcessingTypeId = processingTypeId
        };

        var result = await Mediator.Send(query);
        return HandleResult(result);
    }
}



