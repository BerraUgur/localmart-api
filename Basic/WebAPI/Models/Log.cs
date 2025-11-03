using System;

namespace WebAPI.Models
{
    public class Log
    {
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Level { get; set; } // Error, Warning, Info, etc.
    public string? Message { get; set; }
    public string? Exception { get; set; }
    public string? StackTrace { get; set; }
    public string? Endpoint { get; set; }
    public string? User { get; set; }
    public string? Details { get; set; }
    public string? Ip { get; set; }
    public string? UserType { get; set; }
    public string? UserAgent { get; set; }
    }
}