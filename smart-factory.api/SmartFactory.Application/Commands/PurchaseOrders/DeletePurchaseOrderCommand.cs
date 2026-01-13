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
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (po == null)
        {
            throw new Exception($"Purchase Order with ID {request.Id} not found");
        }

        // Tìm PO gốc
        var originalPOId = po.OriginalPOId ?? po.Id;
        
        // Lấy tất cả PO cần xóa (original + tất cả versions)
        var posToDelete = await _context.PurchaseOrders
            .Include(p => p.POProducts)
            .Include(p => p.POOperations)
            .Where(p => p.Id == originalPOId || p.OriginalPOId == originalPOId)
            .ToListAsync(cancellationToken);

        // Xóa tất cả PO và các bản ghi liên quan
        foreach (var poToDelete in posToDelete)
        {
            if (poToDelete.POProducts?.Any() == true)
            {
                _context.POProducts.RemoveRange(poToDelete.POProducts);
            }

            if (poToDelete.POOperations?.Any() == true)
            {
                _context.POOperations.RemoveRange(poToDelete.POOperations);
            }

            _context.PurchaseOrders.Remove(poToDelete);
            
            _logger.LogInformation("Deleting PO: {PONumber} (Version: {Version}) with ID: {POId}", 
                poToDelete.PONumber, poToDelete.Version, poToDelete.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted {Count} PO(s) including original and all derived versions", 
            posToDelete.Count);

        return true;
    }
}

