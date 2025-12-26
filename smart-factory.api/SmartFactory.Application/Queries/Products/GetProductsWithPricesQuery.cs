using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.Products;

public class GetProductsWithPricesQuery : IRequest<List<ProductWithPriceDto>>
{
}

public class GetProductsWithPricesQueryHandler : IRequestHandler<GetProductsWithPricesQuery, List<ProductWithPriceDto>>
{
    private readonly ApplicationDbContext _context;

    public GetProductsWithPricesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductWithPriceDto>> Handle(GetProductsWithPricesQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .Where(p => p.IsActive)
            .Include(p => p.POProducts)
            .ToListAsync(cancellationToken);

        var result = products.Select(p => new ProductWithPriceDto
        {
            Id = p.Id,
            Code = p.Code,
            Name = p.Name,
            Description = p.Description,
            ImageUrl = p.ImageUrl,
            Category = p.Category,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            // Lấy giá mới nhất từ POProduct
            LatestUnitPrice = p.POProducts
                .Where(pp => pp.UnitPrice.HasValue)
                .OrderByDescending(pp => pp.CreatedAt)
                .Select(pp => pp.UnitPrice)
                .FirstOrDefault(),
            // Tính giá trung bình
            AverageUnitPrice = p.POProducts
                .Where(pp => pp.UnitPrice.HasValue)
                .Any() ? p.POProducts
                    .Where(pp => pp.UnitPrice.HasValue)
                    .Average(pp => pp.UnitPrice!.Value) : null,
            // Đếm số PO có sản phẩm này
            TotalPOs = p.POProducts.Select(pp => pp.PurchaseOrderId).Distinct().Count(),
            // Tổng số lượng đã bán
            TotalQuantity = p.POProducts.Sum(pp => pp.Quantity)
        })
        .OrderByDescending(p => p.CreatedAt)
        .ToList();

        return result;
    }
}

