using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.Warehouse;

public class GetMaterialAdjustmentByIdQuery : IRequest<MaterialAdjustmentDto?>
{
    public Guid Id { get; set; }
}

public class GetMaterialAdjustmentByIdQueryHandler : IRequestHandler<GetMaterialAdjustmentByIdQuery, MaterialAdjustmentDto?>
{
    private readonly ApplicationDbContext _context;

    public GetMaterialAdjustmentByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MaterialAdjustmentDto?> Handle(GetMaterialAdjustmentByIdQuery request, CancellationToken cancellationToken)
    {
        var adjustment = await _context.MaterialAdjustments
            .Include(ma => ma.Customer)
            .Include(ma => ma.Material)
            .Include(ma => ma.Warehouse)
            .FirstOrDefaultAsync(ma => ma.Id == request.Id, cancellationToken);

        if (adjustment == null)
        {
            return null;
        }

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

