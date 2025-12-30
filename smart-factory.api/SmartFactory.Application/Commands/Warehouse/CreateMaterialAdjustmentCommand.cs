using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Commands.Warehouse;

/// <summary>
/// Command để tạo phiếu điều chỉnh kho nguyên vật liệu
/// Tự động cập nhật tồn kho và tạo lịch sử giao dịch
/// Bắt buộc có lý do và người chịu trách nhiệm
/// Không cho phép tồn kho âm sau điều chỉnh
/// </summary>
public class CreateMaterialAdjustmentCommand : IRequest<MaterialAdjustmentDto>
{
    public Guid CustomerId { get; set; }
    public Guid MaterialId { get; set; }
    public Guid WarehouseId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public decimal AdjustmentQuantity { get; set; } // Có thể âm hoặc dương
    public string Unit { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ResponsiblePerson { get; set; } = string.Empty;
    public string AdjustmentNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CreatedBy { get; set; }
}

public class CreateMaterialAdjustmentCommandHandler : IRequestHandler<CreateMaterialAdjustmentCommand, MaterialAdjustmentDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateMaterialAdjustmentCommandHandler> _logger;

    public CreateMaterialAdjustmentCommandHandler(
        ApplicationDbContext context,
        ILogger<CreateMaterialAdjustmentCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MaterialAdjustmentDto> Handle(CreateMaterialAdjustmentCommand request, CancellationToken cancellationToken)
    {
        // Validate business rules
        if (string.IsNullOrWhiteSpace(request.BatchNumber))
        {
            throw new Exception("BatchNumber is required");
        }

        if (request.AdjustmentQuantity == 0)
        {
            throw new Exception("AdjustmentQuantity cannot be 0");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new Exception("Reason is required");
        }

        if (string.IsNullOrWhiteSpace(request.ResponsiblePerson))
        {
            throw new Exception("ResponsiblePerson is required");
        }

        // Validate entities exist
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new Exception($"Customer with ID {request.CustomerId} not found");
        }

        var material = await _context.Materials
            .FirstOrDefaultAsync(m => m.Id == request.MaterialId, cancellationToken);
        if (material == null)
        {
            throw new Exception($"Material with ID {request.MaterialId} not found");
        }

        if (material.CustomerId != request.CustomerId)
        {
            throw new Exception("Material does not belong to the specified customer");
        }

        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId, cancellationToken);
        if (warehouse == null)
        {
            throw new Exception($"Warehouse with ID {request.WarehouseId} not found");
        }

        // Check if AdjustmentNumber already exists
        var existingAdjustment = await _context.MaterialAdjustments
            .FirstOrDefaultAsync(a => a.AdjustmentNumber == request.AdjustmentNumber, cancellationToken);
        if (existingAdjustment != null)
        {
            throw new Exception($"AdjustmentNumber {request.AdjustmentNumber} already exists");
        }

        // Get current stock before transaction
        var stockBefore = material.CurrentStock;
        var stockAfter = stockBefore + request.AdjustmentQuantity;

        // Validate no negative inventory after adjustment
        if (stockAfter < 0)
        {
            throw new Exception(
                $"Adjustment would result in negative stock. Current stock: {stockBefore} {material.Unit}, Adjustment: {request.AdjustmentQuantity} {request.Unit}, Result: {stockAfter} {request.Unit}");
        }

        // Create MaterialAdjustment
        var adjustment = new MaterialAdjustment
        {
            CustomerId = request.CustomerId,
            MaterialId = request.MaterialId,
            WarehouseId = request.WarehouseId,
            BatchNumber = request.BatchNumber,
            AdjustmentQuantity = request.AdjustmentQuantity,
            Unit = request.Unit,
            AdjustmentDate = request.AdjustmentDate,
            Reason = request.Reason,
            ResponsiblePerson = request.ResponsiblePerson,
            AdjustmentNumber = request.AdjustmentNumber,
            Notes = request.Notes,
            Status = "APPROVED",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        _context.MaterialAdjustments.Add(adjustment);

        // Update Material CurrentStock
        material.CurrentStock = stockAfter;
        material.UpdatedAt = DateTime.UtcNow;

        // Create transaction history
        var history = new MaterialTransactionHistory
        {
            CustomerId = request.CustomerId,
            MaterialId = request.MaterialId,
            WarehouseId = request.WarehouseId,
            BatchNumber = request.BatchNumber,
            TransactionType = "ADJUSTMENT",
            ReferenceId = adjustment.Id,
            ReferenceNumber = request.AdjustmentNumber,
            StockBefore = stockBefore,
            QuantityChange = request.AdjustmentQuantity,
            StockAfter = stockAfter,
            Unit = request.Unit,
            TransactionDate = request.AdjustmentDate,
            CreatedBy = request.CreatedBy,
            Notes = $"Reason: {request.Reason}. Responsible: {request.ResponsiblePerson}. {request.Notes}",
            CreatedAt = DateTime.UtcNow
        };

        _context.MaterialTransactionHistories.Add(history);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created MaterialAdjustment {AdjustmentNumber} for Material {MaterialCode}. Stock: {StockBefore} -> {StockAfter}. Responsible: {ResponsiblePerson}",
            adjustment.AdjustmentNumber, material.Code, stockBefore, stockAfter, request.ResponsiblePerson);

        // Load navigation properties
        await _context.Entry(adjustment).Reference(a => a.Customer).LoadAsync(cancellationToken);
        await _context.Entry(adjustment).Reference(a => a.Material).LoadAsync(cancellationToken);
        await _context.Entry(adjustment).Reference(a => a.Warehouse).LoadAsync(cancellationToken);

        return new MaterialAdjustmentDto
        {
            Id = adjustment.Id,
            CustomerId = adjustment.CustomerId,
            CustomerName = adjustment.Customer.Name,
            MaterialId = adjustment.MaterialId,
            MaterialCode = adjustment.Material.Code,
            MaterialName = adjustment.Material.Name,
            WarehouseId = adjustment.WarehouseId,
            WarehouseCode = adjustment.Warehouse.Code,
            WarehouseName = adjustment.Warehouse.Name,
            BatchNumber = adjustment.BatchNumber,
            AdjustmentQuantity = adjustment.AdjustmentQuantity,
            Unit = adjustment.Unit,
            AdjustmentDate = adjustment.AdjustmentDate,
            Reason = adjustment.Reason,
            ResponsiblePerson = adjustment.ResponsiblePerson,
            AdjustmentNumber = adjustment.AdjustmentNumber,
            Notes = adjustment.Notes,
            Status = adjustment.Status,
            CreatedAt = adjustment.CreatedAt,
            UpdatedAt = adjustment.UpdatedAt,
            CreatedBy = adjustment.CreatedBy,
            UpdatedBy = adjustment.UpdatedBy
        };
    }
}

