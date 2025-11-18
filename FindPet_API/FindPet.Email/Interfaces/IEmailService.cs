namespace FindPet.Email.Interfaces;

/// <summary>
///     Service for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    ///     Send email confirmation link
    /// </summary>
    Task SendEmailConfirmationAsync(string email, string userId, string userName, string confirmationToken);

    /// <summary>
    ///     Send password reset link
    /// </summary>
    Task SendPasswordResetEmailAsync(string email, string userName, string resetToken);

    /// <summary>
    ///     Send welcome email after registration
    /// </summary>
    Task SendWelcomeEmailAsync(string email, string userName);

    /// <summary>
    ///     Send password changed notification
    /// </summary>
    Task SendPasswordChangedNotificationAsync(string email, string userName);
}