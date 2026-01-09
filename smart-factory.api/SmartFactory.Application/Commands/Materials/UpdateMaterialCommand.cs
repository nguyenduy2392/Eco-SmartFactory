using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Commands.Materials;

public class UpdateMaterialCommand : IRequest<MaterialDto>
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ColorCode { get; set; }
    public string? Supplier { get; set; }
    public string Unit { get; set; } = "kg";
    public decimal CurrentStock { get; set; }
    public decimal MinStock { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateMaterialCommandHandler : IRequestHandler<UpdateMaterialCommand, MaterialDto>
{
    private readonly ApplicationDbContext _context;

    public UpdateMaterialCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MaterialDto> Handle(UpdateMaterialCommand request, CancellationToken cancellationToken)
    {
        var material = await _context.Materials
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (material == null)
        {
            throw new Exception($"Material with ID {request.Id} not found");
        }

        // Check if code already exists for this customer (if code changed)
        if (material.Code != request.Code)
        {
            var existingMaterial = await _context.Materials
                .FirstOrDefaultAsync(m => m.Code == request.Code && m.CustomerId == material.CustomerId && m.Id != request.Id, cancellationToken);

            if (existingMaterial != null)
            {
                throw new Exception($"Material with code {request.Code} already exists for this customer");
            }
        }

        material.Code = request.Code;
        material.Name = request.Name;
        material.Type = request.Type;
        material.ColorCode = request.ColorCode;
        material.Supplier = request.Supplier;
        material.Unit = request.Unit;
        material.CurrentStock = request.CurrentStock;
        material.MinStock = request.MinStock;
        material.Description = request.Description;
        material.IsActive = request.IsActive;
        material.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Reload to include customer
        await _context.Entry(material).Reference(m => m.Customer).LoadAsync(cancellationToken);

        return new MaterialDto
        {
            Id = material.Id,
            Code = material.Code,
            Name = material.Name,
            Type = material.Type,
            ColorCode = material.ColorCode,
            Supplier = material.Supplier,
            Unit = material.Unit,
            CurrentStock = material.CurrentStock,
            MinStock = material.MinStock,
            Description = material.Description,
            IsActive = material.IsActive,
            CreatedAt = material.CreatedAt,
            CustomerId = material.CustomerId,
            CustomerCode = material.Customer?.Code,
            CustomerName = material.Customer?.Name
        };
    }
}

