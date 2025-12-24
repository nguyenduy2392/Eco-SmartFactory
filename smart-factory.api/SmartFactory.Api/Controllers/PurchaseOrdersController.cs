using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFactory.Application.Commands.Customers;
using SmartFactory.Application.Commands.PurchaseOrders;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Queries.PurchaseOrders;

namespace SmartFactory.Api.Controllers;

[Authorize]
public class PurchaseOrdersController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? versionType, [FromQuery] Guid? customerId)
    {
        var query = new GetAllPurchaseOrdersQuery 
        { 
            Status = status,
            VersionType = versionType,
            CustomerId = customerId
        };
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetPurchaseOrderByIdQuery { Id = id };
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequest request)
    {
        var command = new CreatePurchaseOrderCommand
        {
            PONumber = request.PONumber,
            CustomerId = request.CustomerId,
            TemplateType = request.TemplateType,
            PODate = request.PODate,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            Notes = request.Notes,
            Products = request.Products
        };

        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseOrderRequest request)
    {
        var command = new UpdatePurchaseOrderCommand
        {
            Id = id,
            CustomerId = request.CustomerId,
            PODate = request.PODate,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            Status = request.Status,
            Notes = request.Notes
        };

        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("clone-version")]
    public async Task<IActionResult> CloneVersion([FromBody] ClonePOVersionRequest request)
    {
        var command = new ClonePOVersionCommand
        {
            OriginalPOId = request.OriginalPOId,
            NewVersionType = request.NewVersionType,
            Notes = request.Notes
        };

        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("import-excel")]
    public async Task<IActionResult> ImportFromExcel([FromForm] IFormFile file, [FromForm] string poNumber, 
        [FromForm] Guid? customerId, [FromForm] string templateType, [FromForm] DateTime poDate, 
        [FromForm] DateTime? expectedDeliveryDate, [FromForm] string? notes,
        [FromForm] string? customerName, [FromForm] string? customerCode)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required");
        }

        if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
        {
            return BadRequest("Only Excel files (.xlsx, .xls) are allowed");
        }

        using var stream = file.OpenReadStream();
        
        var command = new ImportPOFromExcelCommand
        {
            FileStream = stream,
            PONumber = poNumber,
            CustomerId = customerId,
            TemplateType = templateType,
            PODate = poDate,
            ExpectedDeliveryDate = expectedDeliveryDate,
            Notes = notes,
            CustomerName = customerName,
            CustomerCode = customerCode
        };

        try
        {
            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

