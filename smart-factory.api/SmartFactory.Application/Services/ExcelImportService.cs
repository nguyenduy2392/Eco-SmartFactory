using OfficeOpenXml;
using SmartFactory.Application.Entities;
using Microsoft.Extensions.Logging;

namespace SmartFactory.Application.Services;

/// <summary>
/// Service để import PO từ Excel cho 3 loại template:
/// - ÉP NHỰA (ep_nhua): có trọng lượng, chu kỳ ép
/// - LẮP RÁP (lap_rap): có nội dung lắp ráp
/// - PHUN IN (phun_in): có vị trí phun, nội dung in
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
    /// Import PO từ file Excel
    /// </summary>
    public async Task<ExcelImportResult> ImportPOFromExcel(Stream fileStream, string templateType)
    {
        try
        {
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0]; // First sheet

            return templateType.ToUpper() switch
            {
                "EP_NHUA" => await ParseEpNhuaTemplate(worksheet),
                "LAP_RAP" => await ParseLapRapTemplate(worksheet),
                "PHUN_IN" => await ParsePhunInTemplate(worksheet),
                _ => throw new ArgumentException($"Unknown template type: {templateType}")
            };
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
    /// Cột: STT, Mã linh kiện, Tên linh kiện, Vật liệu, Màu, Trọng lượng (g), Chu kỳ (s), Số lượng, Đơn giá, Thành tiền
    /// </summary>
    private async Task<ExcelImportResult> ParseEpNhuaTemplate(ExcelWorksheet worksheet)
    {
        var result = new ExcelImportResult { TemplateType = "EP_NHUA" };

        // Start from row 2 (row 1 is header)
        int startRow = 2;
        int currentRow = startRow;

        while (!IsEmptyRow(worksheet, currentRow))
        {
            try
            {
                var operation = new POOperationData
                {
                    SequenceOrder = GetIntValue(worksheet, currentRow, 1), // STT
                    PartCode = GetStringValue(worksheet, currentRow, 2), // Mã linh kiện
                    PartName = GetStringValue(worksheet, currentRow, 3), // Tên linh kiện
                    Material = GetStringValue(worksheet, currentRow, 4), // Vật liệu
                    Color = GetStringValue(worksheet, currentRow, 5), // Màu
                    Weight = GetDecimalValue(worksheet, currentRow, 6), // Trọng lượng
                    CycleTime = GetDecimalValue(worksheet, currentRow, 7), // Chu kỳ
                    Quantity = GetIntValue(worksheet, currentRow, 8), // Số lượng
                    UnitPrice = GetDecimalValue(worksheet, currentRow, 9), // Đơn giá
                    TotalAmount = GetDecimalValue(worksheet, currentRow, 10), // Thành tiền
                    ProcessingTypeName = "ÉP NHỰA"
                };

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
    /// Cột: STT, Mã linh kiện, Tên linh kiện, Nội dung lắp ráp, Số lượng, Đơn giá, Thành tiền
    /// </summary>
    private async Task<ExcelImportResult> ParseLapRapTemplate(ExcelWorksheet worksheet)
    {
        var result = new ExcelImportResult { TemplateType = "LAP_RAP" };

        int startRow = 2;
        int currentRow = startRow;

        while (!IsEmptyRow(worksheet, currentRow))
        {
            try
            {
                var operation = new POOperationData
                {
                    SequenceOrder = GetIntValue(worksheet, currentRow, 1), // STT
                    PartCode = GetStringValue(worksheet, currentRow, 2), // Mã linh kiện
                    PartName = GetStringValue(worksheet, currentRow, 3), // Tên linh kiện
                    AssemblyContent = GetStringValue(worksheet, currentRow, 4), // Nội dung lắp ráp
                    Quantity = GetIntValue(worksheet, currentRow, 5), // Số lượng
                    UnitPrice = GetDecimalValue(worksheet, currentRow, 6), // Đơn giá
                    TotalAmount = GetDecimalValue(worksheet, currentRow, 7), // Thành tiền
                    ProcessingTypeName = "LẮP RÁP"
                };

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
    /// Cột: STT, Mã linh kiện, Tên linh kiện, Vị trí phun, Nội dung in, Màu sơn, Số lượng, Đơn giá, Thành tiền
    /// </summary>
    private async Task<ExcelImportResult> ParsePhunInTemplate(ExcelWorksheet worksheet)
    {
        var result = new ExcelImportResult { TemplateType = "PHUN_IN" };

        int startRow = 2;
        int currentRow = startRow;

        while (!IsEmptyRow(worksheet, currentRow))
        {
            try
            {
                var operation = new POOperationData
                {
                    SequenceOrder = GetIntValue(worksheet, currentRow, 1), // STT
                    PartCode = GetStringValue(worksheet, currentRow, 2), // Mã linh kiện
                    PartName = GetStringValue(worksheet, currentRow, 3), // Tên linh kiện
                    SprayPosition = GetStringValue(worksheet, currentRow, 4), // Vị trí phun
                    PrintContent = GetStringValue(worksheet, currentRow, 5), // Nội dung in
                    Color = GetStringValue(worksheet, currentRow, 6), // Màu sơn
                    Quantity = GetIntValue(worksheet, currentRow, 7), // Số lượng
                    UnitPrice = GetDecimalValue(worksheet, currentRow, 8), // Đơn giá
                    TotalAmount = GetDecimalValue(worksheet, currentRow, 9), // Thành tiền
                    ProcessingTypeName = "PHUN IN"
                };

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

    #endregion
}

/// <summary>
/// Kết quả import Excel
/// </summary>
public class ExcelImportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string TemplateType { get; set; } = string.Empty;
    public List<POOperationData> Operations { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Data model cho PO Operation từ Excel
/// </summary>
public class POOperationData
{
    public int SequenceOrder { get; set; }
    public string PartCode { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public string ProcessingTypeName { get; set; } = string.Empty;
    
    // ÉP NHỰA fields
    public string? Material { get; set; }
    public string? Color { get; set; }
    public decimal? Weight { get; set; }
    public decimal? CycleTime { get; set; }
    
    // LẮP RÁP fields
    public string? AssemblyContent { get; set; }
    
    // PHUN IN fields
    public string? SprayPosition { get; set; }
    public string? PrintContent { get; set; }
    
    // Common fields
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
}

