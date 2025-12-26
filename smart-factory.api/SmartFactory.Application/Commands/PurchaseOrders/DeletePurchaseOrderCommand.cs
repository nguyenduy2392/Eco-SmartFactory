using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;

namespace SmartFactory.Application.Commands.PurchaseOrders;

public class DeletePurchaseOrderCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeletePurchaseOrderCommandHandler : IRequestHandler<DeletePurchaseOrderCommand, bool>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeletePurchaseOrderCommandHandler> _logger;

    public DeletePurchaseOrderCommandHandler(
        ApplicationDbContext context,
        ILogger<DeletePurchaseOrderCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(DeletePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await _context.PurchaseOrders
            .Include(p => p.POProducts)
            .Include(p => p.POOperations)
            .Include(p => p.DerivedVersions)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (po == null)
        {
            throw new Exception($"Purchase Order with ID {request.Id} not found");
        }

        // Kiểm tra nếu PO có các phiên bản derived (FINAL, PRODUCTION)
        if (po.DerivedVersions != null && po.DerivedVersions.Any())
        {
            throw new Exception($"Cannot delete PO {po.PONumber} because it has derived versions. Please delete derived versions first.");
        }

        // Kiểm tra nếu PO này là derived version của PO khác
        if (po.OriginalPOId.HasValue)
        {
            // Cho phép xóa nhưng cảnh báo
            _logger.LogWarning("Deleting derived PO version: {PONumber}", po.PONumber);
        }

        // Xóa các POProducts (cascade delete)
        if (po.POProducts != null && po.POProducts.Any())
        {
            _context.POProducts.RemoveRange(po.POProducts);
        }

        // Xóa các POOperations (cascade delete)
        if (po.POOperations != null && po.POOperations.Any())
        {
            _context.POOperations.RemoveRange(po.POOperations);
        }

        // Xóa PO
        _context.PurchaseOrders.Remove(po);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted PO: {PONumber} with ID: {POId}", po.PONumber, po.Id);

        return true;
    }
}

