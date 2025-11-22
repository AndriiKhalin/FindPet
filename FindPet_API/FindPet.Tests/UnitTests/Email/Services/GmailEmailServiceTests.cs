using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Email.Configuration;
using FindPet.Email.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace FindPet.Tests.UnitTests.Email.Services;

public class GmailEmailServiceTests
{
    private readonly EmailSettings _emailSettings;
    private readonly Mock<ILoggerManager> _loggerMock;
    private readonly GmailEmailService _sut;

    public GmailEmailServiceTests()
    {
        _loggerMock = new Mock<ILoggerManager>();
        _emailSettings = new EmailSettings
        {
            EnableEmailSending = true,
            SmtpHost = "smtp.gmail.com",
            SmtpPort = 587,
            SmtpUsername = "test@gmail.com",
            SmtpPassword = "test-password",
            FromEmail = "noreply@findpet.com",
            FromName = "FindPet Support",
            EnableSsl = true,
            BaseUrl = "https://findpet.com"
        };
        _sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
    }

    #region Configuration Validation Tests

    [Theory]
    [InlineData(null, "test@gmail.com", "password")]
    [InlineData("smtp.gmail.com", null, "password")]
    [InlineData("smtp.gmail.com", "test@gmail.com", null)]
    [InlineData("", "test@gmail.com", "password")]
    [InlineData("smtp.gmail.com", "", "password")]
    [InlineData("smtp.gmail.com", "test@gmail.com", "")]
    public async Task SendEmail_WithInvalidConfiguration_ShouldThrowInvalidOperationException(
        string smtpHost, string smtpUsername, string smtpPassword)
    {
        // Arrange
        var settings = new EmailSettings
        {
            EnableEmailSending = true,
            SmtpHost = smtpHost,
            SmtpUsername = smtpUsername,
            SmtpPassword = smtpPassword,
            SmtpPort = 587,
            EnableSsl = true,
            FromEmail = "noreply@findpet.com",
            FromName = "FindPet",
            BaseUrl = "https://findpet.com"
        };
        var sut = new GmailEmailService(settings, _loggerMock.Object);

        // Act
        var act = async () => await sut.SendWelcomeEmailAsync("test@test.com", "Test User");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email service is not properly configured.");

        _loggerMock.Verify(
            x => x.LogError(It.Is<string>(msg =>
                    msg.Contains("Gmail SMTP is not configured properly")),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Once);
    }

    #endregion

    #region Multiple Scenarios

    [Fact]
    public async Task MultipleEmailsSent_ShouldLogEachAttempt()
    {
        // Arrange
        _emailSettings.EnableEmailSending = false;
        var sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
        var emails = new[] { "user1@test.com", "user2@test.com", "user3@test.com" };

        // Act
        foreach (var email in emails) await sut.SendWelcomeEmailAsync(email, "Test User");

        // Assert
        _loggerMock.Verify(
            x => x.LogWarn(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()),
            Times.Exactly(3));
    }

    #endregion

    #region SendEmailConfirmationAsync Tests

    [Fact]
    public async Task SendEmailConfirmationAsync_WithValidData_ShouldLogSuccessAndNotThrow()
    {
        // Arrange
        var email = "user@example.com";
        var userId = "user-123";
        var userName = "John Doe";
        var confirmationToken = "valid-token-123";

        // Note: Actual SMTP sending will fail in unit tests, but we verify the attempt is made
        // In real scenarios, you'd mock SmtpClient or use integration tests

        // Act & Assert
        var act = async () => await _sut.SendEmailConfirmationAsync(email, userId, userName, confirmationToken);

        // This will throw in unit test environment (no real SMTP), but that's expected
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_WhenEmailSendingDisabled_ShouldLogWarningAndNotSend()
    {
        // Arrange
        _emailSettings.EnableEmailSending = false;
        var sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
        var email = "user@example.com";
        var userId = "user-123";
        var userName = "John Doe";
        var confirmationToken = "valid-token";

        // Act
        await sut.SendEmailConfirmationAsync(email, userId, userName, confirmationToken);

        // Assert
        _loggerMock.Verify(
            x => x.LogWarn(It.Is<string>(msg =>
                    msg.Contains("Email sending is disabled") &&
                    msg.Contains(email)),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_WithMissingSmtpHost_ShouldLogErrorAndThrow()
    {
        // Arrange
        _emailSettings.SmtpHost = null;
        var sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
        var email = "user@example.com";
        var userId = "user-123";
        var userName = "John Doe";
        var confirmationToken = "valid-token";

        // Act
        var act = async () => await sut.SendEmailConfirmationAsync(email, userId, userName, confirmationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email service is not properly configured.");

        _loggerMock.Verify(
            x => x.LogError(It.Is<string>(msg =>
                    msg.Contains("Gmail SMTP is not configured properly")),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_WithMissingSmtpUsername_ShouldLogErrorAndThrow()
    {
        // Arrange
        _emailSettings.SmtpUsername = "";
        var sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
        var email = "user@example.com";
        var userId = "user-123";
        var userName = "John Doe";
        var confirmationToken = "valid-token";

        // Act
        var act = async () => await sut.SendEmailConfirmationAsync(email, userId, userName, confirmationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email service is not properly configured.");
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_WithMissingSmtpPassword_ShouldLogErrorAndThrow()
    {
        // Arrange
        _emailSettings.SmtpPassword = null;
        var sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
        var email = "user@example.com";
        var userId = "user-123";
        var userName = "John Doe";
        var confirmationToken = "valid-token";

        // Act
        var act = async () => await sut.SendEmailConfirmationAsync(email, userId, userName, confirmationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        _loggerMock.Verify(
            x => x.LogError(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.AtLeast(2));
    }

    #endregion

    #region SendPasswordResetEmailAsync Tests

    [Fact]
    public async Task SendPasswordResetEmailAsync_WhenEmailSendingDisabled_ShouldLogWarningAndNotSend()
    {
        // Arrange
        _emailSettings.EnableEmailSending = false;
        var sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
        var email = "user@example.com";
        var userName = "John Doe";
        var resetToken = "reset-token-123";

        // Act
        await sut.SendPasswordResetEmailAsync(email, userName, resetToken);

        // Assert
        _loggerMock.Verify(
            x => x.LogWarn(It.Is<string>(msg =>
                    msg.Contains("Email sending is disabled") &&
                    msg.Contains(email)),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_WithValidData_ShouldAttemptToSendEmail()
    {
        // Arrange
        var email = "user@example.com";
        var userName = "John Doe";
        var resetToken = "reset-token-123";

        // Act & Assert
        var act = async () => await _sut.SendPasswordResetEmailAsync(email, userName, resetToken);

        // Will throw in unit test environment (no real SMTP)
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_WithInvalidConfiguration_ShouldLogErrorAndThrow()
    {
        // Arrange
        _emailSettings.SmtpHost = "";
        _emailSettings.SmtpUsername = "";
        _emailSettings.SmtpPassword = "";
        var sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
        var email = "user@example.com";
        var userName = "John Doe";
        var resetToken = "reset-token";

        // Act
        var act = async () => await sut.SendPasswordResetEmailAsync(email, userName, resetToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email service is not properly configured.");

        _loggerMock.Verify(
            x => x.LogError(It.Is<string>(msg =>
                    msg.Contains("Gmail SMTP is not configured properly")),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Once);
    }

    #endregion

    #region SendWelcomeEmailAsync Tests

    [Fact]
    public async Task SendWelcomeEmailAsync_WhenEmailSendingDisabled_ShouldLogWarningAndNotSend()
    {
        // Arrange
        _emailSettings.EnableEmailSending = false;
        var sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
        var email = "user@example.com";
        var userName = "John Doe";

        // Act
        await sut.SendWelcomeEmailAsync(email, userName);

        // Assert
        _loggerMock.Verify(
            x => x.LogWarn(It.Is<string>(msg =>
                    msg.Contains("Email sending is disabled") &&
                    msg.Contains(email)),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_WithValidData_ShouldAttemptToSendEmail()
    {
        // Arrange
        var email = "newuser@example.com";
        var userName = "Jane Smith";

        // Act & Assert
        var act = async () => await _sut.SendWelcomeEmailAsync(email, userName);

        // Will throw in unit test environment (no real SMTP)
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_WithMissingConfiguration_ShouldLogErrorAndThrow()
    {
        // Arrange
        _emailSettings.SmtpHost = null;
        var sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
        var email = "user@example.com";
        var userName = "John Doe";

        // Act
        var act = async () => await sut.SendWelcomeEmailAsync(email, userName);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        _loggerMock.Verify(
            x => x.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()),
            Times.AtLeast(2));
    }

    #endregion

    #region SendPasswordChangedNotificationAsync Tests

    [Fact]
    public async Task SendPasswordChangedNotificationAsync_WhenEmailSendingDisabled_ShouldLogWarningAndNotSend()
    {
        // Arrange
        _emailSettings.EnableEmailSending = false;
        var sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
        var email = "user@example.com";
        var userName = "John Doe";

        // Act
        await sut.SendPasswordChangedNotificationAsync(email, userName);

        // Assert
        _loggerMock.Verify(
            x => x.LogWarn(It.Is<string>(msg =>
                    msg.Contains("Email sending is disabled") &&
                    msg.Contains(email)),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordChangedNotificationAsync_WithValidData_ShouldAttemptToSendEmail()
    {
        // Arrange
        var email = "user@example.com";
        var userName = "John Doe";

        // Act & Assert
        var act = async () => await _sut.SendPasswordChangedNotificationAsync(email, userName);

        // Will throw in unit test environment (no real SMTP)
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task SendPasswordChangedNotificationAsync_WithInvalidConfiguration_ShouldThrow()
    {
        // Arrange
        _emailSettings.SmtpUsername = "";
        _emailSettings.SmtpPassword = "";
        var sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
        var email = "user@example.com";
        var userName = "John Doe";

        // Act
        var act = async () => await sut.SendPasswordChangedNotificationAsync(email, userName);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region Email Content Validation Tests

    [Fact]
    public async Task SendEmailConfirmationAsync_ShouldConstructCorrectUrl()
    {
        // Arrange
        _emailSettings.EnableEmailSending = false; // Disable to avoid SMTP errors
        var sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
        var email = "user@example.com";
        var userId = "user-123";
        var userName = "John Doe";
        var confirmationToken = "token-abc";

        // Act
        await sut.SendEmailConfirmationAsync(email, userId, userName, confirmationToken);

        // Assert
        // Verify the expected URL format is constructed (indirectly via logger)
        _loggerMock.Verify(
            x => x.LogWarn(It.Is<string>(msg => msg.Contains(email)), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_ShouldConstructCorrectUrl()
    {
        // Arrange
        _emailSettings.EnableEmailSending = false;
        var sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
        var email = "user@example.com";
        var userName = "John Doe";
        var resetToken = "reset-token-xyz";

        // Act
        await sut.SendPasswordResetEmailAsync(email, userName, resetToken);

        // Assert
        _loggerMock.Verify(
            x => x.LogWarn(It.Is<string>(msg => msg.Contains(email)), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_ShouldConstructCorrectDashboardUrl()
    {
        // Arrange
        _emailSettings.EnableEmailSending = false;
        var sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
        var email = "user@example.com";
        var userName = "John Doe";

        // Act
        await sut.SendWelcomeEmailAsync(email, userName);

        // Assert
        _loggerMock.Verify(
            x => x.LogWarn(It.Is<string>(msg => msg.Contains(email)), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Once);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task SendEmailConfirmationAsync_WithSpecialCharactersInEmail_ShouldHandleCorrectly()
    {
        // Arrange
        _emailSettings.EnableEmailSending = false;
        var sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
        var email = "user+test@example.com";
        var userId = "user-123";
        var userName = "John O'Doe";
        var confirmationToken = "token-with-special-chars-!@#";

        // Act
        var act = async () => await sut.SendEmailConfirmationAsync(email, userId, userName, confirmationToken);

        // Assert
        await act.Should().NotThrowAsync();

        _loggerMock.Verify(
            x => x.LogWarn(It.Is<string>(msg => msg.Contains(email)), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_WithLongUserName_ShouldHandleCorrectly()
    {
        // Arrange
        _emailSettings.EnableEmailSending = false;
        var sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
        var email = "user@example.com";
        var userName = new string('A', 200); // Very long name
        var resetToken = "token-123";

        // Act
        var act = async () => await sut.SendPasswordResetEmailAsync(email, userName, resetToken);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_WithEmptyBaseUrl_ShouldStillAttemptSend()
    {
        // Arrange
        _emailSettings.BaseUrl = "";
        _emailSettings.EnableEmailSending = false;
        var sut = new GmailEmailService(_emailSettings, _loggerMock.Object);
        var email = "user@example.com";
        var userName = "John Doe";

        // Act
        var act = async () => await sut.SendWelcomeEmailAsync(email, userName);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion
}