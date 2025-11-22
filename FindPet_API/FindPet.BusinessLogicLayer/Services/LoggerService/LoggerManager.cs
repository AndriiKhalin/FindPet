using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web;
using FindPet.Domain.Interfaces.ILoggerService;
using NLog;

namespace FindPet.BusinessLogicLayer.Services.LoggerService;

public class LoggerManager : ILoggerManager
{
    private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

    // Regex patterns for detecting sensitive information
    private static readonly Regex EmailPattern =
        new(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled);

    private static readonly Regex PhonePattern = new(@"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b", RegexOptions.Compiled);

    private static readonly Regex GuidPattern =
        new(@"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b", RegexOptions.Compiled);

    private static readonly Regex IpPattern = new(@"\b(?:\d{1,3}\.){3}\d{1,3}\b", RegexOptions.Compiled);

    public void LogDebug(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var sanitizedMessage = SanitizeMessage(message, true);
        var className = GetSafeClassName();
        var sanitizedMemberName = SanitizeMemberName(memberName);

        logger.Debug(
            $"<span style='background:rgb(100,220,0)'> {DateTime.Now} | DEBUG | Project: {className} | Method: {sanitizedMemberName} | Message: {sanitizedMessage} | Line: {sourceLineNumber}</span>");
    }

    public void LogError(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var sanitizedMessage = SanitizeMessage(message, true);
        var className = GetSafeClassName();
        var sanitizedMemberName = SanitizeMemberName(memberName);

        logger.Error(
            $"<span style='background:rgb(160,0,0)'> {DateTime.Now} | ERROR | Project: {className} | Method: {sanitizedMemberName} | Message: {sanitizedMessage} | Line: {sourceLineNumber}</span>");
    }

    public void LogInfo(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var sanitizedMessage = SanitizeMessage(message, true);
        var className = GetSafeClassName();
        var sanitizedMemberName = SanitizeMemberName(memberName);

        logger.Info(
            $"<span style='background:rgb(0,160,0)'> {DateTime.Now} | INFO | Project: {className} | Method: {sanitizedMemberName} | Message: {sanitizedMessage} | Line: {sourceLineNumber}</span>");
    }

    public void LogWarn(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var sanitizedMessage = SanitizeMessage(message, true);
        var className = GetSafeClassName();
        var sanitizedMemberName = SanitizeMemberName(memberName);

        logger.Warn(
            $"<span style='background:rgb(200,200,0)'> {DateTime.Now} | WARN | Project: {className} | Method: {sanitizedMemberName} | Message: {sanitizedMessage} | Line: {sourceLineNumber}</span>");
    }

    /// <summary>
    ///     Sanitizes log messages to prevent log forging attacks (CWE-117) and exposure of sensitive data (CWE-359)
    /// </summary>
    /// <param name="message">The message to sanitize</param>
    /// <param name="redactSensitiveData">Whether to redact sensitive information like emails, GUIDs, etc.</param>
    /// <returns>Sanitized message safe for logging</returns>
    private static string SanitizeMessage(string message, bool redactSensitiveData = true)
    {
        if (string.IsNullOrEmpty(message))
            return message ?? string.Empty;

        var sanitized = message;

        // Remove control characters and newlines that could forge log entries
        sanitized = sanitized
            .Replace("\r", "")
            .Replace("\n", " ")
            .Replace("\t", " ")
            .Replace(Environment.NewLine, " ");

        // Redact sensitive information patterns
        if (redactSensitiveData) sanitized = RedactSensitiveData(sanitized);

        // HTML encode to prevent HTML injection in log viewers
        sanitized = HttpUtility.HtmlEncode(sanitized);

        // Limit length to prevent log flooding
        const int maxLength = 500;
        if (sanitized.Length > maxLength) sanitized = sanitized.Substring(0, maxLength) + "... [truncated]";

        return sanitized;
    }

    /// <summary>
    ///     Redacts sensitive information like emails, phone numbers, GUIDs, and IP addresses
    /// </summary>
    private static string RedactSensitiveData(string message)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        var redacted = message;

        // Redact email addresses
        redacted = EmailPattern.Replace(redacted, match =>
        {
            var email = match.Value;
            var atIndex = email.IndexOf('@');
            if (atIndex > 2) return email.Substring(0, 2) + "***@" + email.Substring(atIndex + 1);
            return "***@***";
        });

        // Redact phone numbers
        redacted = PhonePattern.Replace(redacted, "XXX-XXX-****");

        // Redact GUIDs (keep first 8 chars for debugging)
        redacted = GuidPattern.Replace(redacted,
            match => { return match.Value.Substring(0, 8) + "-****-****-****-************"; });

        // Redact IP addresses (keep first octet)
        redacted = IpPattern.Replace(redacted, match =>
        {
            var parts = match.Value.Split('.');
            return parts.Length == 4 ? $"{parts[0]}.***.***.***" : "***.***.***";
        });

        // Redact common sensitive keywords
        redacted = RedactKeywordPatterns(redacted);

        return redacted;
    }

    /// <summary>
    ///     Redacts values following sensitive keywords like "password", "token", etc.
    /// </summary>
    private static string RedactKeywordPatterns(string message)
    {
        var sensitiveKeywords = new[]
        {
            "password", "pwd", "passwd", "token", "secret", "key", "apikey",
            "api_key", "authorization", "auth", "credential", "ssn", "social",
            "connectionstring", "email", "phone", "creditcard", "bearer"
        };

        foreach (var keyword in sensitiveKeywords)
        {
            // Match patterns like "password: value" or "password=value"
            var pattern = new Regex(
                $@"({keyword})\s*[:=]\s*[^\s,;]+",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            message = pattern.Replace(message, "$1: [REDACTED]");
        }

        return message;
    }

    /// <summary>
    ///     Safely gets the caller class name without exposing full system paths
    /// </summary>
    private static string GetSafeClassName()
    {
        try
        {
            var frame = new StackTrace().GetFrame(2);
            var method = frame?.GetMethod();
            var declaringType = method?.DeclaringType;

            if (declaringType == null)
                return "Unknown";

            // Only return class name without namespace to avoid exposing internal structure
            var fullName = declaringType.Name;

            // Remove generic type parameters for cleaner logging
            var genericIndex = fullName.IndexOf('`');
            return genericIndex > 0 ? fullName.Substring(0, genericIndex) : fullName;
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    ///     Sanitizes method names to prevent exposure of sensitive operation details
    /// </summary>
    private static string SanitizeMemberName(string memberName)
    {
        if (string.IsNullOrEmpty(memberName))
            return "Unknown";

        // Remove async suffix for cleaner logs
        return memberName.Replace("Async", "").Replace("<>", "");
    }
}