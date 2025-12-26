using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Commands.PurchaseOrders;

public class CreatePurchaseOrderCommand : IRequest<PurchaseOrderDto>
{
    public string PONumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string? TemplateType { get; set; }
    public DateTime PODate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    public List<CreatePOProductRequest>? Products { get; set; }
}

public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, PurchaseOrderDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreatePurchaseOrderCommandHandler> _logger;

    public CreatePurchaseOrderCommandHandler(
        ApplicationDbContext context,
        ILogger<CreatePurchaseOrderCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PurchaseOrderDto> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate customer exists
        var customer = await _context.Customers.FindAsync(new object[] { request.CustomerId }, cancellationToken);
        if (customer == null)
        {
            throw new Exception($"Customer with ID {request.CustomerId} not found");
        }

        // Validate PONumber is unique
        var existingPO = await _context.PurchaseOrders
            .FirstOrDefaultAsync(p => p.PONumber == request.PONumber, cancellationToken);
        if (existingPO != null)
        {
            throw new Exception($"Mã PO '{request.PONumber}' đã tồn tại trong hệ thống. Vui lòng sử dụng mã PO khác.");
        }

        var po = new PurchaseOrder
        {
            PONumber = request.PONumber,
            CustomerId = request.CustomerId,
            TemplateType = request.TemplateType,
            PODate = request.PODate,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            Notes = request.Notes,
            VersionType = "ORIGINAL",
            VersionNumber = 1,
            Status = "New",
            TotalAmount = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.PurchaseOrders.Add(po);

        // Add products if provided
        if (request.Products != null && request.Products.Any())
        {
            decimal totalAmount = 0;
            foreach (var productRequest in request.Products)
            {
                var product = await _context.Products.FindAsync(new object[] { productRequest.ProductId }, cancellationToken);
                if (product == null) continue;

                var productTotal = (productRequest.UnitPrice ?? 0) * productRequest.Quantity;
                totalAmount += productTotal;

                var poProduct = new POProduct
                {
                    PurchaseOrderId = po.Id,
                    ProductId = productRequest.ProductId,
                    Quantity = productRequest.Quantity,
                    UnitPrice = productRequest.UnitPrice,
                    TotalAmount = productTotal,
                    CreatedAt = DateTime.UtcNow
                };

                _context.POProducts.Add(poProduct);
            }

            po.TotalAmount = totalAmount;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created new PO: {PONumber} with ID: {POId}", po.PONumber, po.Id);

        return new PurchaseOrderDto
        {
            Id = po.Id,
            PONumber = po.PONumber,
            CustomerId = po.CustomerId,
            CustomerName = customer.Name,
            VersionType = po.VersionType,
            TemplateType = po.TemplateType,
            PODate = po.PODate,
            ExpectedDeliveryDate = po.ExpectedDeliveryDate,
            Status = po.Status,
            TotalAmount = po.TotalAmount,
            Notes = po.Notes,
            VersionNumber = po.VersionNumber,
            IsActive = po.IsActive,
            CreatedAt = po.CreatedAt
        };
    }
}



