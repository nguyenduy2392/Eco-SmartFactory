using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Commands.Warehouse;

public class UpdateMaterialReceiptStatusCommand : IRequest<MaterialReceiptDto>
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UpdateMaterialReceiptStatusCommandHandler : IRequestHandler<UpdateMaterialReceiptStatusCommand, MaterialReceiptDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdateMaterialReceiptStatusCommandHandler> _logger;

    public UpdateMaterialReceiptStatusCommandHandler(
        ApplicationDbContext context,
        ILogger<UpdateMaterialReceiptStatusCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MaterialReceiptDto> Handle(UpdateMaterialReceiptStatusCommand request, CancellationToken cancellationToken)
    {
        var receipt = await _context.MaterialReceipts
            .Include(mr => mr.Customer)
            .Include(mr => mr.Material)
            .Include(mr => mr.Warehouse)
            .FirstOrDefaultAsync(mr => mr.Id == request.Id, cancellationToken);

        if (receipt == null)
        {
            throw new Exception($"Material receipt with ID {request.Id} not found");
        }

        receipt.Status = request.Status;
        receipt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated MaterialReceipt {ReceiptNumber} status to {Status}",
            receipt.ReceiptNumber, request.Status);

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

