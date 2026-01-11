using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.PurchaseOrders;

public class GetAllPurchaseOrdersQuery : IRequest<List<PurchaseOrderListDto>>
{
    public string? Status { get; set; }
    public string? Version { get; set; }
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
        // Only show operation POs (editable), not original POs (readonly)
        // Operation POs have OriginalPOId != null, Original POs have OriginalPOId == null
        var query = _context.PurchaseOrders
            .Include(p => p.Customer)
            .Include(p => p.POProducts)
            .Where(p => p.IsActive && p.OriginalPOId != null); // Only operation POs

        // Apply filters
        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(p => p.Status == request.Status);
        }

        if (!string.IsNullOrEmpty(request.Version))
        {
            query = query.Where(p => p.Version == request.Version);
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
                Version = p.Version,
                ProcessingType = p.ProcessingType,
                PODate = p.PODate,
                Status = p.Status,
                TotalAmount = p.TotalAmount,
                ProductCount = p.POProducts.Count,
                IsMaterialFullyReceived = p.IsMaterialFullyReceived,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return pos;
    }
}




