using WebAPI.Models;
using WebAPI.Services.Concrete;

namespace WebAPI.Middleware;

public class ExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, LogService logService)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var ip = context.Connection?.RemoteIpAddress?.ToString();
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var userType = DetermineUserType(context);
            
            var log = new Log
            {
                Level = "Error",
                Message = ex.Message,
                Exception = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Endpoint = context.Request.Path,
                User = context.User?.Identity?.Name,
                Details = $"RequestId: {context.TraceIdentifier}",
                Ip = ip,
                UserType = userType,
                UserAgent = userAgent
            };
            
            await logService.AddLogAsync(log);
            throw;
        }
    }

    private static string DetermineUserType(HttpContext context)
    {
        if (context.User?.IsInRole("Admin") == true)
            return "Admin";
        
        if (context.User?.Identity?.IsAuthenticated == true)
            return "User";
        
        return "Anonymous";
    }
}

