using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.Materials;

public class GetMaterialByIdQuery : IRequest<MaterialDto?>
{
    public Guid Id { get; set; }
}

public class GetMaterialByIdQueryHandler : IRequestHandler<GetMaterialByIdQuery, MaterialDto?>
{
    private readonly ApplicationDbContext _context;

    public GetMaterialByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MaterialDto?> Handle(GetMaterialByIdQuery request, CancellationToken cancellationToken)
    {
        var material = await _context.Materials
            .Include(m => m.Customer)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (material == null)
        {
            return null;
        }

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

