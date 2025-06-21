using Application.Dtos;
using Application.Interfaces.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using Domain.Entitties;

namespace Infrastructure.ExternalServices
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly string Email;
        private readonly string Password;
        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            Email = _config["SMTP:email"]!;
            Password = _config["SMTP:password"]!;
            _logger = logger;
        }
        public async Task<bool> SendBulkEmailAsync(ICollection<User> to, string subject, AssessmentDto assessment)
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
                        Text = Template(item, assessment)
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

        public Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SendResultEmailAsync( Submission submission, User user)
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
                        Text = ResultTemplate(user, submission)
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

        public string Template(User user, AssessmentDto assessment)
        {
            string template = $@"
                <html>
                  <body style='font-family: Arial, sans-serif; color: #333;'>
                    <h2>You're Invited to an Assessment</h2>
                    <p>Hi <strong>{user.FullName}</strong>,</p>
                    <p>
                      You have been invited to participate in the assessment <strong>{assessment.Title}</strong> on <strong>{assessment.TechnologyStack}</strong>.
                    </p>
                    <h4>Assessment Details:</h4>
                    <ul>
                      <li><strong>Start Date:</strong> {assessment.StartDate}</li>
                      <li><strong>End Date:</strong> {assessment.EndDate}</li>
                      <li><strong>Duration:</strong> {assessment.DurationInMinutes} minutes</li>
                    </ul>
                    <p>
                      Please click the link below to access the assessment:
                    </p>
                    <p>
                      <a href='assessmentLink' style='background-color: #4A90E2; color: #fff; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Start Assessment</a>
                    </p>
                    <br/>
                    <p>If you have any questions, feel free to reply to this email.</p>
                    <p>Good luck!</p>
                    <p>The Assessment Team</p>
                    <hr style='margin-top:30px;' />
                    <small style='color: gray;'>This is an automated message. Please do not reply directly to this email.</small>
                  </body>
                </html>";
            return template;
        }
        public string ResultTemplate(User user , Submission submission)
        {
            string template = $@"
                <html>
                  <body style='font-family: Arial, sans-serif; color: #333;'>
                    <h2>Assessment Result</h2>
                    <p>Hi <strong>{user.FullName}</strong>,</p>
                    <p>
                      Your assessment submission has been evaluated. Here are your results:
                    </p>
                      <p>
                          Thank you for completing the assessment <strong>{submission.Assessment.Title}</strong> on <strong>{submission.Assessment.TechnologyStack}</strong>.
                       </p>
                    <h4>Submission Details:</h4>
                    <ul>
                      <li><strong>Assessment ID:</strong> {submission.AssessmentId}</li>
                      <li><strong>Submitted At:</strong> {submission.SubmittedAt}</li>
                      <li><strong>Total Score:</strong> {submission.TotalScore}</li>
                      <li><strong>Feedback:</strong> {submission.FeedBack}</li>
                    </ul>
                    <p>If you have any questions or need further clarification, feel free to reach out.</p>
                    <p>Best regards,</p>
                    <p>The Assessment Team</p>
                    <hr style='margin-top:30px;' />
                    <small style='color: gray;'>This is an automated message. Please do not reply directly to this email.</small>
                  </body>
                </html>";
            return template;
        }
    }
}
