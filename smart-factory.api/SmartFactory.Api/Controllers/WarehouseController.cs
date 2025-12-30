using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFactory.Application.Commands.Warehouse;
using SmartFactory.Application.Queries.Warehouse;
using SmartFactory.Application.Services;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace SmartFactory.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WarehouseController : BaseApiController
{
    private readonly WarehouseExcelService _excelService;
    private readonly ApplicationDbContext _context;

    public WarehouseController(WarehouseExcelService excelService, ApplicationDbContext context)
    {
        _excelService = excelService;
        _context = context;
    }
    /// <summary>
    /// Lấy danh sách tất cả kho
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllWarehouses([FromQuery] bool? isActive)
    {
        var query = new GetAllWarehousesQuery { IsActive = isActive };
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Nhập kho nguyên vật liệu
    /// </summary>
    [HttpPost("receipt")]
    public async Task<IActionResult> CreateMaterialReceipt([FromBody] CreateMaterialReceiptCommand command)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                command.CreatedBy = userId.Value.ToString();
            }

            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetMaterialReceipt), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Xuất kho nguyên vật liệu
    /// </summary>
    [HttpPost("issue")]
    public async Task<IActionResult> CreateMaterialIssue([FromBody] CreateMaterialIssueCommand command)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                command.CreatedBy = userId.Value.ToString();
            }

            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetMaterialIssue), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Điều chỉnh kho nguyên vật liệu
    /// </summary>
    [HttpPost("adjustment")]
    public async Task<IActionResult> CreateMaterialAdjustment([FromBody] CreateMaterialAdjustmentCommand command)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                command.CreatedBy = userId.Value.ToString();
            }

            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetMaterialAdjustment), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy lịch sử giao dịch kho
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetTransactionHistory(
        [FromQuery] Guid? materialId,
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? warehouseId,
        [FromQuery] string? batchNumber,
        [FromQuery] string? transactionType,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize)
    {
        var query = new GetMaterialTransactionHistoryQuery
        {
            MaterialId = materialId,
            CustomerId = customerId,
            WarehouseId = warehouseId,
            BatchNumber = batchNumber,
            TransactionType = transactionType,
            FromDate = fromDate,
            ToDate = toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin tồn kho của nguyên vật liệu
    /// </summary>
    [HttpGet("stock/{materialId}")]
    public async Task<IActionResult> GetMaterialStock(Guid materialId, [FromQuery] Guid? warehouseId)
    {
        try
        {
            var query = new GetMaterialStockQuery
            {
                MaterialId = materialId,
                WarehouseId = warehouseId
            };
            var result = await Mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Import nhập kho từ Excel
    /// </summary>
    [HttpPost("import-receipts")]
    public async Task<IActionResult> ImportMaterialReceipts([FromForm] IFormFile file, [FromForm] Guid customerId)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "File không được để trống" });
            }

            using var stream = file.OpenReadStream();
            var importResult = await _excelService.ImportMaterialReceiptsFromExcel(stream, customerId);

            if (!importResult.Success)
            {
                return BadRequest(new { error = importResult.ErrorMessage, errors = importResult.Errors });
            }

            // Process each receipt
            var createdReceipts = new List<MaterialReceiptDto>();
            foreach (var receiptData in importResult.Receipts)
            {
                // Find or create Material
                var material = await _context.Materials
                    .FirstOrDefaultAsync(m => m.Code == receiptData.MaterialCode && m.CustomerId == customerId);

                if (material == null)
                {
                    // Create Material
                    material = new Material
                    {
                        Code = receiptData.MaterialCode,
                        Name = receiptData.MaterialName,
                        Type = receiptData.MaterialType ?? "Unknown",
                        Unit = receiptData.Unit,
                        CustomerId = customerId,
                        Supplier = receiptData.SupplierCode,
                        CurrentStock = 0,
                        MinStock = 0,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Materials.Add(material);
                    await _context.SaveChangesAsync();
                }

                // Find or create Warehouse
                var warehouse = await _context.Warehouses
                    .FirstOrDefaultAsync(w => w.Code == receiptData.WarehouseCode);

                if (warehouse == null)
                {
                    warehouse = new Warehouse
                    {
                        Code = receiptData.WarehouseCode,
                        Name = $"Kho {receiptData.WarehouseCode}",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Warehouses.Add(warehouse);
                    await _context.SaveChangesAsync();
                }

                // Create receipt using command
                var command = new CreateMaterialReceiptCommand
                {
                    CustomerId = customerId,
                    MaterialId = material.Id,
                    WarehouseId = warehouse.Id,
                    Quantity = receiptData.Quantity,
                    Unit = receiptData.Unit,
                    BatchNumber = receiptData.BatchNumber,
                    ReceiptDate = receiptData.ReceiptDate,
                    SupplierCode = receiptData.SupplierCode,
                    PurchasePOCode = receiptData.PurchasePOCode,
                    ReceiptNumber = receiptData.ReceiptNumber,
                    Notes = receiptData.Notes,
                    CreatedBy = GetCurrentUserId()?.ToString()
                };

                var receipt = await Mediator.Send(command);
                createdReceipts.Add(receipt);
            }

            return Ok(new { 
                success = true, 
                message = $"Đã import thành công {createdReceipts.Count} phiếu nhập kho",
                receipts = createdReceipts,
                errors = importResult.Errors
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Export lịch sử giao dịch kho ra Excel
    /// </summary>
    [HttpGet("export-history")]
    public async Task<IActionResult> ExportTransactionHistory(
        [FromQuery] Guid? materialId,
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? warehouseId,
        [FromQuery] string? batchNumber,
        [FromQuery] string? transactionType,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        try
        {
            var query = new GetMaterialTransactionHistoryQuery
            {
                MaterialId = materialId,
                CustomerId = customerId,
                WarehouseId = warehouseId,
                BatchNumber = batchNumber,
                TransactionType = transactionType,
                FromDate = fromDate,
                ToDate = toDate
            };
            var history = await Mediator.Send(query);

            var excelBytes = await _excelService.ExportTransactionHistoryToExcel(history);

            var fileName = $"Lich_su_kho_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Placeholder methods for CreatedAtAction
    private IActionResult GetMaterialReceipt(Guid id) => Ok();
    private IActionResult GetMaterialIssue(Guid id) => Ok();
    private IActionResult GetMaterialAdjustment(Guid id) => Ok();
}

