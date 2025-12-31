using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Commands.PurchaseOrders;

public class CreatePOOperationCommand : IRequest<POOperationDto>
{
    public Guid PurchaseOrderId { get; set; }
    public Guid PartId { get; set; }
    public Guid ProcessingTypeId { get; set; }
    public Guid? ProcessMethodId { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public int ChargeCount { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? SprayPosition { get; set; }
    public string? PrintContent { get; set; }
    public decimal? CycleTime { get; set; }
    public string? AssemblyContent { get; set; }
    public DateTime? CompletionDate { get; set; }
    public int SequenceOrder { get; set; }
}

public class CreatePOOperationCommandHandler : IRequestHandler<CreatePOOperationCommand, POOperationDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreatePOOperationCommandHandler> _logger;

    public CreatePOOperationCommandHandler(
        ApplicationDbContext context,
        ILogger<CreatePOOperationCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<POOperationDto> Handle(CreatePOOperationCommand request, CancellationToken cancellationToken)
    {
        var po = await _context.PurchaseOrders
            .FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId, cancellationToken);

        if (po == null)
        {
            throw new Exception($"Purchase Order with ID {request.PurchaseOrderId} not found");
        }

        // Cho phép thêm mọi lúc - đã bỏ kiểm tra trạng thái DRAFT
        // if (po.Status != "DRAFT")
        // {
        //     throw new Exception("Chỉ có thể thêm công đoạn khi PO ở trạng thái DRAFT");
        // }

        var part = await _context.Parts
            .Include(p => p.Product)
            .FirstOrDefaultAsync(p => p.Id == request.PartId, cancellationToken);

        if (part == null)
        {
            throw new Exception($"Part with ID {request.PartId} not found");
        }

        var processingType = await _context.ProcessingTypes
            .FirstOrDefaultAsync(pt => pt.Id == request.ProcessingTypeId, cancellationToken);

        if (processingType == null)
        {
            throw new Exception($"Processing Type with ID {request.ProcessingTypeId} not found");
        }

        var totalAmount = request.ChargeCount * request.UnitPrice * request.Quantity;

        var operation = new Entities.POOperation
        {
            PurchaseOrderId = request.PurchaseOrderId,
            PartId = request.PartId,
            ProcessingTypeId = request.ProcessingTypeId,
            ProcessMethodId = request.ProcessMethodId,
            OperationName = request.OperationName,
            ChargeCount = request.ChargeCount,
            UnitPrice = request.UnitPrice,
            Quantity = request.Quantity,
            TotalAmount = totalAmount,
            SprayPosition = request.SprayPosition,
            PrintContent = request.PrintContent,
            CycleTime = request.CycleTime,
            AssemblyContent = request.AssemblyContent,
            CompletionDate = request.CompletionDate,
            SequenceOrder = request.SequenceOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.POOperations.Add(operation);

        // Update PO total amount
        po.TotalAmount = await _context.POOperations
            .Where(op => op.PurchaseOrderId == request.PurchaseOrderId)
            .SumAsync(op => op.TotalAmount, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created PO Operation: {OperationName} for PO: {PONumber}", 
            operation.OperationName, po.PONumber);

        var processMethod = operation.ProcessMethodId.HasValue
            ? await _context.ProcessMethods.FindAsync(new object[] { operation.ProcessMethodId.Value }, cancellationToken)
            : null;

        return new POOperationDto
        {
            Id = operation.Id,
            PurchaseOrderId = operation.PurchaseOrderId,
            PartId = operation.PartId,
            PartCode = part.Code,
            PartName = part.Name,
            ProductId = part.ProductId,
            ProductCode = part.Product?.Code ?? string.Empty,
            ProductName = part.Product?.Name,
            ProcessingTypeId = operation.ProcessingTypeId,
            ProcessingTypeName = processingType.Name,
            ProcessMethodId = operation.ProcessMethodId,
            ProcessMethodName = processMethod?.Name,
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

