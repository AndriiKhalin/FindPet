namespace FindPet.Email.Configuration;

public class EmailSettings
{
    /// <summary>
    ///     SMTP Host (e.g., smtp.gmail.com)
    /// </summary>
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>
    ///     SMTP Port (587 for TLS, 465 for SSL)
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    ///     SMTP Username (your Gmail address)
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    ///     SMTP Password (App Password from Google)
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    ///     From email address (must be verified)
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    ///     From name (sender display name)
    /// </summary>
    public string FromName { get; set; } = string.Empty;

    /// <summary>
    ///     Base URL for email links
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    ///     Enable/disable email sending (for development/testing)
    /// </summary>
    public bool EnableEmailSending { get; set; } = true;

    /// <summary>
    ///     Enable SSL (use true for port 465, false for 587 with STARTTLS)
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    ///     API Key
    /// </summary>
    // Legacy Brevo settings (can be removed after migration)
    [Obsolete("Use SMTP settings instead")]
    public string ApiKey { get; set; } = string.Empty;
}