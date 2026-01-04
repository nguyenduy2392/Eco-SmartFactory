using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFactory.Application.Commands.ProcessBOM;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Api.Controllers;

[Authorize]
[Route("api/processbom")]
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
    /// Get BOM history for a part + processing type
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetBOMHistory([FromQuery] Guid? partId, [FromQuery] string? processingType)
    {
        if (!partId.HasValue || string.IsNullOrEmpty(processingType))
        {
            return BadRequest(new { message = "PartId and ProcessingType are required" });
        }

        var query = new GetAllProcessBOMQuery
        {
            PartId = partId,
            ProcessingType = processingType
        };
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Delete Process BOM (only if not ACTIVE)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteProcessBOMCommand { Id = id };
        var result = await Mediator.Send(command);
        
        if (!result.Success)
        {
            return BadRequest(new { success = false, message = result.Message });
        }
        
        return Ok(new { success = true, message = result.Message });
    }
}






