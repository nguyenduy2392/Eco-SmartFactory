using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Commands.Warehouse;

/// <summary>
/// Command để tạo phiếu xuất kho nguyên vật liệu
/// Tự động cập nhật tồn kho và tạo lịch sử giao dịch
/// Không cho phép tồn kho âm
/// </summary>
public class CreateMaterialIssueCommand : IRequest<MaterialIssueDto>
{
    public Guid CustomerId { get; set; }
    public Guid MaterialId { get; set; }
    public Guid WarehouseId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string IssueNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CreatedBy { get; set; }
}

public class CreateMaterialIssueCommandHandler : IRequestHandler<CreateMaterialIssueCommand, MaterialIssueDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateMaterialIssueCommandHandler> _logger;

    public CreateMaterialIssueCommandHandler(
        ApplicationDbContext context,
        ILogger<CreateMaterialIssueCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MaterialIssueDto> Handle(CreateMaterialIssueCommand request, CancellationToken cancellationToken)
    {
        // Validate business rules
        if (string.IsNullOrWhiteSpace(request.BatchNumber))
        {
            throw new Exception("BatchNumber is required");
        }

        if (request.Quantity <= 0)
        {
            throw new Exception("Quantity must be greater than 0");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new Exception("Reason is required");
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

        // Check if IssueNumber already exists
        var existingIssue = await _context.MaterialIssues
            .FirstOrDefaultAsync(i => i.IssueNumber == request.IssueNumber, cancellationToken);
        if (existingIssue != null)
        {
            throw new Exception($"IssueNumber {request.IssueNumber} already exists");
        }

        // Get current stock before transaction
        var stockBefore = material.CurrentStock;

        // Validate no negative inventory
        if (stockBefore < request.Quantity)
        {
            throw new Exception(
                $"Insufficient stock. Current stock: {stockBefore} {material.Unit}, Requested: {request.Quantity} {request.Unit}");
        }

        // Create MaterialIssue
        var issue = new MaterialIssue
        {
            CustomerId = request.CustomerId,
            MaterialId = request.MaterialId,
            WarehouseId = request.WarehouseId,
            BatchNumber = request.BatchNumber,
            Quantity = request.Quantity,
            Unit = request.Unit,
            IssueDate = request.IssueDate,
            Reason = request.Reason,
            IssueNumber = request.IssueNumber,
            Notes = request.Notes,
            Status = "ISSUED",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        _context.MaterialIssues.Add(issue);

        // Update Material CurrentStock
        material.CurrentStock -= request.Quantity;
        material.UpdatedAt = DateTime.UtcNow;

        var stockAfter = material.CurrentStock;

        // Create transaction history
        var history = new MaterialTransactionHistory
        {
            CustomerId = request.CustomerId,
            MaterialId = request.MaterialId,
            WarehouseId = request.WarehouseId,
            BatchNumber = request.BatchNumber,
            TransactionType = "ISSUE",
            ReferenceId = issue.Id,
            ReferenceNumber = request.IssueNumber,
            StockBefore = stockBefore,
            QuantityChange = -request.Quantity, // Negative for issue
            StockAfter = stockAfter,
            Unit = request.Unit,
            TransactionDate = request.IssueDate,
            CreatedBy = request.CreatedBy,
            Notes = $"{request.Reason}. {request.Notes}",
            CreatedAt = DateTime.UtcNow
        };

        _context.MaterialTransactionHistories.Add(history);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created MaterialIssue {IssueNumber} for Material {MaterialCode}. Stock: {StockBefore} -> {StockAfter}",
            issue.IssueNumber, material.Code, stockBefore, stockAfter);

        // Load navigation properties
        await _context.Entry(issue).Reference(i => i.Customer).LoadAsync(cancellationToken);
        await _context.Entry(issue).Reference(i => i.Material).LoadAsync(cancellationToken);
        await _context.Entry(issue).Reference(i => i.Warehouse).LoadAsync(cancellationToken);

        return new MaterialIssueDto
        {
            Id = issue.Id,
            CustomerId = issue.CustomerId,
            CustomerName = issue.Customer.Name,
            MaterialId = issue.MaterialId,
            MaterialCode = issue.Material.Code,
            MaterialName = issue.Material.Name,
            WarehouseId = issue.WarehouseId,
            WarehouseCode = issue.Warehouse.Code,
            WarehouseName = issue.Warehouse.Name,
            BatchNumber = issue.BatchNumber,
            Quantity = issue.Quantity,
            Unit = issue.Unit,
            IssueDate = issue.IssueDate,
            Reason = issue.Reason,
            IssueNumber = issue.IssueNumber,
            Notes = issue.Notes,
            Status = issue.Status,
            CreatedAt = issue.CreatedAt,
            UpdatedAt = issue.UpdatedAt,
            CreatedBy = issue.CreatedBy,
            UpdatedBy = issue.UpdatedBy
        };
    }
}

