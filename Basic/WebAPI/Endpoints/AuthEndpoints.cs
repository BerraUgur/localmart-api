using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using WebAPI.Data;
using WebAPI.Filters;
using WebAPI.Models;
using WebAPI.ModelViews;
using WebAPI.Services.Abstract;

namespace WebAPI.Endpoints;

public record MailRequest(string To, string Subject, string Body);
public record ForgotPasswordRequest(string Email);

public static class AuthEndpoints
{
    private const string EmailSeparator = "</br>";
    
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
        var frontendUrl = config["FrontendUrl"];

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
                var emailBody = FormatEmailBody(request.Body);

                if (string.IsNullOrEmpty(smtpUser))
                {
                    logger.LogError("SMTP user is null or empty.");
                    return Results.Problem("SMTP user is not configured.");
                }
                
                await SendEmailAsync(smtpHost, smtpPort, smtpUser, smtpPass, smtpSenderName, smtpEnableSsl, request.To, request.Subject, emailBody);

                return Results.Ok("Mail sent successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while sending the mail to: {Email}", request.To);
                return Results.Problem("An error occurred while sending the mail.");
            }
        });

        auth.MapPost("reset-password", async (IAuthService authService, ResetPasswordRequest request, ApplicationDBContext db, [FromServices] ILogger<object> logger) =>
        {
            var tokenEntry = await db.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == request.Token);
            if (tokenEntry == null || tokenEntry.IsUsed || tokenEntry.ExpirationDate < DateTime.UtcNow)
            {
                logger.LogWarning("Password reset failed. Token invalid or expired: {Token}", request.Token);
                return Results.BadRequest("The link is expired or invalid.");
            }
            
            var result = await authService.ResetPasswordAsync(request);
            if (!result)
            {
                logger.LogWarning("Password reset failed. User not found: {Email}", request.Email);
                return Results.BadRequest("User not found");
            }
            
            tokenEntry.IsUsed = true;
            await db.SaveChangesAsync();
            return Results.Ok("Password reset successful");
        });
        auth.MapPost("forgot-password", async (IAuthService authService, ForgotPasswordRequest request, ApplicationDBContext db, [FromServices] ILogger<object> logger) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                logger.LogWarning("Forgot password: Email not found {Email}", request.Email);
                return Results.BadRequest("Email not found");
            }

            var token = Guid.NewGuid().ToString();
            var expiration = DateTime.UtcNow.AddMinutes(10);
            var resetToken = new PasswordResetToken
            {
                Token = token,
                Email = request.Email,
                ExpirationDate = expiration,
                IsUsed = false
            };
            db.PasswordResetTokens.Add(resetToken);
            await db.SaveChangesAsync();

            var resetUrl = $"{frontendUrl}/reset-password?token={token}&email={request.Email}";
            var mailBody = $"You can use the link below to reset your password (valid for 10 minutes): <a href='{resetUrl}'>{resetUrl}</a>";
            
            try
            {
                await SendEmailAsync(smtpHost, smtpPort, smtpUser, smtpPass, smtpSenderName, smtpEnableSsl, request.Email, "Password Reset", mailBody);
                return Results.Ok("Password reset email sent successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Password reset email could not be sent: {Email}", request.Email);
                return Results.Problem("Email could not be sent.");
            }
        });
    }

    private static string FormatEmailBody(string body)
    {
        string[] lines = body.Split(EmailSeparator, StringSplitOptions.None);
        var formattedBody = "<html><body>";
        foreach (var line in lines)
        {
            formattedBody += $"<p>{line}</p>";
        }
        formattedBody += "</body></html>";
        return formattedBody;
    }

    private static async Task SendEmailAsync(string? host, int port, string? user, string? password, string? senderName, bool enableSsl, string to, string subject, string body)
    {
        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException("SMTP configuration is incomplete.");
        }

        var mail = new MailMessage
        {
            From = new MailAddress(user, senderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mail.To.Add(to);

        using var smtp = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, password),
            EnableSsl = enableSsl
        };

        await smtp.SendMailAsync(mail);
    }
}