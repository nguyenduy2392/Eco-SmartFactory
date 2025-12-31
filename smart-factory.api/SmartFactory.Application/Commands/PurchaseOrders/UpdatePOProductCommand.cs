using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Commands.PurchaseOrders;

public class UpdatePOProductCommand : IRequest<POProductDto>
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
}

public class UpdatePOProductCommandHandler : IRequestHandler<UpdatePOProductCommand, POProductDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdatePOProductCommandHandler> _logger;

    public UpdatePOProductCommandHandler(
        ApplicationDbContext context,
        ILogger<UpdatePOProductCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<POProductDto> Handle(UpdatePOProductCommand request, CancellationToken cancellationToken)
    {
        var poProduct = await _context.POProducts
            .Include(pp => pp.Product)
            .Include(pp => pp.PurchaseOrder)
            .FirstOrDefaultAsync(pp => pp.Id == request.Id && pp.PurchaseOrderId == request.PurchaseOrderId, cancellationToken);

        if (poProduct == null)
        {
            throw new Exception($"PO Product with ID {request.Id} not found");
        }

        // Cho phép sửa mọi lúc - đã bỏ kiểm tra trạng thái DRAFT
        // if (poProduct.PurchaseOrder.Status != "DRAFT")
        // {
        //     throw new Exception("Chỉ có thể chỉnh sửa sản phẩm khi PO ở trạng thái DRAFT");
        // }

        poProduct.Quantity = request.Quantity;
        poProduct.UnitPrice = request.UnitPrice;
        poProduct.TotalAmount = (request.UnitPrice ?? 0) * request.Quantity;
        poProduct.PurchaseOrder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated PO Product: {ProductCode} (Quantity: {Quantity}) for PO: {PONumber}", 
            poProduct.Product.Code, poProduct.Quantity, poProduct.PurchaseOrder.PONumber);

        return new POProductDto
        {
            Id = poProduct.Id,
            PurchaseOrderId = poProduct.PurchaseOrderId,
            ProductId = poProduct.ProductId,
            ProductCode = poProduct.Product.Code,
            ProductName = poProduct.Product.Name,
            Quantity = poProduct.Quantity,
            UnitPrice = poProduct.UnitPrice,
            TotalAmount = poProduct.TotalAmount
        };
    }
}

