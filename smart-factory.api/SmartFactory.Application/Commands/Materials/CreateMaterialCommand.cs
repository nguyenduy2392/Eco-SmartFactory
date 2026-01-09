using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Commands.Materials;

public class CreateMaterialCommand : IRequest<MaterialDto>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ColorCode { get; set; }
    public string? Supplier { get; set; }
    public string Unit { get; set; } = "kg";
    public decimal CurrentStock { get; set; }
    public decimal MinStock { get; set; }
    public string? Description { get; set; }
    public Guid? CustomerId { get; set; }
}

public class CreateMaterialCommandHandler : IRequestHandler<CreateMaterialCommand, MaterialDto>
{
    private readonly ApplicationDbContext _context;

    public CreateMaterialCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MaterialDto> Handle(CreateMaterialCommand request, CancellationToken cancellationToken)
    {
        // Validate customer exists if CustomerId is provided
        Customer? customer = null;
        if (request.CustomerId.HasValue)
        {
            customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == request.CustomerId.Value, cancellationToken);

            if (customer == null)
            {
                throw new Exception($"Customer with ID {request.CustomerId} not found");
            }
        }

        // Check if code already exists
        var existingMaterial = await _context.Materials
            .FirstOrDefaultAsync(m => m.Code == request.Code, cancellationToken);

        if (existingMaterial != null)
        {
            throw new Exception($"Material with code {request.Code} already exists");
        }

        var material = new Material
        {
            Code = request.Code,
            Name = request.Name,
            Type = request.Type,
            ColorCode = request.ColorCode,
            Supplier = request.Supplier,
            Unit = request.Unit,
            CurrentStock = request.CurrentStock,
            MinStock = request.MinStock,
            Description = request.Description,
            CustomerId = request.CustomerId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Materials.Add(material);
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

