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
        var logEntry = CreateSafeLogEntry("DEBUG", message, memberName, sourceLineNumber, "rgb(100,220,0)");
        logger.Debug(logEntry);
    }

    public void LogError(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var logEntry = CreateSafeLogEntry("ERROR", message, memberName, sourceLineNumber, "rgb(160,0,0)");
        logger.Error(logEntry);
    }

    public void LogInfo(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var logEntry = CreateSafeLogEntry("INFO", message, memberName, sourceLineNumber, "rgb(0,160,0)");
        logger.Info(logEntry);
    }

    public void LogWarn(string message, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var logEntry = CreateSafeLogEntry("WARN", message, memberName, sourceLineNumber, "rgb(200,200,0)");
        logger.Warn(logEntry);
    }

    /// <summary>
    ///     Creates a safe log entry with complete sanitization and encoding
    /// </summary>
    private static string CreateSafeLogEntry(string level, string message, string memberName, int lineNumber, string backgroundColor)
    {
        // Sanitize all components separately
        var sanitizedMessage = SanitizeMessage(message, true);
        var className = GetSafeClassName();
        var sanitizedMemberName = SanitizeMemberName(memberName);

        // Use structured logging approach - build safe log entry
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // All values are now sanitized, HTML encoded, and safe for logging
        return $"<span style='background:{backgroundColor}'> {timestamp} | {level} | Project: {className} | Method: {sanitizedMemberName} | Message: {sanitizedMessage} | Line: {lineNumber}</span>";
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
            return string.Empty;

        var sanitized = message;

        // CRITICAL: Remove ALL control characters and newlines that could forge log entries
        sanitized = RemoveControlCharacters(sanitized);

        // Redact sensitive information patterns
        if (redactSensitiveData)
            sanitized = RedactSensitiveData(sanitized);

        // HTML encode to prevent HTML injection in log viewers
        sanitized = HttpUtility.HtmlEncode(sanitized);

        // Limit length to prevent log flooding
        const int maxLength = 500;
        if (sanitized.Length > maxLength)
            sanitized = sanitized.Substring(0, maxLength) + "... [truncated]";

        return sanitized;
    }

    /// <summary>
    ///     Removes all control characters, newlines, and potential log forging characters
    /// </summary>
    private static string RemoveControlCharacters(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Remove all newline variations
        var cleaned = input
            .Replace("\r\n", " ")
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace("\t", " ")
            .Replace(Environment.NewLine, " ");

        // Remove other control characters (ASCII 0-31 and 127)
        var result = new System.Text.StringBuilder(cleaned.Length);
        foreach (var ch in cleaned)
        {
            if (ch >= 32 && ch != 127) // Keep only printable characters
            {
                result.Append(ch);
            }
            else
            {
                result.Append(' '); // Replace control chars with space
            }
        }

        return result.ToString();
    }

    /// <summary>
    ///     Redacts sensitive information like emails, phone numbers, GUIDs, and IP addresses
    /// </summary>
    private static string RedactSensitiveData(string message)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;

        var redacted = message;

        // Redact email addresses
        redacted = EmailPattern.Replace(redacted, match =>
        {
            var email = match.Value;
            var atIndex = email.IndexOf('@');
            if (atIndex > 2)
                return email.Substring(0, 2) + "***@" + email.Substring(atIndex + 1);
            return "***@***";
        });

        // Redact phone numbers
        redacted = PhonePattern.Replace(redacted, "XXX-XXX-****");

        // Redact GUIDs (keep first 8 chars for debugging)
        redacted = GuidPattern.Replace(redacted, match =>
            match.Value.Substring(0, 8) + "-****-****-****-************");

        // Redact IP addresses (keep first octet)
        redacted = IpPattern.Replace(redacted, match =>
        {
            var parts = match.Value.Split('.');
            return parts.Length == 4 ? $"{parts[0]}.***.***.***" : "***.***.***.***";
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
            "connectionstring", "email", "phone", "creditcard", "bearer", "session"
        };

        return sensitiveKeywords
            .Select(keyword => new Regex(
                $@"({keyword})\s*[:=]\s*[^\s,;]+",
                RegexOptions.IgnoreCase | RegexOptions.Compiled))
            .Aggregate(message, (current, pattern) => pattern.Replace(current, "$1: [REDACTED]"));
    }

    /// <summary>
    ///     Safely gets the caller class name without exposing full system paths
    /// </summary>
    private static string GetSafeClassName()
    {
        try
        {
            var frame = new StackTrace().GetFrame(1); // Adjusted frame index
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

        // Remove async suffix and special characters for cleaner logs
        var sanitized = memberName
            .Replace("Async", "")
            .Replace("<>", "")
            .Replace(".", "_");

        // HTML encode for safety
        return HttpUtility.HtmlEncode(sanitized);
    }
}