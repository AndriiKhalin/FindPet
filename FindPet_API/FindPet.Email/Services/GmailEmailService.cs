using System.Net;
using System.Net.Mail;
using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Email.Configuration;
using FindPet.Email.Interfaces;
using FindPet.Email.Models;
using FindPet.Email.Templates;

namespace FindPet.Email.Services;

public class GmailEmailService(EmailSettings emailSettings, ILoggerManager logger) : IEmailService
{
    public async Task SendEmailConfirmationAsync(string email, string userId, string userName, string confirmationToken)
    {
        try
        {
            var fullUrl = $"{emailSettings.BaseUrl}/auth/confirm-email?userId={userId}&token={confirmationToken}";
            var htmlContent = EmailTemplateBuilder.BuildEmailConfirmationTemplate(userName, fullUrl);
            var textContent = EmailTemplateBuilder.ToPlainText(htmlContent);

            await SendEmailAsync(new EmailMessage
            {
                ToEmail = email,
                ToName = userName,
                Subject = "Confirm Your Email - FindPet 🐾",
                HtmlContent = htmlContent,
                TextContent = textContent
            });

            logger.LogInfo($"Email confirmation sent successfully to {email}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to send email confirmation to {email}. Error: {ex.Message}");
            throw;
        }
    }

    public async Task SendPasswordResetEmailAsync(string email, string userName, string resetToken)
    {
        try
        {
            var fullUrl = $"{emailSettings.BaseUrl}/auth/reset-password?email={email}&token={resetToken}";
            var htmlContent = EmailTemplateBuilder.BuildPasswordResetTemplate(userName, fullUrl);
            var textContent = EmailTemplateBuilder.ToPlainText(htmlContent);

            await SendEmailAsync(new EmailMessage
            {
                ToEmail = email,
                ToName = userName,
                Subject = "Reset Your Password - FindPet 🔒",
                HtmlContent = htmlContent,
                TextContent = textContent
            });

            logger.LogInfo($"Password reset email sent successfully to {email}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to send password reset email to {email}. Error: {ex.Message}");
            throw;
        }
    }

    public async Task SendWelcomeEmailAsync(string email, string userName)
    {
        try
        {
            var dashboardUrl = $"{emailSettings.BaseUrl}/dashboard";
            var htmlContent = EmailTemplateBuilder.BuildWelcomeTemplate(userName, dashboardUrl);
            var textContent = EmailTemplateBuilder.ToPlainText(htmlContent);

            await SendEmailAsync(new EmailMessage
            {
                ToEmail = email,
                ToName = userName,
                Subject = "Welcome to FindPet! 🎉",
                HtmlContent = htmlContent,
                TextContent = textContent
            });

            logger.LogInfo($"Welcome email sent successfully to {email}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to send welcome email to {email}. Error: {ex.Message}");
            throw;
        }
    }

    public async Task SendPasswordChangedNotificationAsync(string email, string userName)
    {
        try
        {
            var htmlContent = EmailTemplateBuilder.BuildPasswordChangedTemplate(userName);
            var textContent = EmailTemplateBuilder.ToPlainText(htmlContent);

            await SendEmailAsync(new EmailMessage
            {
                ToEmail = email,
                ToName = userName,
                Subject = "Password Changed Successfully - FindPet ✅",
                HtmlContent = htmlContent,
                TextContent = textContent
            });

            logger.LogInfo($"Password changed notification sent successfully to {email}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to send password changed notification to {email}. Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    ///     Core method to send email via Gmail SMTP using System.Net.Mail
    /// </summary>
    private async Task SendEmailAsync(EmailMessage emailMessage)
    {
        if (!emailSettings.EnableEmailSending)
        {
            logger.LogWarn($"Email sending is disabled. Email to {emailMessage.ToEmail} was not sent.");
            return;
        }

        // Validate settings
        if (string.IsNullOrEmpty(emailSettings.SmtpHost) ||
            string.IsNullOrEmpty(emailSettings.SmtpUsername) ||
            string.IsNullOrEmpty(emailSettings.SmtpPassword))
        {
            logger.LogError("Gmail SMTP is not configured properly. Missing host, username, or password.");
            throw new InvalidOperationException("Email service is not properly configured.");
        }

        try
        {
            using var smtpClient = new SmtpClient(emailSettings.SmtpHost, emailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(emailSettings.SmtpUsername, emailSettings.SmtpPassword),
                EnableSsl = emailSettings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000 // 30 seconds timeout
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(emailSettings.FromEmail, emailSettings.FromName),
                Subject = emailMessage.Subject,
                Body = emailMessage.HtmlContent,
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };

            mailMessage.To.Add(new MailAddress(emailMessage.ToEmail, emailMessage.ToName));

            // Add alternative plain text view
            if (!string.IsNullOrEmpty(emailMessage.TextContent))
            {
                var plainView = AlternateView.CreateAlternateViewFromString(
                    emailMessage.TextContent,
                    null,
                    "text/plain"
                );
                mailMessage.AlternateViews.Add(plainView);
            }

            await smtpClient.SendMailAsync(mailMessage);

            logger.LogInfo($"Email sent successfully via Gmail SMTP to {emailMessage.ToEmail}");
        }
        catch (SmtpException ex)
        {
            logger.LogError($"SMTP error while sending email to {emailMessage.ToEmail}. " +
                            $"StatusCode: {ex.StatusCode}, Message: {ex.Message}");
            throw new Exception($"Failed to send email via Gmail SMTP: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            logger.LogError($"Unexpected error while sending email to {emailMessage.ToEmail}. Error: {ex.Message}");
            throw;
        }
    }
}