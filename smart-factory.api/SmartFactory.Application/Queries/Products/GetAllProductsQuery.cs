using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Queries.Products;

public class GetAllProductsQuery : IRequest<List<ProductDto>>
{
}

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllProductsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return products;
    }
}

