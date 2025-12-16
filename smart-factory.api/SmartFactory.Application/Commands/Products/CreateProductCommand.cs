using MediatR;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Commands.Products;

public class CreateProductCommand : IRequest<ProductDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        ApplicationDbContext context,
        ILogger<CreateProductCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created new product: {ProductName} with ID: {ProductId}", product.Name, product.Id);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        };
    }
}

