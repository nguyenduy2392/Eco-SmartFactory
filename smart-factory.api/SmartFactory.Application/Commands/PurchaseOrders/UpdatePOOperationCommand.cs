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
    // Product and Part codes for updating relationships
    public string? ProductCode { get; set; }
    public string? PartCode { get; set; }
    public string? PartName { get; set; }
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
    public int? NumberOfPresses { get; set; } // Số lần ép
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
                .ThenInclude(part => part!.Product)
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

        // Handle ProductCode and PartCode updates
        // For LAP_RAP, ProductCode can be provided without PartCode
        Entities.Product? product = null;
        Entities.Part? part = null;

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
        else if (operation.Part != null)
        {
            // If ProductCode is not provided, use the existing product from the current part
            await _context.Entry(operation.Part)
                .Reference(p => p.Product)
                .LoadAsync(cancellationToken);
            product = operation.Part.Product;
        }

        // Get or create Part if PartCode is provided (optional for LAP_RAP)
        if (!string.IsNullOrWhiteSpace(request.PartCode))
        {
            part = await _context.Parts
                .FirstOrDefaultAsync(p => p.Code == request.PartCode, cancellationToken);

            if (part == null)
            {
                if (product == null)
                {
                    throw new Exception("Cannot create Part without Product. Please provide ProductCode when creating a new Part.");
                }

                // Use PartName from request if provided, otherwise use PartCode
                var partName = !string.IsNullOrWhiteSpace(request.PartName) 
                    ? request.PartName 
                    : $"Part {request.PartCode}";

                part = new Entities.Part
                {
                    Code = request.PartCode,
                    Name = partName,
                    ProductId = product.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Parts.Add(part);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Created new Part: Code={PartCode}, Name={PartName}, ProductId={ProductId}", 
                    part.Code, part.Name, part.ProductId);
            }
            else
            {
                // Update part name if provided and different
                if (!string.IsNullOrWhiteSpace(request.PartName) && part.Name != request.PartName)
                {
                    part.Name = request.PartName;
                    part.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation("Updated Part Name: PartCode={PartCode}, NewName={PartName}", 
                        part.Code, part.Name);
                }
                
                // If part exists but product is different, update the part's product relationship
                if (product != null && part.ProductId != product.Id)
                {
                    part.ProductId = product.Id;
                    part.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation("Updated Part ProductId: PartCode={PartCode}, NewProductId={ProductId}", 
                        part.Code, product.Id);
                }
                
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        else if (product != null)
        {
            // For LAP_RAP: If only ProductCode is provided (no PartCode), create a virtual Part to link Product to Operation
            // Use ProductCode as PartCode for LAP_RAP operations
            var virtualPartCode = request.ProductCode;
            part = await _context.Parts
                .FirstOrDefaultAsync(p => p.Code == virtualPartCode && p.ProductId == product.Id, cancellationToken);

            if (part == null)
            {
                part = new Entities.Part
                {
                    Code = virtualPartCode,
                    Name = $"Part {virtualPartCode}",
                    ProductId = product.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Parts.Add(part);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Created virtual Part for LAP_RAP: Code={PartCode}, ProductId={ProductId}", part.Code, part.ProductId);
            }
        }
        else
        {
            // If PartCode is not provided and no ProductCode, use the existing part (can be null for LAP_RAP)
            part = operation.Part;
        }

        // Update operation's PartId if it changed
        var newPartId = part?.Id;
        if (operation.PartId != newPartId)
        {
            operation.PartId = newPartId;
            // Reload the Part navigation property to reflect the change
            if (part != null)
            {
                await _context.Entry(operation)
                    .Reference(op => op.Part)
                    .LoadAsync(cancellationToken);
                if (operation.Part != null)
                {
                    await _context.Entry(operation.Part)
                        .Reference(p => p.Product)
                        .LoadAsync(cancellationToken);
                }
            }
            _logger.LogInformation("Updated PO Operation PartId: OperationId={OperationId}, NewPartId={PartId}", operation.Id, newPartId);
        }

        operation.OperationName = request.OperationName;
        operation.ChargeCount = request.ChargeCount;
        operation.UnitPrice = request.UnitPrice;
        operation.Quantity = request.Quantity;
        operation.SprayPosition = request.SprayPosition;
        operation.PrintContent = request.PrintContent;
        operation.CycleTime = request.CycleTime;
        operation.AssemblyContent = request.AssemblyContent;
        // ÉP NHỰA specific fields
        operation.ModelNumber = request.ModelNumber;
        operation.Material = request.Material;
        operation.ColorCode = request.ColorCode;
        operation.Color = request.Color;
        operation.CavityQuantity = request.CavityQuantity;
        operation.Set = request.Set;
        operation.NetWeight = request.NetWeight;
        operation.TotalWeight = request.TotalWeight;
        operation.MachineType = request.MachineType;
        operation.RequiredMaterial = request.RequiredMaterial;
        operation.RequiredColor = request.RequiredColor;
        operation.NumberOfPresses = request.NumberOfPresses;
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
            PartId = operation.PartId ?? Guid.Empty,
            PartCode = operation.Part?.Code ?? string.Empty,
            PartName = operation.Part?.Name ?? string.Empty,
            PartImageUrl = operation.Part?.ImageUrl,
            ProductId = operation.Part?.ProductId,
            ProductCode = operation.Part?.Product?.Code ?? string.Empty,
            ProductName = operation.Part?.Product?.Name,
            ProcessingTypeId = operation.ProcessingTypeId,
            ProcessingTypeName = operation.ProcessingType.Name,
            ProcessMethodId = operation.ProcessMethodId,
            ProcessMethodName = operation.ProcessMethod?.Name,
            OperationName = operation.OperationName,
            ChargeCount = operation.ChargeCount,
            UnitPrice = operation.UnitPrice,
            ContractUnitPrice = operation.ContractUnitPrice,
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
            NumberOfPresses = operation.NumberOfPresses,
            Notes = operation.Notes,
            CompletionDate = operation.CompletionDate,
            SequenceOrder = operation.SequenceOrder
        };
    }
}

