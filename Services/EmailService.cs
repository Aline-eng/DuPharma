using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace DuPharma.Services;

public interface IEmailService
{
    Task SendContactReplyAsync(string toEmail, string customerName, string originalMessage, string reply);
    Task SendTestEmailAsync(string toEmail, string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendContactReplyAsync(string toEmail, string customerName, string originalMessage, string reply)
    {
        var subject = "Reply to Your Contact Message - DuPharma Pharmacy";
        
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: linear-gradient(135deg, #0D182B 0%, #1B2940 100%); color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;'>
                        <h1 style='margin: 0; font-size: 24px;'>DuPharma Pharmacy</h1>
                        <p style='margin: 5px 0 0 0; opacity: 0.9;'>Your Trusted Partner in Health</p>
                    </div>
                    
                    <div style='background: #f8f9fa; padding: 20px; border-left: 4px solid #3498db;'>
                        <h2 style='color: #0D182B; margin-top: 0;'>Hello {customerName},</h2>
                        <p>Thank you for contacting DuPharma Pharmacy. We have reviewed your message and are pleased to provide you with a response.</p>
                    </div>
                    
                    <div style='background: white; padding: 20px; border: 1px solid #dee2e6;'>
                        <h3 style='color: #0D182B; border-bottom: 2px solid #3498db; padding-bottom: 10px;'>Your Original Message:</h3>
                        <div style='background: #f1f3f4; padding: 15px; border-radius: 5px; margin: 10px 0;'>
                            <p style='margin: 0; font-style: italic;'>""{originalMessage}""</p>
                        </div>
                        
                        <h3 style='color: #0D182B; border-bottom: 2px solid #27ae60; padding-bottom: 10px; margin-top: 30px;'>Our Response:</h3>
                        <div style='background: #e8f5e8; padding: 15px; border-radius: 5px; margin: 10px 0;'>
                            <p style='margin: 0;'>{reply}</p>
                        </div>
                    </div>
                    
                    <div style='background: #0D182B; color: white; padding: 20px; text-align: center; border-radius: 0 0 8px 8px;'>
                        <p style='margin: 0 0 10px 0;'><strong>Need further assistance?</strong></p>
                        <p style='margin: 0; opacity: 0.9;'>
                            📧 Email: info@dupharma.com<br>
                            📞 Phone: +250 789 252 719<br>
                            🏥 Visit us: 123 Medical Street, Health City
                        </p>
                        <p style='margin: 15px 0 0 0; font-size: 12px; opacity: 0.7;'>
                            © 2024 DuPharma Pharmacy. All rights reserved.
                        </p>
                    </div>
                </div>
            </body>
            </html>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendTestEmailAsync(string toEmail, string subject, string body)
    {
        await SendEmailAsync(toEmail, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("DuPharma Pharmacy", _configuration["Email:FromEmail"] ?? "noreply@dupharma.com"));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            // For development, you can use a fake SMTP server or configure real SMTP
            var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"] ?? "";
            var smtpPass = _configuration["Email:SmtpPassword"] ?? "";

            if (string.IsNullOrEmpty(smtpUser))
            {
                _logger.LogWarning("Email configuration not found. Email not sent to {Email}", toEmail);
                return;
            }

            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }
}