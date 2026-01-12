using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;

namespace SmartFactory.Application.Queries.PMC;

/// <summary>
/// Debug query to check available POs for PMC creation
/// </summary>
public class GetAvailablePOsForPMCQuery : IRequest<object>
{
}

public class GetAvailablePOsForPMCQueryHandler : IRequestHandler<GetAvailablePOsForPMCQuery, object>
{
    private readonly ApplicationDbContext _context;

    public GetAvailablePOsForPMCQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<object> Handle(GetAvailablePOsForPMCQuery request, CancellationToken cancellationToken)
    {
        // Get all active POs
        var allPOs = await _context.PurchaseOrders
            .Include(po => po.POProducts)
                .ThenInclude(pop => pop.Product)
                    .ThenInclude(p => p!.Parts)
            .Include(po => po.Customer)
            .Where(po => po.IsActive)
            .Select(po => new
            {
                po.PONumber,
                po.Status,
                po.ProcessingType,
                CustomerName = po.Customer.Name,
                ProductCount = po.POProducts.Count,
                Products = po.POProducts.Select(pop => new
                {
                    ProductCode = pop.Product!.Code,
                    ProductName = pop.Product.Name,
                    PartCount = pop.Product.Parts.Count,
                    Parts = pop.Product.Parts.Select(p => new
                    {
                        p.Code,
                        p.Name
                    }).ToList()
                }).ToList()
            })
            .ToListAsync(cancellationToken);
        
        // Eligible POs: DRAFT/APPROVED status (all ProcessingTypes)
        var eligiblePOs = allPOs
            .Where(po => po.Status == "DRAFT" || po.Status == "APPROVED" || po.Status == "APPROVED_FOR_PMC")
            .ToList();
        
        // PHUN/IN subset
        var phunInPOs = eligiblePOs
            .Where(po => po.ProcessingType == "PHUN_IN" || po.ProcessingType == "PHUN" || po.ProcessingType == "IN")
            .ToList();
        
        return new
        {
            TotalPOs = allPOs.Count,
            EligiblePOsCount = eligiblePOs.Count,
            PhunInPOsCount = phunInPOs.Count,
            EligiblePOs = eligiblePOs,
            PhunInPOs = phunInPOs,
            Message = phunInPOs.Any() 
                ? $"Will use {phunInPOs.Count} PHUN/IN POs" 
                : $"No PHUN/IN POs found, will use all {eligiblePOs.Count} eligible POs",
            AllPOsGroupByProcessingType = allPOs
                .GroupBy(po => po.ProcessingType ?? "NULL")
                .Select(g => new
                {
                    ProcessingType = g.Key,
                    Count = g.Count(),
                    POs = g.Select(po => po.PONumber).ToList()
                })
                .ToList(),
            AllPOsGroupByStatus = allPOs
                .GroupBy(po => po.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    POs = g.Select(po => po.PONumber).ToList()
                })
                .ToList()
        };
    }
}
