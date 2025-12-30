using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.Warehouse;

/// <summary>
/// Query để lấy danh sách tất cả kho
/// </summary>
public class GetAllWarehousesQuery : IRequest<List<WarehouseDto>>
{
    public bool? IsActive { get; set; }
}

public class GetAllWarehousesQueryHandler : IRequestHandler<GetAllWarehousesQuery, List<WarehouseDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllWarehousesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WarehouseDto>> Handle(GetAllWarehousesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Warehouses.AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(w => w.IsActive == request.IsActive.Value);
        }

        var warehouses = await query
            .OrderBy(w => w.Code)
            .ToListAsync(cancellationToken);

        return warehouses.Select(w => new WarehouseDto
        {
            Id = w.Id,
            Code = w.Code,
            Name = w.Name,
            Address = w.Address,
            Description = w.Description,
            IsActive = w.IsActive,
            CreatedAt = w.CreatedAt,
            UpdatedAt = w.UpdatedAt
        }).ToList();
    }
}

