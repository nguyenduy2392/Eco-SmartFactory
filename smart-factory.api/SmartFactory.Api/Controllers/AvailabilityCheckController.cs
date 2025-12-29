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
        if (request.PurchaseOrderId == Guid.Empty)
        {
            return BadRequest(new { message = "PurchaseOrderId is required" });
        }

        if (request.PlannedQuantity <= 0)
        {
            return BadRequest(new { message = "PlannedQuantity must be > 0" });
        }

        try
        {
            var command = new CheckMaterialAvailabilityCommand
            {
                PurchaseOrderId = request.PurchaseOrderId,
                PlannedQuantity = request.PlannedQuantity
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


