using brevo_csharp.Api;
using brevo_csharp.Client;
using brevo_csharp.Model;
using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Email.Configuration;
using FindPet.Email.Interfaces;
using FindPet.Email.Models;
using FindPet.Email.Templates;
using Task = System.Threading.Tasks.Task;

namespace FindPet.Email.Services;

public class BrevoEmailService(EmailSettings emailSettings, ILoggerManager logger, TransactionalEmailsApi emailApi)
    : IEmailService
{
    public async Task SendEmailConfirmationAsync(string email, string userId, string userName, string confirmationToken)
    {
        try
        {
            var fullUrl = $"{emailSettings.BaseUrl}/auth/confirm-email?userId={userId}&token={confirmationToken}";

            var htmlContent = EmailTemplateBuilder.BuildEmailConfirmationTemplate(userName, fullUrl);

            await SendEmailAsync(new EmailMessage
            {
                ToEmail = email,
                ToName = userName,
                Subject = "Confirm Your Email - FindPet 🐾",
                HtmlContent = htmlContent
            });

            logger.LogInfo("Email confirmation sent successfully to {Email}", email);
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

            await SendEmailAsync(new EmailMessage
            {
                ToEmail = email,
                ToName = userName,
                Subject = "Reset Your Password - FindPet 🔒",
                HtmlContent = htmlContent
            });

            logger.LogInfo("Password reset email sent successfully to {Email}", email);
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

            await SendEmailAsync(new EmailMessage
            {
                ToEmail = email,
                ToName = userName,
                Subject = "Welcome to FindPet! 🎉",
                HtmlContent = htmlContent
            });

            logger.LogInfo("Welcome email sent successfully to {Email}", email);
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

            await SendEmailAsync(new EmailMessage
            {
                ToEmail = email,
                ToName = userName,
                Subject = "Password Changed Successfully - FindPet ✅",
                HtmlContent = htmlContent
            });

            logger.LogInfo("Password changed notification sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to send password changed notification to {email}. Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    ///     Core method to send email via Brevo API using official SDK
    /// </summary>
    private async Task SendEmailAsync(EmailMessage emailMessage)
    {
        if (!emailSettings.EnableEmailSending)
        {
            logger.LogWarn("Email sending is disabled. Email to {Email} was not sent.", emailMessage.ToEmail);
            return;
        }

        if (emailApi == null)
        {
            logger.LogError($"Brevo API is not configured. Cannot send email to {emailMessage.ToEmail}");
            throw new InvalidOperationException("Email service is not properly configured.");
        }

        try
        {
            // Create sender
            var sender = new SendSmtpEmailSender(
                emailSettings.FromName,
                emailSettings.FromEmail
            );

            // Create recipient
            var recipient = new SendSmtpEmailTo(
                emailMessage.ToEmail,
                emailMessage.ToName
            );

            // Create email payload
            var sendSmtpEmail = new SendSmtpEmail(
                sender,
                new List<SendSmtpEmailTo> { recipient },
                subject: emailMessage.Subject,
                htmlContent: emailMessage.HtmlContent
            );

            // Send email
            var result = await emailApi.SendTransacEmailAsync(sendSmtpEmail);

            logger.LogInfo(
                "Email sent successfully via Brevo. MessageId: {MessageId}, To: {Email}",
                result.MessageId,
                emailMessage.ToEmail
            );
        }
        catch (ApiException ex)
        {
            logger.LogError(
                $"Brevo API error while sending email to {emailMessage.ToEmail}. StatusCode: {ex.ErrorCode}, Response: {ex.Message}");
            throw new Exception($"Failed to send email via Brevo: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            logger.LogError($"Unexpected error while sending email to {emailMessage.ToEmail}. Error: {ex.Message}");
            throw;
        }
    }
}