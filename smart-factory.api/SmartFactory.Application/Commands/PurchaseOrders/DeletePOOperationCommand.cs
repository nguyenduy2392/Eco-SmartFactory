using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;

namespace SmartFactory.Application.Commands.PurchaseOrders;

public class DeletePOOperationCommand : IRequest
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
}

public class DeletePOOperationCommandHandler : IRequestHandler<DeletePOOperationCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeletePOOperationCommandHandler> _logger;

    public DeletePOOperationCommandHandler(
        ApplicationDbContext context,
        ILogger<DeletePOOperationCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(DeletePOOperationCommand request, CancellationToken cancellationToken)
    {
        var operation = await _context.POOperations
            .Include(op => op.PurchaseOrder)
            .FirstOrDefaultAsync(op => op.Id == request.Id && op.PurchaseOrderId == request.PurchaseOrderId, cancellationToken);

        if (operation == null)
        {
            throw new Exception($"PO Operation with ID {request.Id} not found");
        }

        // Cho phép xóa mọi lúc - đã bỏ kiểm tra trạng thái DRAFT
        // if (operation.PurchaseOrder.Status != "DRAFT")
        // {
        //     throw new Exception("Chỉ có thể xóa công đoạn khi PO ở trạng thái DRAFT");
        // }

        var po = operation.PurchaseOrder;
        var operationName = operation.OperationName;

        _context.POOperations.Remove(operation);

        // Update PO total amount
        po.TotalAmount = await _context.POOperations
            .Where(op => op.PurchaseOrderId == request.PurchaseOrderId)
            .SumAsync(op => op.TotalAmount, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted PO Operation: {OperationName} from PO: {PONumber}", 
            operationName, po.PONumber);
    }
}

