namespace WebAPI.Models;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }

    public ErrorResponse() { }

    public ErrorResponse(int statusCode, string message, string? details = null, string? traceId = null)
    {
        StatusCode = statusCode;
        Message = message;
        Details = details;
        TraceId = traceId;
    }
}
