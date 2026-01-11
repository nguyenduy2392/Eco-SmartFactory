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
    public Guid? CustomerId { get; set; }
    public string TemplateType { get; set; } = string.Empty;
    public DateTime PODate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    
    // Customer creation
    public string? CustomerName { get; set; }
    public string? CustomerCode { get; set; }
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
        try
        {
            // Get or create customer
            Guid customerId = request.CustomerId ?? Guid.Empty;
            Customer? customer = null;

            if (customerId != Guid.Empty)
            {
                customer = await _context.Customers.FindAsync(new object[] { customerId }, cancellationToken);
                if (customer == null)
                {
                    throw new Exception($"Customer with ID {customerId} not found");
                }
            }
            else if (!string.IsNullOrWhiteSpace(request.CustomerName))
            {
                // Create new customer
                customer = new Customer
                {
                    Code = request.CustomerCode ?? GenerateCustomerCode(),
                    Name = request.CustomerName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync(cancellationToken);
                customerId = customer.Id;
                _logger.LogInformation("Created new customer: {CustomerName} with code {CustomerCode}",
                    customer.Name, customer.Code);
            }
            else
            {
                throw new Exception("Either CustomerId or CustomerName must be provided");
            }

            // Parse Excel file
            var importResult = await _excelImportService.ImportPOFromExcel(request.FileStream, request.TemplateType,
                request.CustomerName, request.CustomerCode);

            if (!importResult.Success)
            {
                throw new Exception($"Failed to import Excel: {importResult.ErrorMessage}");
            }

            if (!importResult.Operations.Any())
            {
                throw new Exception("No operations found in Excel file");
            }

            // Validate PONumber is unique for operation PO (not original)
            var existingPO = await _context.PurchaseOrders
                .FirstOrDefaultAsync(p => p.PONumber == request.PONumber && p.OriginalPOId == null, cancellationToken);
            if (existingPO != null)
            {
                throw new Exception($"Mã PO '{request.PONumber}' đã tồn tại trong hệ thống. Vui lòng sử dụng mã PO khác hoặc xóa PO cũ trước khi import.");
            }

            // Create Original PO (readonly, locked) - PO gốc
            var originalPO = new PurchaseOrder
            {
                PONumber = $"{request.PONumber}_ORIGINAL",
                CustomerId = customerId,
                ProcessingType = request.TemplateType,
                PODate = request.PODate,
                ExpectedDeliveryDate = request.ExpectedDeliveryDate,
                Notes = request.Notes,
                Version = "V0",
                VersionNumber = 0,
                Status = "LOCKED", // Original PO is always locked (readonly)
                TotalAmount = 0,
                OriginalPOId = null, // This is the original
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.PurchaseOrders.Add(originalPO);
            await _context.SaveChangesAsync(cancellationToken); // Save to get the ID

            // Create Operation PO (editable) - PO operation
            var operationPO = new PurchaseOrder
            {
                PONumber = request.PONumber,
                CustomerId = customerId,
                ProcessingType = request.TemplateType,
                PODate = request.PODate,
                ExpectedDeliveryDate = request.ExpectedDeliveryDate,
                Notes = request.Notes,
                Version = "V0",
                VersionNumber = 0,
                Status = "DRAFT", // Operation PO starts as DRAFT (editable)
                TotalAmount = 0,
                OriginalPOId = originalPO.Id, // Link to original PO
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.PurchaseOrders.Add(operationPO);
            await _context.SaveChangesAsync(cancellationToken); // Save to get the ID

            // Use operationPO for operations (editable version)
            var po = operationPO;

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
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Process operations and create Parts, Products if needed
            decimal totalAmount = 0;

            // Dictionary để group operations theo Product và tính tổng quantity, total amount
            var productGroups = new Dictionary<Guid, (Product product, int totalQuantity, decimal totalAmount, string productCode, string productName)>();

            // Lists to store operations and products for copying to original PO
            var operationsToCopy = new List<POOperation>();
            var productsToCopy = new List<POProduct>();

            foreach (var operationData in importResult.Operations)
            {
                // Get or create Product
                // Ưu tiên sử dụng ProductCode từ Excel, nếu không có thì suy luận từ PartCode
                var productCode = !string.IsNullOrWhiteSpace(operationData.ProductCode)
                    ? operationData.ProductCode
                    : (operationData.PartCode.Contains('-')
                        ? operationData.PartCode.Split('-')[0]
                        : operationData.PartCode);

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Code == productCode, cancellationToken);

                if (product == null)
                {
                    product = new Product
                    {
                        Code = productCode,
                        Name = !string.IsNullOrWhiteSpace(operationData.ProductName)
                            ? operationData.ProductName
                            : $"Product {productCode}",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                else if (!string.IsNullOrWhiteSpace(operationData.ProductName) &&
                         product.Name != operationData.ProductName)
                {
                    // Cập nhật tên sản phẩm nếu có thay đổi
                    product.Name = operationData.ProductName;
                    product.UpdatedAt = DateTime.UtcNow;
                }

                // Get or create Part
                // Validate PartCode không được rỗng
                if (string.IsNullOrWhiteSpace(operationData.PartCode))
                {
                    _logger.LogWarning("Row {Row}: PartCode is empty, skipping part creation. ProductCode: {ProductCode}, ProductName: {ProductName}",
                        operationData.SequenceOrder, operationData.ProductCode, operationData.ProductName);
                    // Nếu không có PartCode, tạo một PartCode tạm từ ProductCode và sequence
                    operationData.PartCode = $"{productCode}-PART-{operationData.SequenceOrder}";
                    _logger.LogInformation("Generated temporary PartCode: {PartCode}", operationData.PartCode);
                }

                var part = await _context.Parts
                    .FirstOrDefaultAsync(p => p.Code == operationData.PartCode, cancellationToken);

                if (part == null)
                {
                    part = new Part
                    {
                        Code = operationData.PartCode,
                        Name = !string.IsNullOrWhiteSpace(operationData.PartName)
                            ? operationData.PartName
                            : $"Part {operationData.PartCode}",
                        ProductId = product.Id,
                        Material = operationData.Material,
                        Color = operationData.Color ?? operationData.ColorCode, // Ưu tiên Color, nếu không có thì dùng ColorCode
                        Weight = operationData.Weight,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Parts.Add(part);
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Created new Part: Code={PartCode}, Name={PartName}", part.Code, part.Name);
                }
                else
                {
                    // Cập nhật thông tin part nếu có thay đổi
                    if (!string.IsNullOrWhiteSpace(operationData.PartName) && part.Name != operationData.PartName)
                    {
                        part.Name = operationData.PartName;
                    }
                    if (!string.IsNullOrWhiteSpace(operationData.Material) && part.Material != operationData.Material)
                    {
                        part.Material = operationData.Material;
                    }
                    if (operationData.Weight.HasValue && part.Weight != operationData.Weight)
                    {
                        part.Weight = operationData.Weight;
                    }
                    var colorValue = operationData.Color ?? operationData.ColorCode;
                    if (!string.IsNullOrWhiteSpace(colorValue) && part.Color != colorValue)
                    {
                        part.Color = colorValue;
                    }
                    part.UpdatedAt = DateTime.UtcNow;
                }

                // Get or create Tool (Mold) nếu có MoldCode
                Guid? toolId = null;
                if (!string.IsNullOrWhiteSpace(operationData.MoldCode))
                {
                    var tool = await _context.Tools
                        .FirstOrDefaultAsync(t => t.Code == operationData.MoldCode, cancellationToken);

                    if (tool == null)
                    {
                        tool = new Tool
                        {
                            Code = operationData.MoldCode,
                            Name = $"Khuôn {operationData.MoldCode}",
                            Type = "Mold",
                            OwnerId = customerId,
                            Status = "Available",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Tools.Add(tool);
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                    toolId = tool.Id;
                }

                // Build notes với thông tin khuôn và các thông tin khác nếu có
                var notesParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(operationData.MoldCode))
                {
                    notesParts.Add($"Mã khuôn: {operationData.MoldCode}");
                }
                if (operationData.NumberOfCavities.HasValue)
                {
                    notesParts.Add($"Số lòng khuôn: {operationData.NumberOfCavities}");
                }
                if (!string.IsNullOrWhiteSpace(operationData.ColorCode))
                {
                    notesParts.Add($"Mã màu: {operationData.ColorCode}");
                }
                if (!string.IsNullOrWhiteSpace(operationData.ProcessingPosition))
                {
                    notesParts.Add($"Vị trí gia công: {operationData.ProcessingPosition}");
                }
                if (!string.IsNullOrWhiteSpace(operationData.OperationStep))
                {
                    notesParts.Add($"Công đoạn: {operationData.OperationStep}");
                }
                if (!string.IsNullOrWhiteSpace(operationData.ProcessingContent))
                {
                    notesParts.Add($"Nội dung gia công: {operationData.ProcessingContent}");
                }
                // Thêm notes từ Excel nếu có
                if (!string.IsNullOrWhiteSpace(operationData.Notes))
                {
                    notesParts.Add(operationData.Notes);
                }
                var notes = notesParts.Any() ? string.Join("; ", notesParts) : operationData.Notes;

                // Xác định OperationName
                // Với LAP_RAP, ưu tiên dùng AssemblyContent không cần tiền tố
                var operationName = string.Empty;
                if (request.TemplateType == "LAP_RAP" && !string.IsNullOrWhiteSpace(operationData.AssemblyContent))
                {
                    operationName = operationData.AssemblyContent;
                }
                else if (!string.IsNullOrWhiteSpace(operationData.OperationStep))
                {
                    operationName = $"{processingType.Name} - {operationData.OperationStep}";
                }
                else if (!string.IsNullOrWhiteSpace(operationData.PrintContent))
                {
                    operationName = $"{processingType.Name} - {operationData.PrintContent}";
                }
                else if (!string.IsNullOrWhiteSpace(operationData.AssemblyContent))
                {
                    operationName = operationData.AssemblyContent;
                }
                else
                {
                    operationName = string.IsNullOrWhiteSpace(operationData.PartName) ? processingType.Name : $"{processingType.Name} - {operationData.PartName}";
                }

                // Create PO Operation
                var poOperation = new POOperation
                {
                    PurchaseOrderId = po.Id,
                    PartId = part.Id,
                    ProductId = product.Id,
                    ProcessingTypeId = processingType.Id,
                    OperationName = operationName,
                    ChargeCount = operationData.ChargeCount ?? 0,  // Giữ nguyên giá trị từ Excel, kể cả 0
                    UnitPrice = operationData.UnitPrice,
                    ContractUnitPrice = operationData.ContractUnitPrice,
                    Quantity = operationData.Quantity,
                    // Tính TotalAmount = ChargeCount × UnitPrice × Quantity (không dùng từ Excel nữa)
                    TotalAmount = (operationData.ChargeCount ?? 0) * operationData.UnitPrice * operationData.Quantity,
                    SprayPosition = operationData.SprayPosition ?? operationData.ProcessingPosition,
                    PrintContent = operationData.PrintContent ?? operationData.OperationStep,
                    CycleTime = operationData.CycleTime,
                    AssemblyContent = operationData.AssemblyContent ?? operationData.ProcessingContent,
                    SequenceOrder = operationData.SequenceOrder,
                    Notes = notes,
                    // ÉP NHỰA specific fields from Excel
                    ModelNumber = operationData.MoldCode,
                    Material = operationData.Material,
                    ColorCode = operationData.ColorCode,
                    Color = operationData.Color,
                    CavityQuantity = operationData.NumberOfCavities,
                    Set = operationData.Set,
                    NetWeight = operationData.Weight,
                    TotalWeight = operationData.TotalWeight,
                    MachineType = operationData.PressMachine,
                    RequiredMaterial = operationData.RequiredPlasticQuantity,
                    RequiredColor = operationData.RequiredColorQuantity,
                    NumberOfPresses = operationData.NumberOfPresses,
                    CreatedAt = DateTime.UtcNow
                };

                _context.POOperations.Add(poOperation);
                operationsToCopy.Add(poOperation); // Store for copying to original PO
                // Tính lại totalAmount để tránh sai số từ Excel
                totalAmount += (operationData.ChargeCount ?? 0) * operationData.UnitPrice * operationData.Quantity;

                // Group theo Product để tạo POProduct
                if (!productGroups.ContainsKey(product.Id))
                {
                    productGroups[product.Id] = (
                        product,
                        0,
                        0,
                        !string.IsNullOrWhiteSpace(operationData.ProductCode) ? operationData.ProductCode : product.Code,
                        !string.IsNullOrWhiteSpace(operationData.ProductName) ? operationData.ProductName : product.Name
                    );
                }

                // Cập nhật tổng quantity và total amount cho product
                var group = productGroups[product.Id];
                var operationTotalAmount = (operationData.ChargeCount ?? 0) * operationData.UnitPrice * operationData.Quantity;
                productGroups[product.Id] = (
                    group.product,
                    group.totalQuantity + operationData.Quantity,
                    group.totalAmount + operationTotalAmount,  // Dùng giá trị đã tính lại
                    group.productCode,
                    group.productName
                );
            }

            // Tạo POProduct cho mỗi Product unique
            foreach (var (productId, (product, totalQuantity, productTotalAmount, productCode, productName)) in productGroups)
            {
                var poProduct = new POProduct
                {
                    PurchaseOrderId = po.Id,
                    ProductId = productId,
                    Quantity = totalQuantity,
                    UnitPrice = productTotalAmount > 0 && totalQuantity > 0 ? productTotalAmount / totalQuantity : null,
                    TotalAmount = productTotalAmount,
                    CreatedAt = DateTime.UtcNow
                };

                _context.POProducts.Add(poProduct);
                productsToCopy.Add(poProduct); // Store for copying to original PO
            }

            po.TotalAmount = totalAmount;
            originalPO.TotalAmount = totalAmount; // Copy total amount to original

            // Copy all operations and products to original PO (for reference, readonly)
            foreach (var operation in operationsToCopy)
            {
                var originalOperation = new POOperation
                {
                    PurchaseOrderId = originalPO.Id,
                    PartId = operation.PartId,
                    ProcessingTypeId = operation.ProcessingTypeId,
                    OperationName = operation.OperationName,
                    ChargeCount = operation.ChargeCount,
                    UnitPrice = operation.UnitPrice,
                    Quantity = operation.Quantity,
                    TotalAmount = operation.TotalAmount,
                    SprayPosition = operation.SprayPosition,
                    PrintContent = operation.PrintContent,
                    CycleTime = operation.CycleTime,
                    AssemblyContent = operation.AssemblyContent,
                    SequenceOrder = operation.SequenceOrder,
                    Notes = operation.Notes,
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
                    CreatedAt = operation.CreatedAt
                };
                _context.POOperations.Add(originalOperation);
            }

            foreach (var product in productsToCopy)
            {
                var originalProduct = new POProduct
                {
                    PurchaseOrderId = originalPO.Id,
                    ProductId = product.ProductId,
                    Quantity = product.Quantity,
                    UnitPrice = product.UnitPrice,
                    TotalAmount = product.TotalAmount,
                    CreatedAt = product.CreatedAt
                };
                _context.POProducts.Add(originalProduct);
            }

            // Save PurchaseOrderMaterials from Sheet 2 (NHAP_NGUYEN_VAT_LIEU)
            // Đây là danh sách nguyên vật liệu CẦN THIẾT để gia công PO (như BOM)
            // KHÔNG phải nhập kho thực tế
            foreach (var materialData in importResult.MaterialReceipts)
            {
                // Get or create Material - Tất cả PO dùng chung Materials, chỉ check theo Code
                var material = await _context.Materials
                    .FirstOrDefaultAsync(m => m.Code == materialData.MaterialCode, cancellationToken);

                if (material == null)
                {
                    // Tạo Material mới - Material dùng chung cho tất cả Customer
                    material = new Material
                    {
                        Code = materialData.MaterialCode,
                        Name = materialData.MaterialName,
                        Type = materialData.MaterialType ?? "Unknown",
                        Unit = materialData.Unit,
                        CustomerId = null, // Material dùng chung, không gắn với Customer cụ thể
                        Supplier = materialData.SupplierCode,
                        CurrentStock = 0, // CurrentStock chỉ được update khi nhập kho thực tế
                        MinStock = 0,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Materials.Add(material);
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Created new shared Material: {MaterialCode} - {MaterialName}", material.Code, material.Name);
                }
                else
                {
                    // Cập nhật thông tin Material nếu có thay đổi (ưu tiên thông tin mới hơn)
                    var hasChanges = false;
                    
                    if (!string.IsNullOrWhiteSpace(materialData.MaterialName) && material.Name != materialData.MaterialName)
                    {
                        material.Name = materialData.MaterialName;
                        hasChanges = true;
                    }
                    if (!string.IsNullOrWhiteSpace(materialData.MaterialType) && material.Type != materialData.MaterialType)
                    {
                        material.Type = materialData.MaterialType;
                        hasChanges = true;
                    }
                    if (!string.IsNullOrWhiteSpace(materialData.Unit) && material.Unit != materialData.Unit)
                    {
                        material.Unit = materialData.Unit;
                        hasChanges = true;
                    }
                    if (!string.IsNullOrWhiteSpace(materialData.SupplierCode) && material.Supplier != materialData.SupplierCode)
                    {
                        material.Supplier = materialData.SupplierCode;
                        hasChanges = true;
                    }
                    
                    if (hasChanges)
                    {
                        material.UpdatedAt = DateTime.UtcNow;
                        _logger.LogDebug("Updated shared Material: {MaterialCode}", material.Code);
                    }
                }

                // Tạo PurchaseOrderMaterial cho Original PO (để hiển thị "Nguyên Vật Liệu Cần Cho PO")
                var poMaterial = new PurchaseOrderMaterial
                {
                    PurchaseOrderId = originalPO.Id, // Gắn với Original PO
                    MaterialCode = materialData.MaterialCode,
                    MaterialName = materialData.MaterialName,
                    MaterialType = materialData.MaterialType,
                    PlannedQuantity = materialData.Quantity,
                    Unit = materialData.Unit,
                    Notes = materialData.Notes,
                    CreatedAt = DateTime.UtcNow
                };
                _context.PurchaseOrderMaterials.Add(poMaterial);

                _logger.LogDebug("Created PurchaseOrderMaterial: {MaterialCode} - Planned Quantity: {Quantity} {Unit}",
                    materialData.MaterialCode, materialData.Quantity, materialData.Unit);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Imported PO from Excel: {PONumber} (Original: {OriginalPONumber}) with {OperationCount} operations and {MaterialCount} required materials",
                operationPO.PONumber, originalPO.PONumber, importResult.Operations.Count, importResult.MaterialReceipts.Count);

            // Return operation PO (editable version)
            return new PurchaseOrderDto
            {
                Id = operationPO.Id,
                PONumber = operationPO.PONumber,
                CustomerId = operationPO.CustomerId,
                CustomerName = customer.Name,
                Version = operationPO.Version,
                ProcessingType = operationPO.ProcessingType,
                PODate = operationPO.PODate,
                ExpectedDeliveryDate = operationPO.ExpectedDeliveryDate,
                Status = operationPO.Status,
                TotalAmount = operationPO.TotalAmount,
                Notes = operationPO.Notes,
                VersionNumber = operationPO.VersionNumber,
                OriginalPOId = originalPO.Id, // Include reference to original PO
                IsActive = operationPO.IsActive,
                CreatedAt = operationPO.CreatedAt
            };
        }
        catch (Exception ex)
        {

            throw;
        }
    }

    private string GenerateCustomerCode()
    {
        return $"C-{DateTime.Now:yyyyMMddHHmmss}";
    }
}



