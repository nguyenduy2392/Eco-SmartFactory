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
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? version, [FromQuery] Guid? customerId)
    {
        var query = new GetAllPurchaseOrdersQuery 
        { 
            Status = status,
            Version = version,
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
        try
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
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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
            Notes = request.Notes
        };

        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveVersion(Guid id)
    {
        try
        {
            var command = new ApprovePOVersionCommand { PurchaseOrderId = id };
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("import-excel")]
    public async Task<IActionResult> ImportFromExcel([FromForm] IFormFile file, [FromForm] string poNumber, 
        [FromForm] Guid? customerId, [FromForm] string processingType, [FromForm] DateTime poDate, 
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
            TemplateType = processingType,
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeletePurchaseOrderCommand { Id = id };
            var result = await Mediator.Send(command);
            return Ok(new { success = true, message = "PO đã được xóa thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/general-info")]
    public async Task<IActionResult> UpdateGeneralInfo(Guid id, [FromBody] UpdateGeneralInfoRequest request)
    {
        try
        {
            var command = new UpdatePurchaseOrderGeneralInfoCommand
            {
                Id = id,
                PONumber = request.PONumber,
                CustomerId = request.CustomerId,
                ProcessingType = request.ProcessingType,
                PODate = request.PODate,
                ExpectedDeliveryDate = request.ExpectedDeliveryDate,
                Notes = request.Notes
            };
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/export-operations")]
    public async Task<IActionResult> ExportOperations(Guid id)
    {
        try
        {
            var query = new GetPurchaseOrderByIdQuery { Id = id };
            var po = await Mediator.Send(query);
            
            if (po == null)
            {
                return NotFound(new { error = "PO not found" });
            }

            // TODO: Implement Excel export service
            // For now, return a simple response
            return BadRequest(new { error = "Export functionality not yet implemented" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/operations")]
    public async Task<IActionResult> CreateOperation(Guid id, [FromBody] CreatePOOperationRequest request)
    {
        try
        {
            var command = new CreatePOOperationCommand
            {
                PurchaseOrderId = id,
                PartId = request.PartId,
                ProcessingTypeId = request.ProcessingTypeId,
                ProcessMethodId = request.ProcessMethodId,
                OperationName = request.OperationName,
                ChargeCount = request.ChargeCount,
                UnitPrice = request.UnitPrice,
                Quantity = request.Quantity,
                SprayPosition = request.SprayPosition,
                PrintContent = request.PrintContent,
                CycleTime = request.CycleTime,
                AssemblyContent = request.AssemblyContent,
                CompletionDate = request.CompletionDate,
                Notes = request.Notes,
                SequenceOrder = request.SequenceOrder,
                // Product and Part codes
                ProductCode = request.ProductCode,
                PartCode = request.PartCode,
                // ÉP NHỰA specific fields
                ModelNumber = request.ModelNumber,
                Material = request.Material,
                ColorCode = request.ColorCode,
                Color = request.Color,
                CavityQuantity = request.CavityQuantity,
                Set = request.Set,
                NetWeight = request.NetWeight,
                TotalWeight = request.TotalWeight,
                MachineType = request.MachineType,
                RequiredMaterial = request.RequiredMaterial,
                RequiredColor = request.RequiredColor
            };
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/operations/{operationId}")]
    public async Task<IActionResult> UpdateOperation(Guid id, Guid operationId, [FromBody] UpdatePOOperationRequest request)
    {
        try
        {
            var command = new UpdatePOOperationCommand
            {
                Id = operationId,
                PurchaseOrderId = id,
                OperationName = request.OperationName,
                ChargeCount = request.ChargeCount,
                UnitPrice = request.UnitPrice,
                Quantity = request.Quantity,
                SprayPosition = request.SprayPosition,
                PrintContent = request.PrintContent,
                CycleTime = request.CycleTime,
                AssemblyContent = request.AssemblyContent,
                CompletionDate = request.CompletionDate,
                Notes = request.Notes,
                // Product and Part codes
                ProductCode = request.ProductCode,
                PartCode = request.PartCode,
                // ÉP NHỰA specific fields
                ModelNumber = request.ModelNumber,
                Material = request.Material,
                ColorCode = request.ColorCode,
                Color = request.Color,
                CavityQuantity = request.CavityQuantity,
                Set = request.Set,
                NetWeight = request.NetWeight,
                TotalWeight = request.TotalWeight,
                MachineType = request.MachineType,
                RequiredMaterial = request.RequiredMaterial,
                RequiredColor = request.RequiredColor
            };
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}/operations/{operationId}")]
    public async Task<IActionResult> DeleteOperation(Guid id, Guid operationId)
    {
        try
        {
            var command = new DeletePOOperationCommand
            {
                Id = operationId,
                PurchaseOrderId = id
            };
            await Mediator.Send(command);
            return Ok(new { success = true, message = "Operation deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/products/{productId}")]
    public async Task<IActionResult> UpdateProduct(Guid id, Guid productId, [FromBody] UpdatePOProductRequest request)
    {
        try
        {
            var command = new UpdatePOProductCommand
            {
                Id = productId,
                PurchaseOrderId = id,
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice
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

