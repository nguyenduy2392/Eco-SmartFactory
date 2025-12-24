using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.Data;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Entities;
using SmartFactory.Application.Services;

namespace SmartFactory.Application.Commands.PurchaseOrders;

/// <summary>
/// Command để import PO từ Excel
/// </summary>
public class ImportPOFromExcelCommand : IRequest<PurchaseOrderDto>
{
    public Stream FileStream { get; set; } = null!;
    public string PONumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string TemplateType { get; set; } = string.Empty;
    public DateTime PODate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Notes { get; set; }
}

public class ImportPOFromExcelCommandHandler : IRequestHandler<ImportPOFromExcelCommand, PurchaseOrderDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ExcelImportService _excelImportService;
    private readonly ILogger<ImportPOFromExcelCommandHandler> _logger;

    public ImportPOFromExcelCommandHandler(
        ApplicationDbContext context,
        ExcelImportService excelImportService,
        ILogger<ImportPOFromExcelCommandHandler> logger)
    {
        _context = context;
        _excelImportService = excelImportService;
        _logger = logger;
    }

    public async Task<PurchaseOrderDto> Handle(ImportPOFromExcelCommand request, CancellationToken cancellationToken)
    {
        // Validate customer
        var customer = await _context.Customers.FindAsync(new object[] { request.CustomerId }, cancellationToken);
        if (customer == null)
        {
            throw new Exception($"Customer with ID {request.CustomerId} not found");
        }

        // Parse Excel file
        var importResult = await _excelImportService.ImportPOFromExcel(request.FileStream, request.TemplateType);

        if (!importResult.Success)
        {
            throw new Exception($"Failed to import Excel: {importResult.ErrorMessage}");
        }

        if (!importResult.Operations.Any())
        {
            throw new Exception("No operations found in Excel file");
        }

        // Create Purchase Order
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

        // Get or create ProcessingType
        var processingType = await _context.ProcessingTypes
            .FirstOrDefaultAsync(pt => pt.Name == importResult.Operations.First().ProcessingTypeName, cancellationToken);

        if (processingType == null)
        {
            processingType = new ProcessingType
            {
                Code = importResult.TemplateType,
                Name = importResult.Operations.First().ProcessingTypeName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.ProcessingTypes.Add(processingType);
            await _context.SaveChangesAsync(cancellationToken); // Save to get ID
        }

        // Collect unique products from operations
        var uniqueProductCodes = importResult.Operations
            .Select(op => op.PartCode.Split('-')[0]) // Assuming format: PRODUCT-PART
            .Distinct()
            .ToList();

        // Process operations and create Parts, Products if needed
        decimal totalAmount = 0;

        foreach (var operationData in importResult.Operations)
        {
            // Get or create Product (from part code prefix)
            var productCode = operationData.PartCode.Contains('-') 
                ? operationData.PartCode.Split('-')[0] 
                : operationData.PartCode;

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Code == productCode, cancellationToken);

            if (product == null)
            {
                product = new Product
                {
                    Code = productCode,
                    Name = $"Product {productCode}",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Products.Add(product);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Get or create Part
            var part = await _context.Parts
                .FirstOrDefaultAsync(p => p.Code == operationData.PartCode, cancellationToken);

            if (part == null)
            {
                part = new Part
                {
                    Code = operationData.PartCode,
                    Name = operationData.PartName,
                    ProductId = product.Id,
                    Material = operationData.Material,
                    Color = operationData.Color,
                    Weight = operationData.Weight,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Parts.Add(part);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Create PO Operation
            var poOperation = new POOperation
            {
                PurchaseOrderId = po.Id,
                PartId = part.Id,
                ProcessingTypeId = processingType.Id,
                OperationName = $"{processingType.Name} - {operationData.PartName}",
                ChargeCount = 1, // Default
                UnitPrice = operationData.UnitPrice,
                Quantity = operationData.Quantity,
                TotalAmount = operationData.TotalAmount,
                SprayPosition = operationData.SprayPosition,
                PrintContent = operationData.PrintContent,
                CycleTime = operationData.CycleTime,
                AssemblyContent = operationData.AssemblyContent,
                SequenceOrder = operationData.SequenceOrder,
                CreatedAt = DateTime.UtcNow
            };

            _context.POOperations.Add(poOperation);
            totalAmount += operationData.TotalAmount;
        }

        // Update PO total amount
        po.TotalAmount = totalAmount;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Imported PO from Excel: {PONumber} with {OperationCount} operations", 
            po.PONumber, importResult.Operations.Count);

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

