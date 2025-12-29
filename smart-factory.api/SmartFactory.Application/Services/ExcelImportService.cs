using OfficeOpenXml;
using SmartFactory.Application.Entities;
using Microsoft.Extensions.Logging;

namespace SmartFactory.Application.Services;

/// <summary>
/// Service để import PO từ Excel cho 3 loại template:
/// - ÉP NHỰA (ep_nhua): có trọng lượng, chu kỳ ép
/// - LẮP RÁP (lap_rap): có nội dung lắp ráp
/// - PHUN IN (phun_in): có vị trí phun, nội dung in
/// Hỗ trợ cột tiếng Việt và tiếng Trung
/// </summary>
public class ExcelImportService
{
    private readonly ILogger<ExcelImportService> _logger;

    public ExcelImportService(ILogger<ExcelImportService> logger)
    {
        _logger = logger;
        // Required for EPPlus
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    /// <summary>
    /// Import PO từ file Excel (PHASE 1: 2-sheet format)
    /// Sheet 1: NHAP_PO (PO Operations)
    /// Sheet 2: NHAP_NGUYEN_VAT_LIEU (Material Baseline for availability check)
    /// </summary>
    public async Task<ExcelImportResult> ImportPOFromExcel(Stream fileStream, string templateType, string? customerName = null, string? customerCode = null)
    {
        try
        {
            using var package = new ExcelPackage(fileStream);
            
            // Validate sheet count
            if (package.Workbook.Worksheets.Count < 2)
            {
                return new ExcelImportResult
                {
                    Success = false,
                    ErrorMessage = "Excel file must contain exactly 2 sheets: NHAP_PO and NHAP_NGUYEN_VAT_LIEU"
                };
            }
            
            // Parse Sheet 1: NHAP_PO (PO Operations)
            var sheet1 = package.Workbook.Worksheets[0];
            var result = templateType.ToUpper() switch
            {
                "EP_NHUA" => await ParseEpNhuaTemplate(sheet1),
                "LAP_RAP" => await ParseLapRapTemplate(sheet1),
                "PHUN_IN" => await ParsePhunInTemplate(sheet1),
                _ => throw new ArgumentException($"Unknown template type: {templateType}")
            };
            
            if (!result.Success)
            {
                return result;
            }
            
            // Parse Sheet 2: NHAP_NGUYEN_VAT_LIEU (Material Receipt - nhập kho thực tế)
            var sheet2 = package.Workbook.Worksheets[1];
            var materialReceipts = await ParseMaterialReceiptSheet(sheet2);
            result.MaterialReceipts = materialReceipts;
            
            _logger.LogInformation("Imported {OperationCount} operations and {MaterialCount} material receipts",
                result.Operations.Count, result.MaterialReceipts.Count);

            // Thêm thông tin khách hàng
            if (!string.IsNullOrWhiteSpace(customerName))
            {
                result.CustomerName = customerName;
                result.CustomerCode = customerCode ?? GenerateCustomerCode();
                result.ShouldCreateCustomer = true;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing Excel file");
            return new ExcelImportResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Parse template ÉP NHỰA
    /// Template mới có các cột:
    /// Mã sản phẩm, Tên sản phẩm, Mã khuôn/Model, Mã linh kiện, Tên linh kiện, 
    /// Vật liệu, Mã màu, Màu sắc, Số lòng khuôn, Chu kỳ (s), Trọng lượng (g), 
    /// Số lượng (PCS), Đơn giá (VND), Thành tiền (VND)
    /// Parse theo header động để hỗ trợ cả template cũ và mới
    /// </summary>
    private async Task<ExcelImportResult> ParseEpNhuaTemplate(ExcelWorksheet worksheet)
    {
        var result = new ExcelImportResult { TemplateType = "EP_NHUA" };

        // Tìm header row (có thể là row 1 hoặc row khác)
        int headerRow = FindHeaderRow(worksheet);
        if (headerRow == 0)
        {
            result.Success = false;
            result.ErrorMessage = "Không tìm thấy header row trong file Excel";
            return result;
        }

        // Parse header để tìm vị trí các cột
        var columnMap = ParseHeaderRow(worksheet, headerRow);
        
        // Log để debug
        _logger.LogInformation("EP_NHUA: Parsed header row {HeaderRow}. Found columns: {Columns}", 
            headerRow, string.Join(", ", columnMap.Keys));
        
        if (!columnMap.ContainsKey("ProductCode"))
        {
            _logger.LogWarning("EP_NHUA: ProductCode column not found in Excel header");
        }
        if (!columnMap.ContainsKey("ProductName"))
        {
            _logger.LogWarning("EP_NHUA: ProductName column not found in Excel header");
        }

        // Start reading data from row after header
        int startRow = headerRow + 1;
        int currentRow = startRow;

        while (!IsEmptyRow(worksheet, currentRow))
        {
            try
            {
                // Đọc ProductCode và ProductName
                var productCode = GetValueByColumn(worksheet, currentRow, columnMap, "ProductCode", "Mã sản phẩm", "产品代码");
                var productName = GetValueByColumn(worksheet, currentRow, columnMap, "ProductName", "Tên sản phẩm", "产品名称");
                
                // Log nếu không tìm thấy ở dòng đầu tiên
                if (string.IsNullOrWhiteSpace(productCode) && currentRow == startRow)
                {
                    _logger.LogWarning("EP_NHUA Row {Row}: ProductCode is empty", currentRow);
                }
                if (string.IsNullOrWhiteSpace(productName) && currentRow == startRow)
                {
                    _logger.LogWarning("EP_NHUA Row {Row}: ProductName is empty", currentRow);
                }
                
                var operation = new POOperationData
                {
                    // Thông tin sản phẩm
                    ProductCode = productCode,
                    ProductName = productName,
                    
                    // Thông tin khuôn
                    MoldCode = GetValueByColumn(worksheet, currentRow, columnMap, "MoldCode", "Mã khuôn", "模具代码", "Model"),
                    
                    // Thông tin linh kiện
                    PartCode = GetValueByColumn(worksheet, currentRow, columnMap, "PartCode", "Mã linh kiện", "零件代码"),
                    PartName = GetValueByColumn(worksheet, currentRow, columnMap, "PartName", "Tên linh kiện", "零件名称"),
                    
                    // Thông tin vật liệu và màu
                    Material = GetValueByColumn(worksheet, currentRow, columnMap, "Material", "Vật liệu", "材料"),
                    ColorCode = GetValueByColumn(worksheet, currentRow, columnMap, "ColorCode", "Mã màu", "颜色代码"),
                    Color = GetValueByColumn(worksheet, currentRow, columnMap, "Color", "Màu sắc", "颜色"),
                    
                    // Thông tin kỹ thuật
                    NumberOfCavities = GetIntValueByColumn(worksheet, currentRow, columnMap, "NumberOfCavities", "Số lòng khuôn", "模腔数"),
                    CycleTime = GetDecimalValueByColumn(worksheet, currentRow, columnMap, "CycleTime", "Chu kỳ", "周期", "Chu kỳ (s)", "周期(s)"),
                    Weight = GetDecimalValueByColumn(worksheet, currentRow, columnMap, "Weight", "Trọng lượng", "重量", "Trọng lượng (g)", "重量(g)"),
                    
                    // Thông tin số lượng và giá
                    Quantity = GetIntValueByColumn(worksheet, currentRow, columnMap, "Quantity", "Số lượng", "数量", "Số lượng (PCS)", "数量(PCS)"),
                    UnitPrice = GetDecimalValueByColumn(worksheet, currentRow, columnMap, "UnitPrice", "Đơn giá", "单价", "Đơn giá (VND)", "单价(VND)"),
                    TotalAmount = GetDecimalValueByColumn(worksheet, currentRow, columnMap, "TotalAmount", "Thành tiền", "总金额", "Thành tiền (VND)", "总金额(VND)"),
                    
                    // STT nếu có
                    SequenceOrder = GetIntValueByColumn(worksheet, currentRow, columnMap, "SequenceOrder", "STT", "序号"),
                    
                    ProcessingTypeName = "ÉP NHỰA"
                };

                // Log để debug nếu PartCode hoặc PartName rỗng (chỉ log ở dòng đầu tiên)
                if (currentRow == startRow)
                {
                    if (string.IsNullOrWhiteSpace(operation.PartCode))
                    {
                        _logger.LogWarning("EP_NHUA Row {Row}: PartCode is empty. ColumnMap contains PartCode: {HasPartCode}", 
                            currentRow, columnMap.ContainsKey("PartCode"));
                        if (columnMap.ContainsKey("PartCode"))
                        {
                            var partCodeCol = columnMap["PartCode"];
                            var rawValue = GetStringValue(worksheet, currentRow, partCodeCol);
                            _logger.LogInformation("EP_NHUA Row {Row}: Raw PartCode value from column {Col}: '{RawValue}'", 
                                currentRow, partCodeCol, rawValue);
                        }
                        else
                        {
                            _logger.LogWarning("EP_NHUA: PartCode column not found in header. Available columns: {Columns}", 
                                string.Join(", ", columnMap.Keys));
                        }
                    }
                    if (string.IsNullOrWhiteSpace(operation.PartName))
                    {
                        _logger.LogWarning("EP_NHUA Row {Row}: PartName is empty. ColumnMap contains PartName: {HasPartName}", 
                            currentRow, columnMap.ContainsKey("PartName"));
                        if (columnMap.ContainsKey("PartName"))
                        {
                            var partNameCol = columnMap["PartName"];
                            var rawValue = GetStringValue(worksheet, currentRow, partNameCol);
                            _logger.LogInformation("EP_NHUA Row {Row}: Raw PartName value from column {Col}: '{RawValue}'", 
                                currentRow, partNameCol, rawValue);
                        }
                        else
                        {
                            _logger.LogWarning("EP_NHUA: PartName column not found in header. Available columns: {Columns}", 
                                string.Join(", ", columnMap.Keys));
                        }
                    }
                }

                // Nếu không có STT, tự động đánh số
                if (operation.SequenceOrder == 0)
                {
                    operation.SequenceOrder = result.Operations.Count + 1;
                }

                result.Operations.Add(operation);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error parsing row {currentRow}: {ex.Message}");
                result.Errors.Add($"Row {currentRow}: {ex.Message}");
            }

            currentRow++;
        }

        result.Success = result.Operations.Any();
        if (!result.Success)
        {
            result.ErrorMessage = "No valid operations found in Excel file";
        }

        return result;
    }

    /// <summary>
    /// Parse template LẮP RÁP
    /// Template mới có các cột:
    /// Mã sản phẩm, Nội dung gia công, Số lần gia công, Đơn giá (VND), Thành tiền (VND),
    /// Số lượng hợp đồng (PCS), Tổng tiền (VND), Ngày hoàn thành, Ghi chú
    /// Parse theo header động để hỗ trợ cả template cũ và mới
    /// </summary>
    private async Task<ExcelImportResult> ParseLapRapTemplate(ExcelWorksheet worksheet)
    {
        var result = new ExcelImportResult { TemplateType = "LAP_RAP" };

        // Tìm header row
        int headerRow = FindHeaderRow(worksheet);
        if (headerRow == 0)
        {
            result.Success = false;
            result.ErrorMessage = "Không tìm thấy header row trong file Excel";
            return result;
        }

        // Parse header để tìm vị trí các cột
        var columnMap = ParseHeaderRow(worksheet, headerRow);

        // Start reading data from row after header
        int startRow = headerRow + 1;
        int currentRow = startRow;

        while (!IsEmptyRow(worksheet, currentRow))
        {
            try
            {
                // Bỏ qua dòng tổng (có ghi chú "Dòng tổng")
                var notes = GetValueByColumn(worksheet, currentRow, columnMap, "Notes", "Ghi chú", "备注");
                if (!string.IsNullOrWhiteSpace(notes) && notes.Contains("Dòng tổng"))
                {
                    currentRow++;
                    continue;
                }

                var operation = new POOperationData
                {
                    // Thông tin sản phẩm
                    ProductCode = GetValueByColumn(worksheet, currentRow, columnMap, "ProductCode", "Mã sản phẩm", "产品代码"),
                    
                    // Nội dung lắp ráp
                    AssemblyContent = GetValueByColumn(worksheet, currentRow, columnMap, "AssemblyContent", "Nội dung gia công", "加工内容", "Nội dung lắp ráp", "装配内容"),
                    
                    // Số lần gia công (Charge Count)
                    ChargeCount = GetIntValueByColumn(worksheet, currentRow, columnMap, "ChargeCount", "Số lần gia công", "加工次数"),
                    
                    // Đơn giá và thành tiền
                    UnitPrice = GetDecimalValueByColumn(worksheet, currentRow, columnMap, "UnitPrice", "Đơn giá", "单价", "Đơn giá (VND)", "单价(VND)"),
                    TotalAmount = GetDecimalValueByColumn(worksheet, currentRow, columnMap, "TotalAmount", "Thành tiền", "总金额", "Thành tiền (VND)", "总金额(VND)"),
                    
                    // Số lượng hợp đồng
                    Quantity = GetIntValueByColumn(worksheet, currentRow, columnMap, "Quantity", "Số lượng", "数量", "Số lượng hợp đồng", "合同数量", "Số lượng hợp đồng (PCS)", "合同数量(PCS)"),
                    
                    ProcessingTypeName = "LẮP RÁP"
                };

                // Nếu không có số lượng từ cột "Số lượng hợp đồng", thử lấy từ cột "Số lượng" thông thường
                if (operation.Quantity == 0)
                {
                    operation.Quantity = GetIntValueByColumn(worksheet, currentRow, columnMap, "Quantity", "Số lượng", "数量");
                }

                // Tự động đánh số thứ tự
                operation.SequenceOrder = result.Operations.Count + 1;

                result.Operations.Add(operation);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error parsing row {currentRow}: {ex.Message}");
                result.Errors.Add($"Row {currentRow}: {ex.Message}");
            }

            currentRow++;
        }

        result.Success = result.Operations.Any();
        if (!result.Success)
        {
            result.ErrorMessage = "No valid operations found in Excel file";
        }

        return result;
    }

    /// <summary>
    /// Parse template PHUN IN
    /// Template mới có các cột:
    /// Tên sản phẩm, Mã sản phẩm, Mã linh kiện, Mô tả linh kiện, Vị trí gia công, Công đoạn,
    /// Số lần gia công, Đơn giá (VND), Đơn giá chuẩn (VND), Số lượng, Đơn giá hợp đồng (PCS),
    /// Thành tiền (VND), Ngày hoàn thành, Ghi chú
    /// Parse theo header động để hỗ trợ cả template cũ và mới
    /// </summary>
    private async Task<ExcelImportResult> ParsePhunInTemplate(ExcelWorksheet worksheet)
    {
        var result = new ExcelImportResult { TemplateType = "PHUN_IN" };

        // Tìm header row
        int headerRow = FindHeaderRow(worksheet);
        if (headerRow == 0)
        {
            result.Success = false;
            result.ErrorMessage = "Không tìm thấy header row trong file Excel";
            return result;
        }

        // Parse header để tìm vị trí các cột
        var columnMap = ParseHeaderRow(worksheet, headerRow);

        // Start reading data from row after header
        int startRow = headerRow + 1;
        int currentRow = startRow;

        while (!IsEmptyRow(worksheet, currentRow))
        {
            try
            {
                var operation = new POOperationData
                {
                    // Thông tin sản phẩm
                    ProductCode = GetValueByColumn(worksheet, currentRow, columnMap, "ProductCode", "Mã sản phẩm", "产品代码"),
                    ProductName = GetValueByColumn(worksheet, currentRow, columnMap, "ProductName", "Tên sản phẩm", "产品名称"),
                    
                    // Thông tin linh kiện
                    PartCode = GetValueByColumn(worksheet, currentRow, columnMap, "PartCode", "Mã linh kiện", "零件代码"),
                    PartName = GetValueByColumn(worksheet, currentRow, columnMap, "PartName", "Mô tả linh kiện", "零件描述", "Tên linh kiện", "零件名称"),
                    
                    // Vị trí gia công / Vị trí phun
                    SprayPosition = GetValueByColumn(worksheet, currentRow, columnMap, "SprayPosition", "Vị trí gia công", "加工位置", "Vị trí phun", "喷涂位置"),
                    
                    // Công đoạn / Nội dung in
                    PrintContent = GetValueByColumn(worksheet, currentRow, columnMap, "PrintContent", "Công đoạn", "工序", "Nội dung in", "印刷内容"),
                    
                    // Số lần gia công (Charge Count)
                    ChargeCount = GetIntValueByColumn(worksheet, currentRow, columnMap, "ChargeCount", "Số lần gia công", "加工次数"),
                    
                    // Đơn giá (ưu tiên Đơn giá hợp đồng, nếu không có thì dùng Đơn giá)
                    UnitPrice = GetDecimalValueByColumn(worksheet, currentRow, columnMap, "ContractUnitPrice", "Đơn giá hợp đồng", "合同单价", "Đơn giá hợp đồng (PCS)", "合同单价(PCS)"),
                    
                    // Đơn giá chuẩn
                    StandardUnitPrice = GetDecimalValueByColumn(worksheet, currentRow, columnMap, "StandardUnitPrice", "Đơn giá chuẩn", "标准单价", "Đơn giá chuẩn (VND)", "标准单价(VND)"),
                    
                    // Số lượng
                    Quantity = GetIntValueByColumn(worksheet, currentRow, columnMap, "Quantity", "Số lượng", "数量"),
                    
                    // Thành tiền
                    TotalAmount = GetDecimalValueByColumn(worksheet, currentRow, columnMap, "TotalAmount", "Thành tiền", "总金额", "Thành tiền (VND)", "总金额(VND)"),
                    
                    ProcessingTypeName = "PHUN IN"
                };

                // Nếu không có đơn giá hợp đồng, dùng đơn giá thông thường
                if (operation.UnitPrice == 0)
                {
                    operation.UnitPrice = GetDecimalValueByColumn(worksheet, currentRow, columnMap, "UnitPrice", "Đơn giá", "单价", "Đơn giá (VND)", "单价(VND)");
                }

                // Tự động đánh số thứ tự
                operation.SequenceOrder = result.Operations.Count + 1;

                result.Operations.Add(operation);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error parsing row {currentRow}: {ex.Message}");
                result.Errors.Add($"Row {currentRow}: {ex.Message}");
            }

            currentRow++;
        }

        result.Success = result.Operations.Any();
        if (!result.Success)
        {
            result.ErrorMessage = "No valid operations found in Excel file";
        }

        return result;
    }

    #region Helper Methods

    /// <summary>
    /// Tìm header row trong worksheet
    /// </summary>
    private int FindHeaderRow(ExcelWorksheet worksheet)
    {
        // Tìm trong 5 dòng đầu
        for (int row = 1; row <= 5; row++)
        {
            // Kiểm tra xem có chứa các từ khóa header không
            for (int col = 1; col <= 20; col++)
            {
                var value = GetStringValue(worksheet, row, col).ToLower();
                if (value.Contains("mã") || value.Contains("tên") || value.Contains("số lượng") || 
                    value.Contains("đơn giá") || value.Contains("零件") || value.Contains("产品"))
                {
                    return row;
                }
            }
        }
        return 0;
    }

    /// <summary>
    /// Parse header row để tạo map cột
    /// </summary>
    private Dictionary<string, int> ParseHeaderRow(ExcelWorksheet worksheet, int headerRow)
    {
        var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        
        // Đọc tất cả các cột trong header row
        for (int col = 1; col <= 50; col++) // Tối đa 50 cột
        {
            var headerValue = GetStringValue(worksheet, headerRow, col).Trim();
            if (string.IsNullOrWhiteSpace(headerValue))
                continue;

            var headerLower = headerValue.ToLower();
            
            // Map các cột theo tên tiếng Việt và tiếng Trung
            // Mã sản phẩm / Product Code - Cải thiện để match linh hoạt hơn
            if ((headerLower.Contains("mã") && headerLower.Contains("sản phẩm")) || 
                headerLower.Contains("产品代码") || 
                headerLower == "product code" || 
                headerLower.Contains("productcode") ||
                headerLower.Contains("product_code") ||
                (headerLower.Contains("code") && headerLower.Contains("product")))
            {
                if (!columnMap.ContainsKey("ProductCode"))
                {
                    columnMap["ProductCode"] = col;
                }
            }
            
            // Tên sản phẩm / Product Name - Cải thiện để match linh hoạt hơn
            if ((headerLower.Contains("tên") && headerLower.Contains("sản phẩm")) || 
                headerLower.Contains("产品名称") || 
                headerLower == "product name" || 
                headerLower.Contains("productname") ||
                headerLower.Contains("product_name") ||
                (headerLower.Contains("name") && headerLower.Contains("product")))
            {
                if (!columnMap.ContainsKey("ProductName"))
                {
                    columnMap["ProductName"] = col;
                }
            }
            
            // Mã khuôn / Mold Code / Model
            if (headerLower.Contains("mã khuôn") || headerLower.Contains("model") || 
                headerLower.Contains("模具代码") || headerLower.Contains("mold code") ||
                headerLower.Contains("moldcode"))
            {
                columnMap["MoldCode"] = col;
            }
            
            // Mã linh kiện / Part Code
            // Loại bỏ dấu tiếng Việt để so sánh tốt hơn
            var headerNormalized = NormalizeVietnamese(headerLower);
            if (headerNormalized.Contains("ma linh kien") || headerLower.Contains("mã linh kiện") || 
                headerLower.Contains("零件代码") || headerLower.Contains("零件代碼") ||
                headerLower == "part code" || headerLower.Contains("partcode") ||
                headerLower.Contains("part_code"))
            {
                if (!columnMap.ContainsKey("PartCode"))
                {
                    columnMap["PartCode"] = col;
                }
            }
            
            // Tên linh kiện / Part Name / Mô tả linh kiện
            if (headerNormalized.Contains("ten linh kien") || headerLower.Contains("tên linh kiện") || 
                headerNormalized.Contains("mo ta linh kien") || headerLower.Contains("mô tả linh kiện") || 
                headerLower.Contains("零件名称") || headerLower.Contains("零件名稱") ||
                headerLower.Contains("零件描述") ||
                headerLower == "part name" || headerLower.Contains("partname") ||
                headerLower.Contains("part_name"))
            {
                if (!columnMap.ContainsKey("PartName"))
                {
                    columnMap["PartName"] = col;
                }
            }
            
            // Vật liệu / Material
            if (headerLower.Contains("vật liệu") || headerLower.Contains("材料") || 
                headerLower == "material")
            {
                columnMap["Material"] = col;
            }
            
            // Mã màu / Color Code
            if (headerLower.Contains("mã màu") || headerLower.Contains("颜色代码") || 
                headerLower == "color code" || headerLower.Contains("colorcode"))
            {
                columnMap["ColorCode"] = col;
            }
            
            // Màu sắc / Color
            if ((headerLower.Contains("màu sắc") || headerLower.Contains("颜色")) && 
                !headerLower.Contains("mã màu") && !headerLower.Contains("颜色代码"))
            {
                columnMap["Color"] = col;
            }
            
            // Số lòng khuôn / Number of Cavities
            if (headerLower.Contains("số lòng khuôn") || headerLower.Contains("模腔数") || 
                headerLower.Contains("number of cavities") || headerLower.Contains("cavities"))
            {
                columnMap["NumberOfCavities"] = col;
            }
            
            // Chu kỳ / Cycle Time
            if (headerLower.Contains("chu kỳ") || headerLower.Contains("周期") || 
                headerLower.Contains("cycle") || headerLower.Contains("cycle time"))
            {
                columnMap["CycleTime"] = col;
            }
            
            // Trọng lượng / Weight
            if (headerLower.Contains("trọng lượng") || headerLower.Contains("重量") || 
                headerLower.Contains("weight"))
            {
                columnMap["Weight"] = col;
            }
            
            // Số lượng / Quantity
            if (headerLower.Contains("số lượng") || headerLower.Contains("数量") || 
                headerLower == "quantity" || headerLower.Contains("qty"))
            {
                columnMap["Quantity"] = col;
            }
            
            // Đơn giá / Unit Price
            if (headerLower.Contains("đơn giá") || headerLower.Contains("单价") || 
                headerLower.Contains("unit price") || headerLower.Contains("unitprice"))
            {
                columnMap["UnitPrice"] = col;
            }
            
            // Thành tiền / Total Amount
            if (headerLower.Contains("thành tiền") || headerLower.Contains("总金额") || 
                headerLower.Contains("total amount") || headerLower.Contains("totalamount"))
            {
                columnMap["TotalAmount"] = col;
            }
            
            // Tổng tiền / Total (khác với Thành tiền)
            if (headerLower.Contains("tổng tiền") && !headerLower.Contains("thành tiền"))
            {
                columnMap["TotalAmount"] = col; // Map vào TotalAmount
            }
            
            // Đơn giá chuẩn / Standard Unit Price
            if (headerLower.Contains("đơn giá chuẩn") || headerLower.Contains("标准单价") ||
                headerLower.Contains("standard unit price") || headerLower.Contains("standardunitprice"))
            {
                columnMap["StandardUnitPrice"] = col;
            }
            
            // Đơn giá hợp đồng / Contract Unit Price
            if (headerLower.Contains("đơn giá hợp đồng") || headerLower.Contains("合同单价") ||
                headerLower.Contains("contract unit price") || headerLower.Contains("contractunitprice"))
            {
                columnMap["ContractUnitPrice"] = col;
            }
            
            // Số lượng hợp đồng / Contract Quantity
            if (headerLower.Contains("số lượng hợp đồng") || headerLower.Contains("合同数量") ||
                headerLower.Contains("contract quantity") || headerLower.Contains("contractquantity"))
            {
                columnMap["ContractQuantity"] = col;
            }
            
            // Số lần gia công / Charge Count / Number of Processing Times
            if (headerLower.Contains("số lần gia công") || headerLower.Contains("加工次数") ||
                headerLower.Contains("charge count") || headerLower.Contains("chargecount") ||
                headerLower.Contains("number of processing times"))
            {
                columnMap["ChargeCount"] = col;
            }
            
            // Vị trí gia công / Processing Position
            if (headerLower.Contains("vị trí gia công") || headerLower.Contains("加工位置") ||
                headerLower.Contains("processing position"))
            {
                columnMap["ProcessingPosition"] = col;
            }
            
            // Công đoạn / Operation Step
            if (headerLower.Contains("công đoạn") || headerLower.Contains("工序") ||
                headerLower.Contains("operation step") || headerLower.Contains("operationstep"))
            {
                columnMap["OperationStep"] = col;
            }
            
            // Nội dung gia công / Processing Content
            if (headerLower.Contains("nội dung gia công") || headerLower.Contains("加工内容") ||
                headerLower.Contains("processing content"))
            {
                columnMap["ProcessingContent"] = col;
            }
            
            // Vị trí phun / Spray Position
            if (headerLower.Contains("vị trí phun") || headerLower.Contains("喷涂位置") ||
                headerLower.Contains("spray position") || headerLower.Contains("sprayposition"))
            {
                columnMap["SprayPosition"] = col;
            }
            
            // Nội dung in / Print Content
            if (headerLower.Contains("nội dung in") || headerLower.Contains("印刷内容") ||
                headerLower.Contains("print content") || headerLower.Contains("printcontent"))
            {
                columnMap["PrintContent"] = col;
            }
            
            // Nội dung lắp ráp / Assembly Content
            if (headerLower.Contains("nội dung lắp ráp") || headerLower.Contains("装配内容") ||
                headerLower.Contains("assembly content") || headerLower.Contains("assemblycontent"))
            {
                columnMap["AssemblyContent"] = col;
            }
            
            // Ghi chú / Notes
            if (headerLower.Contains("ghi chú") || headerLower.Contains("备注") ||
                headerLower == "notes" || headerLower == "note")
            {
                columnMap["Notes"] = col;
            }
            
            // Ngày hoàn thành / Completion Date
            if (headerLower.Contains("ngày hoàn thành") || headerLower.Contains("完成日期") ||
                headerLower.Contains("completion date") || headerLower.Contains("completiondate"))
            {
                columnMap["CompletionDate"] = col;
            }
            
            // STT / Sequence Order
            if (headerLower == "stt" || headerLower.Contains("序号") || 
                headerLower == "no" || headerLower.Contains("sequence"))
            {
                columnMap["SequenceOrder"] = col;
            }
        }
        
        return columnMap;
    }

    /// <summary>
    /// Lấy giá trị string theo tên cột
    /// </summary>
    private string GetValueByColumn(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMap, 
        string key, params string[] alternativeKeys)
    {
        // Thử key chính
        if (columnMap.TryGetValue(key, out int col) && col > 0)
        {
            return GetStringValue(worksheet, row, col);
        }
        
        // Thử các alternative keys
        foreach (var altKey in alternativeKeys)
        {
            if (columnMap.TryGetValue(altKey, out col) && col > 0)
            {
                return GetStringValue(worksheet, row, col);
            }
        }
        
        return string.Empty;
    }

    /// <summary>
    /// Lấy giá trị int theo tên cột
    /// </summary>
    private int GetIntValueByColumn(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMap, 
        string key, params string[] alternativeKeys)
    {
        // Thử key chính
        if (columnMap.TryGetValue(key, out int col) && col > 0)
        {
            return GetIntValue(worksheet, row, col);
        }
        
        // Thử các alternative keys
        foreach (var altKey in alternativeKeys)
        {
            if (columnMap.TryGetValue(altKey, out col) && col > 0)
            {
                return GetIntValue(worksheet, row, col);
            }
        }
        
        return 0;
    }

    /// <summary>
    /// Lấy giá trị decimal theo tên cột
    /// </summary>
    private decimal GetDecimalValueByColumn(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMap, 
        string key, params string[] alternativeKeys)
    {
        // Thử key chính
        if (columnMap.TryGetValue(key, out int col) && col > 0)
        {
            return GetDecimalValue(worksheet, row, col);
        }
        
        // Thử các alternative keys
        foreach (var altKey in alternativeKeys)
        {
            if (columnMap.TryGetValue(altKey, out col) && col > 0)
            {
                return GetDecimalValue(worksheet, row, col);
            }
        }
        
        return 0;
    }

    private bool IsEmptyRow(ExcelWorksheet worksheet, int row)
    {
        // Check if first 3 columns are empty
        for (int col = 1; col <= 3; col++)
        {
            var value = worksheet.Cells[row, col].Value;
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                return false;
            }
        }
        return true;
    }

    private string GetStringValue(ExcelWorksheet worksheet, int row, int col)
    {
        var value = worksheet.Cells[row, col].Value;
        return value?.ToString()?.Trim() ?? string.Empty;
    }

    private int GetIntValue(ExcelWorksheet worksheet, int row, int col)
    {
        var value = worksheet.Cells[row, col].Value;
        if (value == null) return 0;

        if (int.TryParse(value.ToString(), out int result))
            return result;

        if (double.TryParse(value.ToString(), out double doubleResult))
            return (int)Math.Round(doubleResult);

        return 0;
    }

    private decimal GetDecimalValue(ExcelWorksheet worksheet, int row, int col)
    {
        var value = worksheet.Cells[row, col].Value;
        if (value == null) return 0;

        if (decimal.TryParse(value.ToString(), out decimal result))
            return result;

        if (double.TryParse(value.ToString(), out double doubleResult))
            return (decimal)doubleResult;

        return 0;
    }

    /// <summary>
    /// Lấy giá trị DateTime theo tên cột
    /// </summary>
    private DateTime GetDateValueByColumn(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMap, 
        string key, params string[] alternativeKeys)
    {
        // Thử key chính
        if (columnMap.TryGetValue(key, out int col) && col > 0)
        {
            return GetDateValue(worksheet, row, col);
        }
        
        // Thử các alternative keys
        foreach (var altKey in alternativeKeys)
        {
            if (columnMap.TryGetValue(altKey, out col) && col > 0)
            {
                return GetDateValue(worksheet, row, col);
            }
        }
        
        return default;
    }

    /// <summary>
    /// Lấy giá trị DateTime từ cell
    /// </summary>
    private DateTime GetDateValue(ExcelWorksheet worksheet, int row, int col)
    {
        var value = worksheet.Cells[row, col].Value;
        if (value == null) return default;

        // Nếu là DateTime trực tiếp
        if (value is DateTime dateTime)
        {
            return dateTime;
        }

        // Nếu là double (Excel date serial number)
        if (double.TryParse(value.ToString(), out double doubleValue))
        {
            try
            {
                return DateTime.FromOADate(doubleValue);
            }
            catch
            {
                // Ignore
            }
        }

        // Thử parse string
        if (DateTime.TryParse(value.ToString(), out DateTime parsedDate))
        {
            return parsedDate;
        }

        return default;
    }

    private string GenerateCustomerCode()
    {
        return $"C-{DateTime.Now:yyyyMMddHHmmss}";
    }
    
    /// <summary>
    /// Parse NHAP_NGUYEN_VAT_LIEU sheet (Material Receipt - nhập kho thực tế)
    /// Columns: Mã nguyên vật liệu, Tên nguyên vật liệu, Loại nguyên vật liệu, Đơn vị tính,
    ///          Mã kho, Số lượng nhập, Số lô, Ngày nhập kho, Mã nhà cung cấp, 
    ///          Mã PO mua hàng, Số phiếu nhập, Ghi chú, Nhập đầu kỳ tháng
    /// </summary>
    private async Task<List<MaterialReceiptData>> ParseMaterialReceiptSheet(ExcelWorksheet worksheet)
    {
        var result = new List<MaterialReceiptData>();
        
        try
        {
            // Tìm header row
            int headerRow = FindHeaderRow(worksheet);
            if (headerRow == 0)
            {
                _logger.LogWarning("NHAP_NGUYEN_VAT_LIEU: Header row not found, skipping material receipt");
                return result;
            }
            
            // Parse header để tìm vị trí các cột
            var columnMap = ParseHeaderRow(worksheet, headerRow);
            
            _logger.LogInformation("NHAP_NGUYEN_VAT_LIEU: Parsed header row {HeaderRow}. Found columns: {Columns}",
                headerRow, string.Join(", ", columnMap.Keys));
            
            // Start reading data from row after header
            int startRow = headerRow + 1;
            int currentRow = startRow;
            
            while (!IsEmptyRow(worksheet, currentRow))
            {
                try
                {
                    var materialCode = GetValueByColumn(worksheet, currentRow, columnMap, "MaterialCode", 
                        "Mã nguyên vật liệu", "Mã vật liệu", "原料代码");
                    var materialName = GetValueByColumn(worksheet, currentRow, columnMap, "MaterialName", 
                        "Tên nguyên vật liệu", "Tên vật liệu", "原料名称");
                    
                    // Skip empty rows
                    if (string.IsNullOrWhiteSpace(materialCode) && string.IsNullOrWhiteSpace(materialName))
                    {
                        currentRow++;
                        continue;
                    }
                    
                    var materialType = GetValueByColumn(worksheet, currentRow, columnMap, "MaterialType", 
                        "Loại nguyên vật liệu", "材料类型");
                    var unit = GetValueByColumn(worksheet, currentRow, columnMap, "Unit", 
                        "Đơn vị tính", "Đơn vị", "单位");
                    var warehouseCode = GetValueByColumn(worksheet, currentRow, columnMap, "WarehouseCode", 
                        "Mã kho", "仓库代码");
                    var quantity = GetDecimalValueByColumn(worksheet, currentRow, columnMap, "Quantity", 
                        "Số lượng nhập", "Số lượng", "数量");
                    var batchNumber = GetValueByColumn(worksheet, currentRow, columnMap, "BatchNumber", 
                        "Số lô", "批次号");
                    var receiptDate = GetDateValueByColumn(worksheet, currentRow, columnMap, "ReceiptDate", 
                        "Ngày nhập kho", "入库日期");
                    var supplierCode = GetValueByColumn(worksheet, currentRow, columnMap, "SupplierCode", 
                        "Mã nhà cung cấp", "供应商代码");
                    var purchasePOCode = GetValueByColumn(worksheet, currentRow, columnMap, "PurchasePOCode", 
                        "Mã PO mua hàng", "采购订单代码");
                    var receiptNumber = GetValueByColumn(worksheet, currentRow, columnMap, "ReceiptNumber", 
                        "Số phiếu nhập", "入库单号");
                    var notes = GetValueByColumn(worksheet, currentRow, columnMap, "Notes", 
                        "Ghi chú", "备注");
                    var beginningPeriodMonth = GetIntValueByColumn(worksheet, currentRow, columnMap, "BeginningPeriodMonth", 
                        "Nhập đầu kỳ tháng", "期初月份");
                    
                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(materialCode))
                    {
                        _logger.LogWarning("NHAP_NGUYEN_VAT_LIEU Row {Row}: MaterialCode is empty, skipping", currentRow);
                        currentRow++;
                        continue;
                    }
                    
                    if (quantity <= 0)
                    {
                        _logger.LogWarning("NHAP_NGUYEN_VAT_LIEU Row {Row}: Quantity must be > 0, skipping", currentRow);
                        currentRow++;
                        continue;
                    }
                    
                    // Nếu không có ngày nhập kho, dùng ngày hiện tại
                    if (receiptDate == default)
                    {
                        receiptDate = DateTime.UtcNow;
                    }
                    
                    // Tạo số phiếu nhập nếu không có
                    if (string.IsNullOrWhiteSpace(receiptNumber))
                    {
                        receiptNumber = $"PNK-{DateTime.Now:yyyy-MM-dd}-{currentRow}";
                    }
                    
                    // Thêm thông tin nhập đầu kỳ vào notes nếu có
                    if (beginningPeriodMonth > 0)
                    {
                        if (string.IsNullOrWhiteSpace(notes))
                        {
                            notes = $"Nhập đầu kỳ tháng {beginningPeriodMonth}";
                        }
                        else
                        {
                            notes = $"{notes}; Nhập đầu kỳ tháng {beginningPeriodMonth}";
                        }
                    }
                    
                    var materialReceipt = new MaterialReceiptData
                    {
                        MaterialCode = materialCode,
                        MaterialName = materialName,
                        MaterialType = materialType,
                        Unit = unit,
                        WarehouseCode = warehouseCode,
                        Quantity = quantity,
                        BatchNumber = batchNumber,
                        ReceiptDate = receiptDate,
                        SupplierCode = supplierCode,
                        PurchasePOCode = purchasePOCode,
                        ReceiptNumber = receiptNumber,
                        Notes = notes
                    };
                    
                    result.Add(materialReceipt);
                    _logger.LogDebug("NHAP_NGUYEN_VAT_LIEU Row {Row}: Added material receipt {MaterialCode} - {Quantity} {Unit} on {ReceiptDate}",
                        currentRow, materialReceipt.MaterialCode, materialReceipt.Quantity, materialReceipt.Unit, materialReceipt.ReceiptDate);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "NHAP_NGUYEN_VAT_LIEU: Error parsing row {Row}, skipping", currentRow);
                }
                
                currentRow++;
            }
            
            _logger.LogInformation("NHAP_NGUYEN_VAT_LIEU: Parsed {Count} material receipt entries", result.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NHAP_NGUYEN_VAT_LIEU: Error parsing material receipt sheet");
        }
        
        return result;
    }

    /// <summary>
    /// Parse NHAP_NGUYEN_VAT_LIEU sheet (Material Baseline) - DEPRECATED, kept for backward compatibility
    /// Columns: Mã sản phẩm, Tên sản phẩm, Mã linh kiện, Tên linh kiện, Mã nguyên vật liệu, 
    ///          Tên nguyên vật liệu, Số lượng, Đơn vị, Ghi chú
    /// </summary>
    private async Task<List<POMaterialBaselineData>> ParseMaterialBaselineSheet(ExcelWorksheet worksheet)
    {
        var result = new List<POMaterialBaselineData>();
        
        try
        {
            // Tìm header row
            int headerRow = FindHeaderRow(worksheet);
            if (headerRow == 0)
            {
                _logger.LogWarning("NHAP_NGUYEN_VAT_LIEU: Header row not found, skipping material baseline");
                return result;
            }
            
            // Parse header để tìm vị trí các cột
            var columnMap = ParseHeaderRow(worksheet, headerRow);
            
            _logger.LogInformation("NHAP_NGUYEN_VAT_LIEU: Parsed header row {HeaderRow}. Found columns: {Columns}",
                headerRow, string.Join(", ", columnMap.Keys));
            
            // Start reading data from row after header
            int startRow = headerRow + 1;
            int currentRow = startRow;
            
            while (!IsEmptyRow(worksheet, currentRow))
            {
                try
                {
                    var materialCode = GetValueByColumn(worksheet, currentRow, columnMap, "MaterialCode", 
                        "Mã nguyên vật liệu", "Mã vật liệu", "原料代码");
                    var materialName = GetValueByColumn(worksheet, currentRow, columnMap, "MaterialName", 
                        "Tên nguyên vật liệu", "Tên vật liệu", "原料名称");
                    
                    // Skip empty rows
                    if (string.IsNullOrWhiteSpace(materialCode) && string.IsNullOrWhiteSpace(materialName))
                    {
                        currentRow++;
                        continue;
                    }
                    
                    var materialBaseline = new POMaterialBaselineData
                    {
                        ProductCode = GetValueByColumn(worksheet, currentRow, columnMap, "ProductCode", 
                            "Mã sản phẩm", "产品代码"),
                        ProductName = GetValueByColumn(worksheet, currentRow, columnMap, "ProductName", 
                            "Tên sản phẩm", "产品名称"),
                        PartCode = GetValueByColumn(worksheet, currentRow, columnMap, "PartCode", 
                            "Mã linh kiện", "零件代码"),
                        PartName = GetValueByColumn(worksheet, currentRow, columnMap, "PartName", 
                            "Tên linh kiện", "零件名称"),
                        MaterialCode = materialCode,
                        MaterialName = materialName,
                        CommittedQuantity = GetDecimalValueByColumn(worksheet, currentRow, columnMap, "Quantity", 
                            "Số lượng", "数量"),
                        Unit = GetValueByColumn(worksheet, currentRow, columnMap, "Unit", 
                            "Đơn vị", "单位"),
                        Notes = GetValueByColumn(worksheet, currentRow, columnMap, "Notes", 
                            "Ghi chú", "备注")
                    };
                    
                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(materialBaseline.MaterialCode))
                    {
                        _logger.LogWarning("NHAP_NGUYEN_VAT_LIEU Row {Row}: MaterialCode is empty, skipping", currentRow);
                        currentRow++;
                        continue;
                    }
                    
                    if (materialBaseline.CommittedQuantity <= 0)
                    {
                        _logger.LogWarning("NHAP_NGUYEN_VAT_LIEU Row {Row}: CommittedQuantity must be > 0, skipping", currentRow);
                        currentRow++;
                        continue;
                    }
                    
                    result.Add(materialBaseline);
                    _logger.LogDebug("NHAP_NGUYEN_VAT_LIEU Row {Row}: Added material {MaterialCode} - {Quantity} {Unit}",
                        currentRow, materialBaseline.MaterialCode, materialBaseline.CommittedQuantity, materialBaseline.Unit);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "NHAP_NGUYEN_VAT_LIEU: Error parsing row {Row}, skipping", currentRow);
                }
                
                currentRow++;
            }
            
            _logger.LogInformation("NHAP_NGUYEN_VAT_LIEU: Parsed {Count} material baseline entries", result.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NHAP_NGUYEN_VAT_LIEU: Error parsing material baseline sheet");
        }
        
        return result;
    }

    /// <summary>
    /// Chuẩn hóa chuỗi tiếng Việt (loại bỏ dấu) để so sánh tốt hơn
    /// </summary>
    private string NormalizeVietnamese(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var normalized = text.ToLower();
        
        // Loại bỏ dấu tiếng Việt
        normalized = normalized.Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
            .Replace("ă", "a").Replace("ắ", "a").Replace("ằ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a")
            .Replace("â", "a").Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
            .Replace("é", "e").Replace("è", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
            .Replace("ê", "e").Replace("ế", "e").Replace("ề", "e").Replace("ể", "e").Replace("ễ", "e").Replace("ệ", "e")
            .Replace("í", "i").Replace("ì", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
            .Replace("ó", "o").Replace("ò", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
            .Replace("ô", "o").Replace("ố", "o").Replace("ồ", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ộ", "o")
            .Replace("ơ", "o").Replace("ớ", "o").Replace("ờ", "o").Replace("ở", "o").Replace("ỡ", "o").Replace("ợ", "o")
            .Replace("ú", "u").Replace("ù", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
            .Replace("ư", "u").Replace("ứ", "u").Replace("ừ", "u").Replace("ử", "u").Replace("ữ", "u").Replace("ự", "u")
            .Replace("ý", "y").Replace("ỳ", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y")
            .Replace("đ", "d");
        
        return normalized;
    }

    #endregion
}

/// <summary>
/// Kết quả import Excel (PHASE 1: 2-sheet format)
/// </summary>
public class ExcelImportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string TemplateType { get; set; } = string.Empty;
    public List<POOperationData> Operations { get; set; } = new();
    public List<POMaterialBaselineData> MaterialBaselines { get; set; } = new(); // Deprecated, kept for backward compatibility
    public List<MaterialReceiptData> MaterialReceipts { get; set; } = new(); // New: Material Receipt data
    public List<string> Errors { get; set; } = new();
    
    // Customer info
    public string? CustomerName { get; set; }
    public string? CustomerCode { get; set; }
    public bool ShouldCreateCustomer { get; set; }
}

/// <summary>
/// Data model cho PO Operation từ Excel
/// </summary>
public class POOperationData
{
    public int SequenceOrder { get; set; }
    
    // Thông tin sản phẩm
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    
    // Thông tin linh kiện
    public string PartCode { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public string ProcessingTypeName { get; set; } = string.Empty;
    
    // Thông tin khuôn (cho ÉP NHỰA)
    public string? MoldCode { get; set; }
    public int? NumberOfCavities { get; set; }
    
    // ÉP NHỰA fields
    public string? Material { get; set; }
    public string? ColorCode { get; set; }
    public string? Color { get; set; }
    public decimal? Weight { get; set; }
    public decimal? CycleTime { get; set; }
    
    // LẮP RÁP fields
    public string? AssemblyContent { get; set; }
    public string? ProcessingContent { get; set; } // Nội dung gia công
    
    // PHUN IN fields
    public string? SprayPosition { get; set; }
    public string? PrintContent { get; set; }
    public string? ProcessingPosition { get; set; } // Vị trí gia công
    public string? OperationStep { get; set; } // Công đoạn
    
    // Common fields
    public int Quantity { get; set; }
    public int? ContractQuantity { get; set; } // Số lượng hợp đồng
    public decimal UnitPrice { get; set; }
    public decimal? StandardUnitPrice { get; set; } // Đơn giá chuẩn
    public decimal? ContractUnitPrice { get; set; } // Đơn giá hợp đồng
    public decimal TotalAmount { get; set; }
    public int? ChargeCount { get; set; } // Số lần gia công
    
    // Additional fields
    public DateTime? CompletionDate { get; set; } // Ngày hoàn thành
    public string? Notes { get; set; } // Ghi chú
}

/// <summary>
/// Data model cho PO Material Baseline từ Excel (Sheet 2: NHAP_NGUYEN_VAT_LIEU)
/// DEPRECATED: Sử dụng MaterialReceiptData thay thế
/// </summary>
public class POMaterialBaselineData
{
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public string? PartCode { get; set; }
    public string? PartName { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public decimal CommittedQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

/// <summary>
/// Data model cho Material Receipt từ Excel (Sheet 2: NHAP_NGUYEN_VAT_LIEU)
/// </summary>
public class MaterialReceiptData
{
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string? MaterialType { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime ReceiptDate { get; set; }
    public string? SupplierCode { get; set; }
    public string? PurchasePOCode { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
}



