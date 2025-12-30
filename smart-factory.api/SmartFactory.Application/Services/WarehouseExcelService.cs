using OfficeOpenXml;
using SmartFactory.Application.Entities;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.DTOs;

namespace SmartFactory.Application.Services;

/// <summary>
/// Service để import/export Excel cho warehouse operations
/// Import: Nhập kho từ Excel (mỗi dòng = 1 lần nhập)
/// Export: Xuất lịch sử nhập/xuất kho
/// </summary>
public class WarehouseExcelService
{
    private readonly ILogger<WarehouseExcelService> _logger;

    public WarehouseExcelService(ILogger<WarehouseExcelService> logger)
    {
        _logger = logger;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    /// <summary>
    /// Import Material Receipts từ Excel
    /// Format: Mỗi dòng = 1 lần nhập kho
    /// Columns: Mã nguyên vật liệu, Tên nguyên vật liệu, Loại nguyên vật liệu, Đơn vị tính,
    /// Mã kho, Số lượng nhập, Số lô, Ngày nhập kho, Mã nhà cung cấp, Mã PO mua hàng, Số phiếu nhập, Ghi chú
    /// </summary>
    public async Task<WarehouseImportResult> ImportMaterialReceiptsFromExcel(Stream fileStream, Guid customerId)
    {
        var result = new WarehouseImportResult { CustomerId = customerId };

        try
        {
            using var package = new ExcelPackage(fileStream);
            
            if (package.Workbook.Worksheets.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = "Excel file không có sheet nào";
                return result;
            }

            var worksheet = package.Workbook.Worksheets[0];
            
            // Tìm header row
            int headerRow = FindHeaderRow(worksheet);
            if (headerRow == 0)
            {
                result.Success = false;
                result.ErrorMessage = "Không tìm thấy header row trong file Excel";
                return result;
            }

            // Parse header để tìm vị trí các cột
            var columnMap = ParseMaterialReceiptHeader(worksheet, headerRow);
            
            // Validate required columns
            var requiredColumns = new[] { "MaterialCode", "MaterialName", "Unit", "WarehouseCode", "Quantity", "BatchNumber", "ReceiptDate" };
            foreach (var col in requiredColumns)
            {
                if (!columnMap.ContainsKey(col))
                {
                    result.Success = false;
                    result.ErrorMessage = $"Thiếu cột bắt buộc: {col}";
                    return result;
                }
            }

            // Parse data rows
            int startRow = headerRow + 1;
            int currentRow = startRow;
            int rowNumber = startRow;

            while (!IsEmptyRow(worksheet, currentRow))
            {
                try
                {
                    var receiptData = ParseMaterialReceiptRow(worksheet, currentRow, columnMap, rowNumber);
                    if (receiptData != null)
                    {
                        result.Receipts.Add(receiptData);
                    }
                    else
                    {
                        result.Errors.Add($"Dòng {rowNumber}: Không thể parse dữ liệu");
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Dòng {rowNumber}: {ex.Message}");
                }

                currentRow++;
                rowNumber++;
            }

            result.Success = result.Errors.Count == 0;
            _logger.LogInformation("Imported {Count} material receipts from Excel", result.Receipts.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing material receipts from Excel");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    /// <summary>
    /// Export Material Transaction History to Excel
    /// </summary>
    public async Task<byte[]> ExportTransactionHistoryToExcel(List<MaterialTransactionHistoryDto> history)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Lịch sử kho");

        // Header
        worksheet.Cells[1, 1].Value = "Thời gian";
        worksheet.Cells[1, 2].Value = "Loại giao dịch";
        worksheet.Cells[1, 3].Value = "Mã nguyên vật liệu";
        worksheet.Cells[1, 4].Value = "Tên nguyên vật liệu";
        worksheet.Cells[1, 5].Value = "Số lô";
        worksheet.Cells[1, 6].Value = "Số phiếu";
        worksheet.Cells[1, 7].Value = "Tồn trước";
        worksheet.Cells[1, 8].Value = "Thay đổi";
        worksheet.Cells[1, 9].Value = "Tồn sau";
        worksheet.Cells[1, 10].Value = "Đơn vị";
        worksheet.Cells[1, 11].Value = "Kho";
        worksheet.Cells[1, 12].Value = "Ghi chú";
        worksheet.Cells[1, 13].Value = "Người thao tác";

        // Style header
        using (var range = worksheet.Cells[1, 1, 1, 13])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Data
        int row = 2;
        foreach (var h in history)
        {
            worksheet.Cells[row, 1].Value = h.TransactionDate;
            worksheet.Cells[row, 1].Style.Numberformat.Format = "dd/mm/yyyy hh:mm";
            worksheet.Cells[row, 2].Value = GetTransactionTypeLabel(h.TransactionType);
            worksheet.Cells[row, 3].Value = h.MaterialCode;
            worksheet.Cells[row, 4].Value = h.MaterialName;
            worksheet.Cells[row, 5].Value = h.BatchNumber;
            worksheet.Cells[row, 6].Value = h.ReferenceNumber;
            worksheet.Cells[row, 7].Value = h.StockBefore;
            worksheet.Cells[row, 8].Value = h.QuantityChange;
            worksheet.Cells[row, 9].Value = h.StockAfter;
            worksheet.Cells[row, 10].Value = h.Unit;
            worksheet.Cells[row, 11].Value = h.WarehouseCode;
            worksheet.Cells[row, 12].Value = h.Notes;
            worksheet.Cells[row, 13].Value = h.CreatedBy;
            row++;
        }

        // Auto-fit columns
        worksheet.Cells.AutoFitColumns();

        return await Task.FromResult(package.GetAsByteArray());
    }

    #region Helper Methods

    private int FindHeaderRow(ExcelWorksheet worksheet)
    {
        for (int row = 1; row <= 10; row++)
        {
            var cellValue = worksheet.Cells[row, 1].Text?.ToLower().Trim();
            if (cellValue != null && (
                cellValue.Contains("mã nguyên vật liệu") ||
                cellValue.Contains("material code") ||
                cellValue == "mã vt"))
            {
                return row;
            }
        }
        return 0;
    }

    private Dictionary<string, int> ParseMaterialReceiptHeader(ExcelWorksheet worksheet, int headerRow)
    {
        var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int col = 1; col <= 20; col++)
        {
            var headerText = worksheet.Cells[headerRow, col].Text?.Trim().ToLower() ?? "";
            
            if (headerText.Contains("mã nguyên vật liệu") || headerText.Contains("material code") || headerText == "mã vt")
                columnMap["MaterialCode"] = col;
            else if (headerText.Contains("tên nguyên vật liệu") || headerText.Contains("material name") || headerText == "tên vật tư")
                columnMap["MaterialName"] = col;
            else if (headerText.Contains("loại nguyên vật liệu") || headerText.Contains("material type") || headerText == "loại")
                columnMap["MaterialType"] = col;
            else if (headerText.Contains("đơn vị tính") || headerText.Contains("unit"))
                columnMap["Unit"] = col;
            else if (headerText.Contains("mã kho") || headerText.Contains("warehouse code"))
                columnMap["WarehouseCode"] = col;
            else if (headerText.Contains("số lượng nhập") || headerText.Contains("quantity"))
                columnMap["Quantity"] = col;
            else if (headerText.Contains("số lô") || headerText.Contains("batch number") || headerText == "lô")
                columnMap["BatchNumber"] = col;
            else if (headerText.Contains("ngày nhập kho") || headerText.Contains("receipt date") || headerText.Contains("ngày nhập"))
                columnMap["ReceiptDate"] = col;
            else if (headerText.Contains("mã nhà cung cấp") || headerText.Contains("supplier code"))
                columnMap["SupplierCode"] = col;
            else if (headerText.Contains("mã po mua hàng") || headerText.Contains("purchase po code"))
                columnMap["PurchasePOCode"] = col;
            else if (headerText.Contains("số phiếu nhập") || headerText.Contains("receipt number"))
                columnMap["ReceiptNumber"] = col;
            else if (headerText.Contains("ghi chú") || headerText.Contains("notes"))
                columnMap["Notes"] = col;
        }

        return columnMap;
    }

    private MaterialReceiptImportData? ParseMaterialReceiptRow(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMap, int rowNumber)
    {
        try
        {
            var data = new MaterialReceiptImportData();

            // Required fields
            data.MaterialCode = GetCellValue(worksheet, row, columnMap["MaterialCode"]) ?? "";
            data.MaterialName = GetCellValue(worksheet, row, columnMap["MaterialName"]) ?? "";
            data.Unit = GetCellValue(worksheet, row, columnMap["Unit"]) ?? "";
            data.WarehouseCode = GetCellValue(worksheet, row, columnMap["WarehouseCode"]) ?? "";
            
            if (columnMap.ContainsKey("Quantity"))
            {
                var qtyStr = GetCellValue(worksheet, row, columnMap["Quantity"]);
                if (!decimal.TryParse(qtyStr, out decimal qty))
                {
                    throw new Exception($"Số lượng không hợp lệ: {qtyStr}");
                }
                data.Quantity = qty;
            }

            if (columnMap.ContainsKey("BatchNumber"))
            {
                data.BatchNumber = GetCellValue(worksheet, row, columnMap["BatchNumber"]) ?? "";
                if (string.IsNullOrWhiteSpace(data.BatchNumber))
                {
                    throw new Exception("Số lô là bắt buộc");
                }
            }

            if (columnMap.ContainsKey("ReceiptDate"))
            {
                var dateStr = GetCellValue(worksheet, row, columnMap["ReceiptDate"]);
                if (DateTime.TryParse(dateStr, out DateTime date))
                {
                    data.ReceiptDate = date;
                }
                else if (double.TryParse(dateStr, out double oaDate))
                {
                    // Excel date serial number
                    data.ReceiptDate = DateTime.FromOADate(oaDate);
                }
                else
                {
                    throw new Exception($"Ngày nhập kho không hợp lệ: {dateStr}");
                }
            }

            // Optional fields
            if (columnMap.ContainsKey("MaterialType"))
                data.MaterialType = GetCellValue(worksheet, row, columnMap["MaterialType"]);
            
            if (columnMap.ContainsKey("SupplierCode"))
                data.SupplierCode = GetCellValue(worksheet, row, columnMap["SupplierCode"]);
            
            if (columnMap.ContainsKey("PurchasePOCode"))
                data.PurchasePOCode = GetCellValue(worksheet, row, columnMap["PurchasePOCode"]);
            
            if (columnMap.ContainsKey("ReceiptNumber"))
                data.ReceiptNumber = GetCellValue(worksheet, row, columnMap["ReceiptNumber"]) ?? $"PNK-{DateTime.Now:yyyyMMdd}-{rowNumber}";
            else
                data.ReceiptNumber = $"PNK-{DateTime.Now:yyyyMMdd}-{rowNumber}";
            
            if (columnMap.ContainsKey("Notes"))
                data.Notes = GetCellValue(worksheet, row, columnMap["Notes"]);

            // Validate
            if (string.IsNullOrWhiteSpace(data.MaterialCode))
                throw new Exception("Mã nguyên vật liệu là bắt buộc");
            
            if (data.Quantity <= 0)
                throw new Exception("Số lượng nhập phải lớn hơn 0");

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error parsing row {Row}: {Error}", row, ex.Message);
            throw;
        }
    }

    private string? GetCellValue(ExcelWorksheet worksheet, int row, int col)
    {
        return worksheet.Cells[row, col].Text?.Trim();
    }

    private bool IsEmptyRow(ExcelWorksheet worksheet, int row)
    {
        for (int col = 1; col <= 20; col++)
        {
            if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, col].Text))
                return false;
        }
        return true;
    }

    private string GetTransactionTypeLabel(string type)
    {
        return type switch
        {
            "RECEIPT" => "Nhập kho",
            "ISSUE" => "Xuất kho",
            "ADJUSTMENT" => "Điều chỉnh",
            _ => type
        };
    }

    #endregion
}

/// <summary>
/// Result của import Excel
/// </summary>
public class WarehouseImportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid CustomerId { get; set; }
    public List<MaterialReceiptImportData> Receipts { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Data model cho Material Receipt import từ Excel
/// </summary>
public class MaterialReceiptImportData
{
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string? MaterialType { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public string? SupplierCode { get; set; }
    public string? PurchasePOCode { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

