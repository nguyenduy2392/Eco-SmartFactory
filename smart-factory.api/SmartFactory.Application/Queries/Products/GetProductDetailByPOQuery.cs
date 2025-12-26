using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.Products;

public class GetProductDetailByPOQuery : IRequest<ProductDetailDto?>
{
    public Guid ProductId { get; set; }
    public Guid PurchaseOrderId { get; set; }
}

public class ProductDetailDto
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Material { get; set; }
    public string? Color { get; set; }
    public string? ColorHex { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = "Draft";
    public List<ComponentItemDto> Components { get; set; } = new();
}

public class ComponentItemDto
{
    public Guid Id { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public string PartId { get; set; } = string.Empty;
    public string? Material { get; set; }
    public string QuantityRequired { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
}

public class GetProductDetailByPOQueryHandler : IRequestHandler<GetProductDetailByPOQuery, ProductDetailDto?>
{
    private readonly ApplicationDbContext _context;

    public GetProductDetailByPOQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDetailDto?> Handle(GetProductDetailByPOQuery request, CancellationToken cancellationToken)
    {
        // Get PO to get PONumber
        var po = await _context.PurchaseOrders
            .FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId, cancellationToken);

        if (po == null)
        {
            return null;
        }

        // Get Product
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            return null;
        }

        // Get POProduct để lấy quantity và thông tin khác
        var poProduct = await _context.POProducts
            .FirstOrDefaultAsync(pp => pp.PurchaseOrderId == request.PurchaseOrderId && 
                                       pp.ProductId == request.ProductId, cancellationToken);

        // Get all Parts của Product trong PO này (thông qua POOperations)
        var parts = await _context.POOperations
            .Where(op => op.PurchaseOrderId == request.PurchaseOrderId && 
                        op.Part.ProductId == request.ProductId)
            .Include(op => op.Part)
            .Select(op => op.Part)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Get quantity required for each part (tổng quantity từ POOperations)
        var partQuantities = await _context.POOperations
            .Where(op => op.PurchaseOrderId == request.PurchaseOrderId && 
                        op.Part.ProductId == request.ProductId)
            .GroupBy(op => op.PartId)
            .Select(g => new
            {
                PartId = g.Key,
                TotalQuantity = g.Sum(op => op.Quantity)
            })
            .ToListAsync(cancellationToken);

        // Map to ComponentItemDto
        var components = parts.Select(part =>
        {
            var quantity = partQuantities.FirstOrDefault(q => q.PartId == part.Id)?.TotalQuantity ?? 0;
            return new ComponentItemDto
            {
                Id = part.Id,
                ComponentName = part.Name,
                PartId = part.Code,
                Material = part.Material,
                QuantityRequired = $"{quantity} pcs",
                Status = part.IsActive ? "Ready" : "Pending",
                Description = part.Description
            };
        }).ToList();

        return new ProductDetailDto
        {
            Id = product.Id,
            PurchaseOrderId = request.PurchaseOrderId,
            PONumber = po.PONumber,
            ProductName = product.Name,
            SKU = product.Code,
            Quantity = poProduct?.Quantity ?? 0,
            Material = null, // Product không có material, material ở Part level
            Color = null, // Product không có color, color ở Part level
            ColorHex = null,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            Status = po.Status,
            Components = components
        };
    }
}

