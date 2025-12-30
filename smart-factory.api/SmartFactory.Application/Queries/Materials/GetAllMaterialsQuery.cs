using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.Materials;

public class GetAllMaterialsQuery : IRequest<List<MaterialDto>>
{
    public bool? IsActive { get; set; }
    public Guid? CustomerId { get; set; }
}

public class GetAllMaterialsQueryHandler : IRequestHandler<GetAllMaterialsQuery, List<MaterialDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllMaterialsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MaterialDto>> Handle(GetAllMaterialsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Materials
            .Include(m => m.Customer)
            .AsQueryable();

        // Apply filters
        if (request.IsActive.HasValue)
        {
            query = query.Where(m => m.IsActive == request.IsActive.Value);
        }

        if (request.CustomerId.HasValue && request.CustomerId.Value != Guid.Empty)
        {
            query = query.Where(m => m.CustomerId == request.CustomerId.Value);
        }

        var materials = await query
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MaterialDto
            {
                Id = m.Id,
                Code = m.Code,
                Name = m.Name,
                Type = m.Type,
                ColorCode = m.ColorCode,
                Supplier = m.Supplier,
                Unit = m.Unit,
                CurrentStock = m.CurrentStock,
                MinStock = m.MinStock,
                Description = m.Description,
                IsActive = m.IsActive,
                CreatedAt = m.CreatedAt,
                CustomerId = m.CustomerId,
                CustomerCode = m.Customer.Code,
                CustomerName = m.Customer.Name
            })
            .ToListAsync(cancellationToken);

        return materials;
    }
}

