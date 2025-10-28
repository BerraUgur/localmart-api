using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WebAPI.Services.Concrete;
using WebAPI.Models;

namespace WebAPI.Endpoints
{
    public static class LogEndpoints
    {
        public static void RegisterLogEndpoints(this WebApplication app)
        {
            app.MapGet("/logs", async (LogService logService) =>
            {
                var logs = await logService.GetAllLogsAsync();
                return Results.Ok(logs);
            });

            app.MapPost("/logs", async (Log log, LogService logService) =>
            {
                await logService.AddLogAsync(log);
                return Results.Ok();
            });
        }
    }
}