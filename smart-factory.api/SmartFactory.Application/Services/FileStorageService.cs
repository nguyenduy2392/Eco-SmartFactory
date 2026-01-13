using Microsoft.Extensions.Logging;

namespace SmartFactory.Application.Services;

/// <summary>
/// Service để lưu file (ảnh) từ Excel import
/// Lưu vào thư mục wwwroot/uploads/parts
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Lưu ảnh từ byte array và trả về relative URL
    /// </summary>
    Task<string> SavePartImageAsync(byte[] imageBytes, string partCode);
    
    /// <summary>
    /// Xóa ảnh cũ nếu có
    /// </summary>
    Task DeletePartImageAsync(string imageUrl);
}

public class FileStorageService : IFileStorageService
{
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _uploadPath;
    
    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger;
        
        // Đường dẫn lưu: wwwroot/uploads/parts
        var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        _uploadPath = Path.Combine(wwwrootPath, "uploads", "parts");
        
        // Tạo thư mục nếu chưa có
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
            _logger.LogInformation("Created upload directory: {Path}", _uploadPath);
        }
    }
    
    public async Task<string> SavePartImageAsync(byte[] imageBytes, string partCode)
    {
        try
        {
            if (imageBytes == null || imageBytes.Length == 0)
            {
                return string.Empty;
            }
            
            // Tạo tên file: YYYY_MM_DD_HH_mm_ss.png
            var timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
            var fileName = $"{timestamp}.png";
            var filePath = Path.Combine(_uploadPath, fileName);
            
            // Lưu file
            await File.WriteAllBytesAsync(filePath, imageBytes);
            
            // Trả về relative URL (để frontend gọi)
            var relativeUrl = $"/uploads/parts/{fileName}";
            
            _logger.LogInformation("Saved part image: {FileName} ({Size} bytes)", fileName, imageBytes.Length);
            
            return relativeUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving part image for {PartCode}", partCode);
            return string.Empty;
        }
    }
    
    public async Task DeletePartImageAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }
            
            // Extract filename từ URL: /uploads/parts/ABC_20260113.png -> ABC_20260113.png
            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(_uploadPath, fileName);
            
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                _logger.LogInformation("Deleted part image: {FileName}", fileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting part image: {ImageUrl}", imageUrl);
        }
    }
}
