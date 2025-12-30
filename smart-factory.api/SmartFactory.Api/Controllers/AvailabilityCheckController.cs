using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFactory.Application.Commands.AvailabilityCheck;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Api.Controllers;

[Authorize]
public class AvailabilityCheckController : BaseApiController
{
    /// <summary>
    /// Check material availability for production planning
    /// PHASE 1: Used to decide if PMC can plan production
    /// Only works with APPROVED_FOR_PMC PO versions
    /// </summary>
    [HttpPost("check")]
    public async Task<IActionResult> CheckAvailability([FromBody] AvailabilityCheckRequest request)
    {
        if (request.PurchaseOrderId == null || request.PurchaseOrderId == Guid.Empty)
        {
            return BadRequest(new { message = "PurchaseOrderId is required" });
        }

        if (request.PlannedQuantity == null || request.PlannedQuantity <= 0)
        {
            return BadRequest(new { message = "PlannedQuantity must be > 0" });
        }

        try
        {
            var command = new CheckMaterialAvailabilityCommand
            {
                PurchaseOrderId = request.PurchaseOrderId.Value,
                PlannedQuantity = request.PlannedQuantity.Value
            };

            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Check component availability (not PO-based)
    /// Check if a specific part with processing type can be produced
    /// </summary>
    [HttpPost("check-by-component")]
    public async Task<IActionResult> CheckAvailabilityByComponent([FromBody] AvailabilityCheckRequest request)
    {
        if (request.PartId == null || request.PartId == Guid.Empty)
        {
            return BadRequest(new { message = "PartId is required" });
        }

        if (request.ProcessingTypeId == null || request.ProcessingTypeId == Guid.Empty)
        {
            return BadRequest(new { message = "ProcessingTypeId is required" });
        }

        if (request.Quantity == null || request.Quantity <= 0)
        {
            return BadRequest(new { message = "Quantity must be > 0" });
        }

        try
        {
            var command = new CheckComponentAvailabilityCommand
            {
                PartId = request.PartId.Value,
                ProcessingTypeId = request.ProcessingTypeId.Value,
                Quantity = request.Quantity.Value
            };

            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}


