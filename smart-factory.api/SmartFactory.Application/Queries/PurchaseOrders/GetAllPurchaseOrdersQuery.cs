using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.PurchaseOrders;

public class GetAllPurchaseOrdersQuery : IRequest<List<PurchaseOrderListDto>>
{
    public string? Status { get; set; }
    public string? VersionType { get; set; }
    public Guid? CustomerId { get; set; }
}

public class GetAllPurchaseOrdersQueryHandler : IRequestHandler<GetAllPurchaseOrdersQuery, List<PurchaseOrderListDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllPurchaseOrdersQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PurchaseOrderListDto>> Handle(GetAllPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PurchaseOrders
            .Include(p => p.Customer)
            .Include(p => p.POProducts)
            .Where(p => p.IsActive);

        // Apply filters
        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(p => p.Status == request.Status);
        }

        if (!string.IsNullOrEmpty(request.VersionType))
        {
            query = query.Where(p => p.VersionType == request.VersionType);
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(p => p.CustomerId == request.CustomerId.Value);
        }

        var pos = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PurchaseOrderListDto
            {
                Id = p.Id,
                PONumber = p.PONumber,
                CustomerName = p.Customer.Name,
                VersionType = p.VersionType,
                PODate = p.PODate,
                Status = p.Status,
                TotalAmount = p.TotalAmount,
                ProductCount = p.POProducts.Count,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return pos;
    }
}



