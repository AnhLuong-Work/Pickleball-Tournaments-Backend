namespace AppPickleball.Application.Common.Services;

// Interface cho service gửi email
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
