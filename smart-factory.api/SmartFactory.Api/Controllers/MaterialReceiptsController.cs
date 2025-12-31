using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFactory.Application.Commands.Warehouse;
using SmartFactory.Application.Queries.Warehouse;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/material-receipts")]
public class MaterialReceiptsController : BaseApiController
{
    /// <summary>
    /// Lấy danh sách phiếu nhập kho (có thể filter theo customer, material, hoặc status)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? materialId,
        [FromQuery] string? status)
    {
        var query = new GetMaterialReceiptsQuery
        {
            CustomerId = customerId,
            MaterialId = materialId,
            Status = status
        };
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết phiếu nhập kho theo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetMaterialReceiptByIdQuery { Id = id };
        var result = await Mediator.Send(query);
        
        if (result == null)
        {
            return NotFound(new { message = $"Material receipt with ID {id} not found" });
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Tạo phiếu nhập kho mới
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaterialReceiptCommand command)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                command.CreatedBy = userId.Value.ToString();
            }

            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Xác nhận nhập kho (chuyển status từ PENDING sang RECEIVED)
    /// </summary>
    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> ConfirmReceipt(Guid id)
    {
        try
        {
            // Get the receipt first
            var getQuery = new GetMaterialReceiptByIdQuery { Id = id };
            var receipt = await Mediator.Send(getQuery);
            
            if (receipt == null)
            {
                return NotFound(new { message = $"Material receipt with ID {id} not found" });
            }

            if (receipt.Status == "RECEIVED")
            {
                return BadRequest(new { error = "Receipt is already confirmed" });
            }

            // For now, we'll need to create an UpdateMaterialReceiptCommand
            // Since we don't have one yet, we'll use a simple approach
            // This should ideally be a separate command
            var updateCommand = new UpdateMaterialReceiptStatusCommand
            {
                Id = id,
                Status = "RECEIVED"
            };
            
            var result = await Mediator.Send(updateCommand);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

