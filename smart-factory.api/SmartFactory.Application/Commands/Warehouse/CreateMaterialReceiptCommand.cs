using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Commands.Warehouse;

/// <summary>
/// Command để tạo phiếu nhập kho nguyên vật liệu
/// Tự động cập nhật tồn kho và tạo lịch sử giao dịch
/// Nếu MaterialId = Guid.Empty, sẽ tự động tạo nguyên vật liệu mới từ NewMaterialInfo
/// </summary>
public class CreateMaterialReceiptCommand : IRequest<MaterialReceiptDto>
{
    public Guid CustomerId { get; set; }
    public Guid MaterialId { get; set; } // Nếu = Guid.Empty, sẽ tạo mới từ NewMaterialInfo
    public Guid WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public string? SupplierCode { get; set; }
    public string? PurchasePOCode { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CreatedBy { get; set; }
    
    // Thông tin để tạo nguyên vật liệu mới (nếu MaterialId = Guid.Empty)
    public NewMaterialInfo? NewMaterialInfo { get; set; }
}

public class NewMaterialInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ColorCode { get; set; }
    public string? Supplier { get; set; }
    public decimal MinStock { get; set; }
    public string? Description { get; set; }
}

public class CreateMaterialReceiptCommandHandler : IRequestHandler<CreateMaterialReceiptCommand, MaterialReceiptDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateMaterialReceiptCommandHandler> _logger;

    public CreateMaterialReceiptCommandHandler(
        ApplicationDbContext context,
        ILogger<CreateMaterialReceiptCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MaterialReceiptDto> Handle(CreateMaterialReceiptCommand request, CancellationToken cancellationToken)
    {
        // Validate business rules
        if (request.Quantity <= 0)
        {
            throw new Exception("Quantity must be greater than 0");
        }

        // Validate entities exist
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new Exception($"Customer with ID {request.CustomerId} not found");
        }

        // Nếu MaterialId = Guid.Empty, tạo nguyên vật liệu mới
        Material? material;
        if (request.MaterialId == Guid.Empty)
        {
            // Validate NewMaterialInfo
            if (request.NewMaterialInfo == null || 
                string.IsNullOrWhiteSpace(request.NewMaterialInfo.Code) ||
                string.IsNullOrWhiteSpace(request.NewMaterialInfo.Name))
            {
                throw new Exception("NewMaterialInfo is required when MaterialId is empty. Code and Name are required.");
            }

            // Kiểm tra xem mã nguyên vật liệu đã tồn tại chưa
            var existingMaterial = await _context.Materials
                .FirstOrDefaultAsync(m => m.Code == request.NewMaterialInfo.Code 
                    && m.CustomerId == request.CustomerId, cancellationToken);
            
            if (existingMaterial != null)
            {
                throw new Exception($"Material with code {request.NewMaterialInfo.Code} already exists for this customer");
            }

            // Tạo nguyên vật liệu mới
            material = new Material
            {
                Code = request.NewMaterialInfo.Code,
                Name = request.NewMaterialInfo.Name,
                Type = request.NewMaterialInfo.Type ?? "OTHER",
                ColorCode = request.NewMaterialInfo.ColorCode,
                Supplier = request.NewMaterialInfo.Supplier,
                Unit = request.Unit,
                CurrentStock = 0, // Sẽ được cập nhật sau khi nhập kho
                MinStock = request.NewMaterialInfo.MinStock,
                Description = request.NewMaterialInfo.Description,
                CustomerId = request.CustomerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Materials.Add(material);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation(
                "Created new Material {MaterialCode} ({MaterialName}) for Customer {CustomerId}",
                material.Code, material.Name, request.CustomerId);
        }
        else
        {
            // Sử dụng nguyên vật liệu đã có
            material = await _context.Materials
                .FirstOrDefaultAsync(m => m.Id == request.MaterialId, cancellationToken);
            if (material == null)
            {
                throw new Exception($"Material with ID {request.MaterialId} not found");
            }

            if (material.CustomerId != request.CustomerId)
            {
                throw new Exception("Material does not belong to the specified customer");
            }
        }

        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId, cancellationToken);
        if (warehouse == null)
        {
            throw new Exception($"Warehouse with ID {request.WarehouseId} not found");
        }

        // Check if ReceiptNumber already exists
        var existingReceipt = await _context.MaterialReceipts
            .FirstOrDefaultAsync(r => r.ReceiptNumber == request.ReceiptNumber, cancellationToken);
        if (existingReceipt != null)
        {
            throw new Exception($"ReceiptNumber {request.ReceiptNumber} already exists");
        }

        // Get current stock before transaction
        var stockBefore = material.CurrentStock;

        // Chủ hàng chính là nhà cung cấp, tự động set SupplierCode = Customer.Code
        var supplierCode = !string.IsNullOrWhiteSpace(request.SupplierCode) 
            ? request.SupplierCode 
            : customer.Code;

        // Create MaterialReceipt
        var receipt = new MaterialReceipt
        {
            CustomerId = request.CustomerId,
            MaterialId = material.Id, // Sử dụng material.Id (có thể là mới tạo hoặc đã có)
            WarehouseId = request.WarehouseId,
            Quantity = request.Quantity,
            Unit = request.Unit,
            BatchNumber = request.BatchNumber,
            ReceiptDate = request.ReceiptDate,
            SupplierCode = supplierCode, // Tự động set từ Customer.Code
            PurchasePOCode = null, // Không cần PO mua hàng
            ReceiptNumber = request.ReceiptNumber,
            Notes = request.Notes,
            Status = "RECEIVED",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        _context.MaterialReceipts.Add(receipt);

        // Update Material CurrentStock
        material.CurrentStock += request.Quantity;
        material.UpdatedAt = DateTime.UtcNow;

        var stockAfter = material.CurrentStock;

        // Create transaction history
        var history = new MaterialTransactionHistory
        {
            CustomerId = request.CustomerId,
            MaterialId = material.Id, // Sử dụng material.Id
            WarehouseId = request.WarehouseId,
            BatchNumber = request.BatchNumber,
            TransactionType = "RECEIPT",
            ReferenceId = receipt.Id,
            ReferenceNumber = request.ReceiptNumber,
            StockBefore = stockBefore,
            QuantityChange = request.Quantity,
            StockAfter = stockAfter,
            Unit = request.Unit,
            TransactionDate = request.ReceiptDate,
            CreatedBy = request.CreatedBy,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.MaterialTransactionHistories.Add(history);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created MaterialReceipt {ReceiptNumber} for Material {MaterialCode}. Stock: {StockBefore} -> {StockAfter}",
            receipt.ReceiptNumber, material.Code, stockBefore, stockAfter);

        // Load navigation properties
        await _context.Entry(receipt).Reference(r => r.Customer).LoadAsync(cancellationToken);
        await _context.Entry(receipt).Reference(r => r.Material).LoadAsync(cancellationToken);
        await _context.Entry(receipt).Reference(r => r.Warehouse).LoadAsync(cancellationToken);

        return new MaterialReceiptDto
        {
            Id = receipt.Id,
            CustomerId = receipt.CustomerId,
            CustomerName = receipt.Customer.Name,
            MaterialId = receipt.MaterialId,
            MaterialCode = receipt.Material.Code,
            MaterialName = receipt.Material.Name,
            WarehouseId = receipt.WarehouseId,
            WarehouseCode = receipt.Warehouse.Code,
            WarehouseName = receipt.Warehouse.Name,
            Quantity = receipt.Quantity,
            Unit = receipt.Unit,
            BatchNumber = receipt.BatchNumber,
            ReceiptDate = receipt.ReceiptDate,
            SupplierCode = receipt.SupplierCode,
            PurchasePOCode = receipt.PurchasePOCode,
            ReceiptNumber = receipt.ReceiptNumber,
            Notes = receipt.Notes,
            Status = receipt.Status,
            CreatedAt = receipt.CreatedAt,
            UpdatedAt = receipt.UpdatedAt,
            CreatedBy = receipt.CreatedBy,
            UpdatedBy = receipt.UpdatedBy
        };
    }
}

