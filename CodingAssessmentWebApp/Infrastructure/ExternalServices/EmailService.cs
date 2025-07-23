using Application.Dtos;
using Application.Interfaces.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using Domain.Entitties;
using Application.Exceptions;
using Application.Interfaces.Services;

namespace Infrastructure.ExternalServices
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly string Email;
        private readonly string Password;
        private readonly ITemplateService templateService;
        public EmailService(IConfiguration config, ILogger<EmailService> logger, ITemplateService tempService)
        {
            _config = config;
            Email = _config["SMTP:email"]!;
            Password = _config["SMTP:password"]!;
            _logger = logger;
            templateService = tempService;
        }
        public async Task<bool> SendAssessmentEmail(ICollection<UserDto> to, string subject, AssessmentDto assessment)
        {
            if (to == null || to.Any(x => string.IsNullOrWhiteSpace(x.Email)))
            {
                _logger.LogError("One or more recipient emails are missing.");
                return false;
            }
            

            try
            {
                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                await smtpClient.AuthenticateAsync(Email, Password);
                foreach (var item in to)
                {
                    var emailMessage = new MimeMessage();
                    emailMessage.From.Add(new MailboxAddress("CLH", Email));
                    emailMessage.To.Add(new MailboxAddress(item.FullName, item.Email));
                    emailMessage.Subject = subject;
                    emailMessage.Body = new TextPart("html")
                    {
                        Text = templateService.NewAssessmentTemplate(item, assessment)
                    };
                    _logger.LogInformation($"Sending email to {item.Email}...");
                    await smtpClient.SendAsync(emailMessage);
                }
                _logger.LogInformation($"Bulk email sent to {to.Count} recipients.");
                _logger.LogInformation("Bulk email sent successfully.");

                await smtpClient.DisconnectAsync(true); // Fixed: Changed from `smtpClient.Disconnect` to `smtpClient.DisconnectAsync`
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send bulk email: {ex.Message}", ex);
                return false;
            }

            return true;
        }

        public async Task<bool> SendBulkEmailAsync(ICollection<UserDto> to, string subject, string template)
        {
            if (to == null || to.Any(x => string.IsNullOrWhiteSpace(x.Email)))
            {
                _logger.LogError("One or more recipient emails are missing.");
                return false;
            }


            try
            {
                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                await smtpClient.AuthenticateAsync(Email, Password);
                foreach (var item in to)
                {
                    var emailMessage = new MimeMessage();
                    emailMessage.From.Add(new MailboxAddress("CLH", Email));
                    emailMessage.To.Add(new MailboxAddress(item.FullName, item.Email));
                    emailMessage.Subject = subject;
                    emailMessage.Body = new TextPart("html")
                    {
                        Text = template
                    };
                    _logger.LogInformation($"Sending email to {item.Email}...");
                    await smtpClient.SendAsync(emailMessage);
                }
                _logger.LogInformation($"Bulk email sent to {to.Count} recipients.");
                _logger.LogInformation("Bulk email sent successfully.");

                await smtpClient.DisconnectAsync(true); // Fixed: Changed from `smtpClient.Disconnect` to `smtpClient.DisconnectAsync`
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send bulk email: {ex.Message}", ex);
                return false;
            }

            return true;
        }

        public async Task SendEmailAsync(UserDto to, string subject, string body)
        {
            try
            {
                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                await smtpClient.AuthenticateAsync(Email, Password);

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("CLH", Email));
                emailMessage.To.Add(new MailboxAddress(to.FullName, to.Email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart("html")
                {
                    Text = body
                };
                _logger.LogInformation($"Sending email to {to.Email}...");
                await smtpClient.SendAsync(emailMessage);

                _logger.LogInformation("Bulk email sent successfully.");

                await smtpClient.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send bulk email: {ex.Message}", ex);
                throw new ApiException("Error", 500, "EMAIL_SEND_FAILURE", ex);
            }
        }

        public async Task<bool> SendResultEmailAsync( Submission submission, UserDto user)
        {
            if (user == null)
            {
                _logger.LogError("Recipient Email is empty or User is null");
                return false;
            }


            try
            {
                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                await smtpClient.AuthenticateAsync(Email, Password);

                    var emailMessage = new MimeMessage();
                    emailMessage.From.Add(new MailboxAddress("CLH", Email));
                    emailMessage.To.Add(new MailboxAddress(user.FullName, user.Email));
                    emailMessage.Subject = "Assessment Result";
                    emailMessage.Body = new TextPart("html")
                    {
                        Text = templateService.ResultTemplate(user, submission)
                    };
                    _logger.LogInformation($"Sending email to {user.Email}...");
                    await smtpClient.SendAsync(emailMessage);
                
                _logger.LogInformation("Result email sent successfully.");

                await smtpClient.DisconnectAsync(true); // Fixed: Changed from `smtpClient.Disconnect` to `smtpClient.DisconnectAsync`
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send bulk email: {ex.Message}", ex);
                return false;
            }

            return true;
        }

       
    }
}
