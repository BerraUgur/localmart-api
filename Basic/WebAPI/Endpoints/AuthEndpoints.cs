using System.Net.Mail;
using System.Net;
using WebAPI.Filters;
using WebAPI.ModelViews;
using WebAPI.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Endpoints;

public record MailRequest(string To, string Subject, string Body);
public static class AuthEndpoints
{
    private static readonly string[] BrSeparator = ["</br>"];
    public static void RegisterAuthEndpoints(this WebApplication app)
    {
        var config = app.Services.GetRequiredService<IConfiguration>();
        var smtpSection = config.GetSection("Smtp");
        var smtpHost = smtpSection["Host"];
        var smtpPort = int.Parse(smtpSection["Port"] ?? "587");
        var smtpEnableSsl = bool.Parse(smtpSection["EnableSsl"] ?? "true");
        var smtpUser = smtpSection["User"];
        var smtpPass = smtpSection["Password"];
        var smtpSenderName = smtpSection["SenderName"];

        var auth = app.MapGroup("auth").WithTags("Auth");

        auth.MapPost("login", async (IAuthService authService, LoginRequest loginRequest, [FromServices] ILogger<object> logger) =>
        {
            var result = await authService.Login(loginRequest);
            return Results.Ok(result);
        });

        auth.MapPost("register", async (IAuthService authService, RegisterRequest registerRequest, [FromServices] ILogger<object> logger) =>
        {
            await authService.Register(registerRequest);
            return Results.Ok();
        }).AddEndpointFilter<ValidatorFilter<RegisterRequest>>();

        auth.MapPost("refresh-token", async (IAuthService authService, RefreshTokenRequest refreshTokenRequest, [FromServices] ILogger<object> logger) =>
        {
            var result = await authService.RefreshToken(refreshTokenRequest);
            return Results.Ok(result);
        }).AddEndpointFilter<ValidatorFilter<RefreshTokenRequest>>();

        auth.MapPost("{userId}/make-seller", async (int userId, IAuthService authService, [FromServices] ILogger<object> logger) =>
        {
            await authService.MakeUserSellerAsync(userId);
            return Results.Ok("User role updated to Seller");
        }).RequireAuthorization("admin");

        auth.MapPost("{userId}/make-normal", async (int userId, IAuthService authService, [FromServices] ILogger<object> logger) =>
        {
            await authService.MakeUserNormalAsync(userId);
            return Results.Ok("User role updated to User");
        }).RequireAuthorization("admin");

        auth.MapPut("{userId}/update", async (int userId, IAuthService authService, UpdateUserRequest updateUserRequest, [FromServices] ILogger<object> logger) =>
        {
            await authService.UpdateUserAsync(userId, updateUserRequest);
            return Results.Ok("User information updated successfully");
        }).RequireAuthorization();

        auth.MapGet("userlist", async (IAuthService authService, [FromServices] ILogger<object> logger) =>
        {
            var users = await authService.GetUserListAsync();
            return Results.Ok(users);
        }).RequireAuthorization("admin");

        auth.MapGet("{userId}", async (int userId, IAuthService authService, [FromServices] ILogger<object> logger) =>
        {
            var user = await authService.GetUserByIdAsync(userId);
            return Results.Ok(user);
        });

        auth.MapDelete("/{id}", async (int id, IAuthService authService, [FromServices] ILogger<object> logger) =>
        {
            var existingUser = await authService.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                logger.LogWarning("User not found for deletion. UserId: {UserId}", id);
                return Results.NotFound();
            }

            await authService.DeleteUserAsync(id);
            return Results.NoContent();
        });

        auth.MapPost("send-mail", async (IAuthService authService, MailRequest request, [FromServices] ILogger<object> logger) =>
        {
            var users = await authService.GetUserListAsync();
            bool exists = users.Any(u => u.Email == request.To);
            if (!exists)
            {
                logger.LogWarning("Email not found in system: {Email}", request.To);
                return Results.BadRequest("Email not found in system");
            }
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                string[] lines = request.Body.Split(BrSeparator, StringSplitOptions.None);

                string formattedBody = "<html><body>";
                foreach (var line in lines)
                {
                    formattedBody += $"<p>{line}</p>";
                }
                formattedBody += "</body></html>";

                if (string.IsNullOrEmpty(smtpUser))
                {
                    logger.LogError("SMTP user is null or empty.");
                    return Results.Problem("SMTP user is not configured.");
                }
                var mail = new MailMessage
                {
                    From = new MailAddress(smtpUser, smtpSenderName),
                    Subject = request.Subject,
                    Body = formattedBody,
                    IsBodyHtml = true
                };
                mail.To.Add(request.To);
                var smtp = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = smtpEnableSsl
                };
                smtp.Send(mail);

                return Results.Ok("Mail sent successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while sending the mail to: {Email}", request.To);
                return Results.Problem("An error occurred while sending the mail: " + ex.Message);
            }
        });

        auth.MapPost("reset-password", async (IAuthService authService, ResetPasswordRequest request, [FromServices] ILogger<object> logger) =>
        {
            var result = await authService.ResetPasswordAsync(request);
            if (!result)
            {
                logger.LogWarning("Password reset failed. User not found: {Email}", request.Email);
                return Results.BadRequest("User not found");
            }
            return Results.Ok("Password reset successful");
        });
    }
}