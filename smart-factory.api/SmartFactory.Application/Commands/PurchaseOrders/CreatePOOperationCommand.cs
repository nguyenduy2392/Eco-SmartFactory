using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Commands.PurchaseOrders;

public class CreatePOOperationCommand : IRequest<POOperationDto>
{
    public Guid PurchaseOrderId { get; set; }
    public Guid? PartId { get; set; }
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
    public string? Notes { get; set; }
    public int SequenceOrder { get; set; }
    // Product and Part codes for creating relationships
    public string? ProductCode { get; set; }
    public string? PartCode { get; set; }
    // ÉP NHỰA specific fields
    public string? ModelNumber { get; set; }
    public string? Material { get; set; }
    public string? ColorCode { get; set; }
    public string? Color { get; set; }
    public int? CavityQuantity { get; set; }
    public int? Set { get; set; }
    public decimal? NetWeight { get; set; }
    public decimal? TotalWeight { get; set; }
    public string? MachineType { get; set; }
    public decimal? RequiredMaterial { get; set; }
    public decimal? RequiredColor { get; set; }
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

        Entities.Part? part = null;

        // Handle ProductCode and PartCode - find or create Product and Part
        if (!string.IsNullOrWhiteSpace(request.ProductCode) || !string.IsNullOrWhiteSpace(request.PartCode))
        {
            Entities.Product? product = null;

            // Get or create Product if ProductCode is provided
            if (!string.IsNullOrWhiteSpace(request.ProductCode))
            {
                product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Code == request.ProductCode, cancellationToken);

                if (product == null)
                {
                    product = new Entities.Product
                    {
                        Code = request.ProductCode,
                        Name = $"Product {request.ProductCode}",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Created new Product: Code={ProductCode}", product.Code);
                }
            }

            // Get or create Part if PartCode is provided
            if (!string.IsNullOrWhiteSpace(request.PartCode))
            {
                part = await _context.Parts
                    .Include(p => p.Product)
                    .FirstOrDefaultAsync(p => p.Code == request.PartCode, cancellationToken);

                if (part == null)
                {
                    if (product == null)
                    {
                        throw new Exception("Cannot create Part without Product. Please provide ProductCode when creating a new Part.");
                    }

                    part = new Entities.Part
                    {
                        Code = request.PartCode,
                        Name = $"Part {request.PartCode}",
                        ProductId = product.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Parts.Add(part);
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Created new Part: Code={PartCode}, ProductId={ProductId}", part.Code, part.ProductId);
                }
                else
                {
                    // If part exists but product is different, update the part's product relationship
                    if (product != null && part.ProductId != product.Id)
                    {
                        part.ProductId = product.Id;
                        part.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation("Updated Part ProductId: PartCode={PartCode}, NewProductId={ProductId}", part.Code, product.Id);
                    }
                    // Reload Product navigation property
                    await _context.Entry(part)
                        .Reference(p => p.Product)
                        .LoadAsync(cancellationToken);
                }
            }
        }

        // If PartId is provided, use it (fallback if ProductCode/PartCode not provided)
        if (part == null && request.PartId.HasValue)
        {
            part = await _context.Parts
                .Include(p => p.Product)
                .FirstOrDefaultAsync(p => p.Id == request.PartId.Value, cancellationToken);
        }

        if (part == null)
        {
            throw new Exception("Part not found. Please provide either PartId, or ProductCode and PartCode.");
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
            PartId = part.Id, // Use the part we found/created, not request.PartId
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
            Notes = request.Notes,
            SequenceOrder = request.SequenceOrder,
            // ÉP NHỰA specific fields
            ModelNumber = request.ModelNumber,
            Material = request.Material,
            ColorCode = request.ColorCode,
            Color = request.Color,
            CavityQuantity = request.CavityQuantity,
            Set = request.Set,
            NetWeight = request.NetWeight,
            TotalWeight = request.TotalWeight,
            MachineType = request.MachineType,
            RequiredMaterial = request.RequiredMaterial,
            RequiredColor = request.RequiredColor,
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
            // ÉP NHỰA specific fields
            ModelNumber = operation.ModelNumber,
            Material = operation.Material,
            ColorCode = operation.ColorCode,
            Color = operation.Color,
            CavityQuantity = operation.CavityQuantity,
            Set = operation.Set,
            NetWeight = operation.NetWeight,
            TotalWeight = operation.TotalWeight,
            MachineType = operation.MachineType,
            RequiredMaterial = operation.RequiredMaterial,
            RequiredColor = operation.RequiredColor,
            Notes = operation.Notes,
            CompletionDate = operation.CompletionDate,
            SequenceOrder = operation.SequenceOrder
        };
    }
}

