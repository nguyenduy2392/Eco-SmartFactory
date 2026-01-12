namespace SmartFactory.Application.DTOs;

/// <summary>
/// Standardized error response model
/// </summary>
public class ErrorResponse
{
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public IDictionary<string, string[]>? ValidationErrors { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
    public string? TraceId { get; set; }
}
