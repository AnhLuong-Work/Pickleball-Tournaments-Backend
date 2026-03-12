using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Common.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AppPickleball.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = GetHtmlTemplate(subject, body)
                };
                message.Body = builder.ToMessageBody();

                // Logic cho Mock mode (nếu Host rỗng hoặc example)
                if (string.IsNullOrEmpty(_settings.Host) || _settings.Host == "smtp.example.com")
                {
                    _logger.LogInformation("================ EMAIL MOCK (MailKit) ================");
                    _logger.LogInformation($"To: {to}");
                    _logger.LogInformation($"Subject: {subject}");
                    _logger.LogInformation($"Body: {body}");
                    _logger.LogInformation("======================================================");
                    return;
                }

                using var client = new SmtpClient();
                // Demo/Dev: Bỏ qua kiểm tra chứng chỉ SSL. Prod: Nên bật kiểm tra chặt chẽ.
                client.CheckCertificateRevocation = false; 
                
                await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, cancellationToken);
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation($"Email sent to {to} successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {to}");
            }
        }

        private string GetHtmlTemplate(string title, string content)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 20px auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; padding-bottom: 20px; border-bottom: 1px solid #eeeeee; }}
                    .header h1 {{ color: #333333; margin: 0; }}
                    .content {{ padding: 20px 0; color: #555555; line-height: 1.6; }}
                    .otp-box {{ background-color: #f8f9fa; border: 1px dashed #007bff; color: #007bff; font-size: 24px; font-weight: bold; text-align: center; padding: 15px; margin: 20px 0; letter-spacing: 5px; border-radius: 5px; }}
                    .footer {{ text-align: center; padding-top: 20px; border-top: 1px solid #eeeeee; font-size: 12px; color: #999999; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Nextx Web App</h1>
                    </div>
                    <div class='content'>
                        <h2>{title}</h2>
                        {content}
                    </div>
                    <div class='footer'>
                        <p>&copy; {DateTime.Now.Year} Nextx. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
        }
    }
}
