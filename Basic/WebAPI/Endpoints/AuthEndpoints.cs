using System.Net.Mail;
using System.Net;
using WebAPI.Filters;
using WebAPI.ModelViews;
using WebAPI.Services.Abstract;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace WebAPI.Endpoints;

public record MailRequest(string To, string Subject, string Body);
public static class AuthEndpoints
{
    public static void RegisterAuthEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("auth")
            .WithTags("Auth");


        auth.MapPost("login",
            async (IAuthService authService, LoginRequest loginRequest) =>
            {
                var result = await authService.Login(loginRequest);
                return Results.Ok(result);
            });

        auth.MapPost("register",
            async (IAuthService authService, RegisterRequest registerRequest) =>
            {
                await authService.Register(registerRequest);
                return Results.Ok();
            }).AddEndpointFilter<ValidatorFilter<RegisterRequest>>();

        auth.MapPost("refresh-token",
            async (IAuthService authService, RefreshTokenRequest refreshTokenRequest) =>
            {
                var result = await authService.RefreshToken(refreshTokenRequest);
                return Results.Ok(result);
            }).AddEndpointFilter<ValidatorFilter<RefreshTokenRequest>>();

        auth.MapPost("{userId}/make-seller", async (int userId, IAuthService authService) =>
        {
            await authService.MakeUserSellerAsync(userId);
            return Results.Ok("User role updated to Seller");
        }).RequireAuthorization("admin");

        auth.MapPost("{userId}/make-normal", async (int userId, IAuthService authService) =>
        {
            await authService.MakeUserNormalAsync(userId);
            return Results.Ok("User role updated to User");
        }).RequireAuthorization("admin");

        auth.MapPut("{userId}/update", async (int userId, IAuthService authService, UpdateUserRequest updateUserRequest) =>
        {
            await authService.UpdateUserAsync(userId, updateUserRequest);
            return Results.Ok("User information updated successfully");
        }).RequireAuthorization();

        auth.MapGet("userlist", async (IAuthService authService) =>
        {
            var users = await authService.GetUserListAsync();
            return Results.Ok(users);
        }).RequireAuthorization("admin");

        auth.MapGet("{userId}", async (int userId, IAuthService authService) =>
        {
            var user = await authService.GetUserByIdAsync(userId);
            return Results.Ok(user);
        });

        auth.MapDelete("/{id}", async (int id, IAuthService authService) =>
        {
            var existingUser = await authService.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return Results.NotFound();
            }

            await authService.DeleteUserAsync(id);
            return Results.NoContent();
        });

        // 🔥 Basit Mail Gönderme Endpoint'i
        auth.MapPost("send-mail", async (IAuthService authService, MailRequest request) =>
        {
            // Email adresi sistemde kayıtlı mı kontrolü
            var users = await authService.GetUserListAsync();
            bool exists = users.Any(u => u.Email == request.To);
            if (!exists)
            {
                return Results.BadRequest("Email not found in system");
            }
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                // 1. Gelen veriyi satırlara ayır ve her bir satırı <p> ile sarmala
                string[] lines = request.Body.Split(new[] { "</br>" }, StringSplitOptions.None);

                string formattedBody = "<html><body>";  // HTML başlangıcı

                foreach (var line in lines)
                {
                    formattedBody += $"<p>{line}</p>";  // Her bir satırı <p> ile sarıyoruz
                }

                formattedBody += "</body></html>";  // HTML sonu

                var mail = new MailMessage();
                mail.From = new MailAddress("berra12320@gmail.com", "Localmart");
                mail.To.Add(request.To);
                mail.Subject = request.Subject;
                mail.Body = formattedBody;
                mail.IsBodyHtml = true;

                var smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential("berra12320@gmail.com", "zuxz kdhr wscv tonu"); // uygulama şifresi
                smtp.EnableSsl = true;
                smtp.Send(mail);

                return Results.Ok("Mail gönderildi");
            }
            catch (Exception ex)
            {
                return Results.Problem("Mail gönderilirken hata oluştu: " + ex.Message);
            }
        });
        auth.MapPost("reset-password", async (IAuthService authService, ResetPasswordRequest request) =>
        {
            var result = await authService.ResetPasswordAsync(request);
            if (!result)
            {
                return Results.BadRequest("User not found");
            }
            return Results.Ok("Password reset successful");
        });
    }
}