using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Commands.PurchaseOrders;

public class UpdatePOOperationCommand : IRequest<POOperationDto>
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public int ChargeCount { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? SprayPosition { get; set; }
    public string? PrintContent { get; set; }
    public decimal? CycleTime { get; set; }
    public string? AssemblyContent { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePOOperationCommandHandler : IRequestHandler<UpdatePOOperationCommand, POOperationDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdatePOOperationCommandHandler> _logger;

    public UpdatePOOperationCommandHandler(
        ApplicationDbContext context,
        ILogger<UpdatePOOperationCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<POOperationDto> Handle(UpdatePOOperationCommand request, CancellationToken cancellationToken)
    {
        var operation = await _context.POOperations
            .Include(op => op.Part)
                .ThenInclude(part => part.Product)
            .Include(op => op.ProcessingType)
            .Include(op => op.ProcessMethod)
            .Include(op => op.PurchaseOrder)
            .FirstOrDefaultAsync(op => op.Id == request.Id && op.PurchaseOrderId == request.PurchaseOrderId, cancellationToken);

        if (operation == null)
        {
            throw new Exception($"PO Operation with ID {request.Id} not found");
        }

        // Cho phép sửa mọi lúc - đã bỏ kiểm tra trạng thái DRAFT
        // if (operation.PurchaseOrder.Status != "DRAFT")
        // {
        //     throw new Exception("Chỉ có thể chỉnh sửa công đoạn khi PO ở trạng thái DRAFT");
        // }

        operation.OperationName = request.OperationName;
        operation.ChargeCount = request.ChargeCount;
        operation.UnitPrice = request.UnitPrice;
        operation.Quantity = request.Quantity;
        operation.SprayPosition = request.SprayPosition;
        operation.PrintContent = request.PrintContent;
        operation.CycleTime = request.CycleTime;
        operation.AssemblyContent = request.AssemblyContent;
        operation.CompletionDate = request.CompletionDate;
        operation.Notes = request.Notes;
        operation.TotalAmount = request.ChargeCount * request.UnitPrice * request.Quantity;
        operation.UpdatedAt = DateTime.UtcNow;

        // Update PO total amount
        var po = operation.PurchaseOrder;
        po.TotalAmount = await _context.POOperations
            .Where(op => op.PurchaseOrderId == request.PurchaseOrderId)
            .SumAsync(op => op.TotalAmount, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated PO Operation: {OperationName} for PO: {PONumber}", 
            operation.OperationName, po.PONumber);

        return new POOperationDto
        {
            Id = operation.Id,
            PurchaseOrderId = operation.PurchaseOrderId,
            PartId = operation.PartId,
            PartCode = operation.Part.Code,
            PartName = operation.Part.Name,
            ProductId = operation.Part.ProductId,
            ProductCode = operation.Part.Product?.Code ?? string.Empty,
            ProductName = operation.Part.Product?.Name,
            ProcessingTypeId = operation.ProcessingTypeId,
            ProcessingTypeName = operation.ProcessingType.Name,
            ProcessMethodId = operation.ProcessMethodId,
            ProcessMethodName = operation.ProcessMethod?.Name,
            OperationName = operation.OperationName,
            ChargeCount = operation.ChargeCount,
            UnitPrice = operation.UnitPrice,
            Quantity = operation.Quantity,
            TotalAmount = operation.TotalAmount,
            SprayPosition = operation.SprayPosition,
            PrintContent = operation.PrintContent,
            CycleTime = operation.CycleTime,
            AssemblyContent = operation.AssemblyContent,
            Notes = operation.Notes,
            CompletionDate = operation.CompletionDate,
            SequenceOrder = operation.SequenceOrder
        };
    }
}

