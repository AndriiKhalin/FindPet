using System.Text.RegularExpressions;

namespace FindPet.Email.Templates;

/// <summary>
///     Builds professional HTML email templates for FindPet
/// </summary>
public static class EmailTemplateBuilder
{
    private const string BaseTemplate = @"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{0}</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
            line-height: 1.6;
            color: #333333;
            background-color: #f4f6f9;
        }}
        .email-wrapper {{
            width: 100%;
            background-color: #f4f6f9;
            padding: 20px 0;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }}
        .email-header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 40px 30px;
            text-align: center;
        }}
        .email-header h1 {{
            color: #ffffff;
            font-size: 32px;
            font-weight: 700;
            margin-bottom: 10px;
        }}
        .email-header .tagline {{
            color: rgba(255, 255, 255, 0.9);
            font-size: 16px;
        }}
        .email-body {{
            padding: 40px 30px;
        }}
        .email-body h2 {{
            color: #2d3748;
            font-size: 24px;
            margin-bottom: 20px;
        }}
        .email-body p {{
            color: #4a5568;
            margin-bottom: 15px;
            font-size: 16px;
        }}
        .button {{
            display: inline-block;
            padding: 14px 32px;
            margin: 25px 0;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: #ffffff !important;
            text-decoration: none;
            border-radius: 8px;
            font-weight: 600;
            font-size: 16px;
            text-align: center;
        }}
        .alert-box {{
            background-color: #fff3cd;
            border-left: 4px solid #ffc107;
            padding: 16px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        .alert-box p {{
            margin: 0;
            color: #856404;
        }}
        .info-box {{
            background-color: #e7f3ff;
            border-left: 4px solid #2196F3;
            padding: 16px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        .feature-list {{
            list-style: none;
            padding: 0;
            margin: 20px 0;
        }}
        .feature-list li {{
            padding: 12px 0;
            border-bottom: 1px solid #e2e8f0;
            color: #4a5568;
        }}
        .feature-list li:last-child {{
            border-bottom: none;
        }}
        .link-box {{
            background-color: #f7fafc;
            padding: 16px;
            border-radius: 8px;
            word-break: break-all;
            margin: 20px 0;
        }}
        .link-box a {{
            color: #667eea;
            text-decoration: none;
        }}
        .email-footer {{
            background-color: #f7fafc;
            padding: 30px;
            text-align: center;
            border-top: 1px solid #e2e8f0;
        }}
        .email-footer p {{
            color: #718096;
            font-size: 14px;
            margin: 5px 0;
        }}
    </style>
</head>
<body>
    <div class='email-wrapper'>
        <div class='email-container'>
            <div class='email-header'>
                <h1>🐾 FindPet</h1>
                <p class='tagline'>Reuniting Pets with Their Families</p>
            </div>
            <div class='email-body'>
                {1}
            </div>
            <div class='email-footer'>
                <p><strong>FindPet</strong> - Helping Lost Pets Find Their Way Home</p>
                <p>This is an automated email. Please do not reply to this message.</p>
                <p style='margin-top: 20px; font-size: 12px;'>
                    &copy; {2} FindPet. All rights reserved.
                </p>
            </div>
        </div>
    </div>
</body>
</html>";

    public static string BuildEmailConfirmationTemplate(string userName, string confirmationUrl)
    {
        var content = $@"
            <h2>Welcome to FindPet, {userName}! 👋</h2>
            <p>Thank you for joining our community of pet lovers. We're excited to help you reunite lost pets with their families!</p>
            <p>To activate your account and start using FindPet, please confirm your email address:</p>
            <div style='text-align: center;'>
                <a href='{confirmationUrl}' class='button'>Confirm Email Address</a>
            </div>
            <div class='alert-box'>
                <p><strong>⏰ Important:</strong> This confirmation link will expire in 24 hours for security reasons.</p>
            </div>
            <p>If you didn't create an account with FindPet, you can safely ignore this email.</p>
            <div class='link-box'>
                <p><strong>Can't click the button?</strong> Copy and paste this link into your browser:</p>
                <p><a href='{confirmationUrl}'>{confirmationUrl}</a></p>
            </div>
            <p style='margin-top: 30px;'>Best regards,<br><strong>The FindPet Team</strong></p>";

        return string.Format(BaseTemplate, "Confirm Your Email - FindPet", content, DateTime.UtcNow.Year);
    }

    public static string BuildPasswordResetTemplate(string userName, string resetUrl)
    {
        var content = $@"
            <h2>Password Reset Request 🔒</h2>
            <p>Hi {userName},</p>
            <p>We received a request to reset your password for your FindPet account. If you made this request, click the button below to create a new password:</p>
            <div style='text-align: center;'>
                <a href='{resetUrl}' class='button'>Reset Password</a>
            </div>
            <div class='alert-box'>
                <p><strong>⏰ Time Sensitive:</strong> This link will expire in 1 hour for your security.</p>
            </div>
            <div class='info-box'>
                <p><strong>🛡️ Didn't request this?</strong></p>
                <p>If you didn't request a password reset, please ignore this email. Your password will remain unchanged.</p>
            </div>
            <div class='link-box'>
                <p><strong>Button not working?</strong> Copy and paste this link:</p>
                <p><a href='{resetUrl}'>{resetUrl}</a></p>
            </div>
            <p style='margin-top: 30px;'>Best regards,<br><strong>The FindPet Team</strong></p>";

        return string.Format(BaseTemplate, "Reset Your Password - FindPet", content, DateTime.UtcNow.Year);
    }

    public static string BuildWelcomeTemplate(string userName, string dashboardUrl)
    {
        var content = $@"
            <h2>Welcome to FindPet! 🎉</h2>
            <p>Hi {userName},</p>
            <p>Your email has been confirmed successfully! You're now part of the FindPet community.</p>
            <h3 style='color: #667eea; margin-top: 30px;'>What You Can Do Now:</h3>
            <ul class='feature-list'>
                <li><strong>📝 Report Lost Pets:</strong> Create detailed ads for pets that are missing</li>
                <li><strong>🔍 Search for Pets:</strong> Browse pets found by other users in your area</li>
                <li><strong>🤖 AI Breed Detection:</strong> Use our ML-powered automatic breed recognition</li>
                <li><strong>📸 Upload Photos:</strong> Add photos to help identify pets</li>
                <li><strong>💬 Connect:</strong> Communicate with pet finders and owners</li>
                <li><strong>🗺️ Location Search:</strong> Find pets by location</li>
            </ul>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{dashboardUrl}' class='button'>Go to Dashboard</a>
            </div>
            <div class='info-box'>
                <p><strong>💡 Pro Tip:</strong> Add as much detail as possible to your pet listings. The more information you provide, the higher the chances of a successful reunion!</p>
            </div>
            <p style='margin-top: 30px;'>Happy searching!<br><strong>The FindPet Team</strong></p>";

        return string.Format(BaseTemplate, "Welcome to FindPet!", content, DateTime.UtcNow.Year);
    }

    public static string BuildPasswordChangedTemplate(string userName)
    {
        var content = $@"
            <h2>Password Changed Successfully ✅</h2>
            <p>Hi {userName},</p>
            <p>This email confirms that your password has been changed successfully.</p>
            <div class='info-box'>
                <p><strong>🔒 Security Details:</strong></p>
                <ul style='margin: 10px 0; padding-left: 20px;'>
                    <li>Changed on: {DateTime.UtcNow:dddd, MMMM dd, yyyy} at {DateTime.UtcNow:HH:mm} UTC</li>
                    <li>All active sessions have been logged out</li>
                    <li>You'll need to log in again with your new password</li>
                </ul>
            </div>
            <div class='alert-box'>
                <p><strong>⚠️ Didn't make this change?</strong></p>
                <p>If you didn't change your password, please contact our support team immediately. Your account security may be compromised.</p>
            </div>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='mailto:support@findpet.com' class='button'>Contact Support</a>
            </div>
            <p style='margin-top: 30px;'>Stay secure,<br><strong>The FindPet Team</strong></p>";

        return string.Format(BaseTemplate, "Password Changed - FindPet", content, DateTime.UtcNow.Year);
    }

    public static string ToPlainText(string htmlContent)
    {
        // Simple HTML to plain text conversion
        return Regex.Replace(htmlContent, "<.*?>", string.Empty)
            .Replace("&nbsp;", " ")
            .Replace("&quot;", "\"")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&amp;", "&")
            .Trim();
    }
}