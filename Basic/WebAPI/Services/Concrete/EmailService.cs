using SendGrid;
using SendGrid.Helpers.Mail;
using WebAPI.Services.Abstract;

namespace WebAPI.Services.Concrete;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var apiKey = _configuration["SendGrid:ApiKey"];
        var senderEmail = _configuration["SendGrid:SenderEmail"];
        var senderName = _configuration["SendGrid:SenderName"];

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogError("SendGrid API key is not configured");
            throw new InvalidOperationException("SendGrid API key is not configured");
        }

        if (string.IsNullOrEmpty(senderEmail))
        {
            _logger.LogError("SendGrid sender email is not configured");
            throw new InvalidOperationException("SendGrid sender email is not configured");
        }

        try
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(senderEmail, senderName ?? "Localmart");
            var toAddress = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, null, body);

            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode != System.Net.HttpStatusCode.OK && 
                response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("SendGrid email failed. StatusCode: {StatusCode}, Body: {Body}", 
                    response.StatusCode, responseBody);
                throw new Exception($"Email sending failed with status code: {response.StatusCode}");
            }

            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", to);
            throw;
        }
    }
}
