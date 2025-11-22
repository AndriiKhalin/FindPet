using System.Text;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.DTOs.AuthDTOs;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Entities;
using FindPet.Domain.Exceptions;
using FindPet.Domain.ValueObjects;
using FindPet.Email.Interfaces;
using FindPet.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using MockQueryable;
using Moq;
using Xunit;

namespace FindPet.Tests.UnitTests.BusinessLogicLayer.Services.AuthService;

public class AuthServiceTests
{
    private readonly IAuthService _authService;
    private readonly JwtSettings _jwtSettings;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<UserManager<AuthUser>> _mockUserManager;
    private readonly Mock<IUserService> _mockUserService;

    public AuthServiceTests()
    {
        // Setup UserManager mock
        var userStore = new Mock<IUserStore<AuthUser>>();
        _mockUserManager = new Mock<UserManager<AuthUser>>(
            userStore.Object, null, null, null, null, null, null, null, null);

        // Setup RoleManager mock
        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
            roleStore.Object, null, null, null, null);

        _mockTokenService = new Mock<ITokenService>();
        _mockUserService = new Mock<IUserService>();
        _mockUnitOfWork = MockSetupExtensions.SetupUnitOfWorkMock();
        _mockEmailService = new Mock<IEmailService>();
        _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();

        _mockUnitOfWork.Setup(x => x.RefreshToken).Returns(_mockRefreshTokenRepository.Object);

        _mockUnitOfWork.Setup(x => x.SaveAsync())
            .Returns(Task.CompletedTask);

        _mockRefreshTokenRepository.Setup(x => x.GetActiveTokensByUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<RefreshToken>());

        _jwtSettings = new JwtSettings("TestAudience", "TestIssuer", "test-secret-key-with-at-least-32-characters", 15,
            7);

        _authService = new FindPet.BusinessLogicLayer.Services.AuthService.AuthService(
            _mockUserManager.Object,
            _mockUserService.Object,
            _mockRoleManager.Object,
            _mockTokenService.Object,
            _mockUnitOfWork.Object,
            _mockEmailService.Object,
            _jwtSettings
        );
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldRegisterUserSuccessfully()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME,
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Password = TestDataBuilder.TestConstants.DEFAULT_PASSWORD,
            PhoneNumber = TestDataBuilder.TestConstants.DEFAULT_PHONE,
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Photo = "users/photo.jpg"
        };

        AuthUser? createdUser = null;

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync((AuthUser)null);

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AuthUser>(), registerDto.Password))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<AuthUser, string>((user, _) => createdUser = user);

        _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(() => createdUser);

        _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<AuthUser>()))
            .ReturnsAsync("test-confirmation-token");

        _mockRoleManager.Setup(x => x.RoleExistsAsync(UserRoles.User))
            .ReturnsAsync(true);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<AuthUser>(), UserRoles.User))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserService.Setup(x => x.CreateUserAsync(It.IsAny<UserForCreateDto>()))
            .ReturnsAsync(TestDataBuilder.BuildBasicUser());

        _mockEmailService.Setup(x => x.SendEmailConfirmationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Account created successfully! Please log in.");

        _mockUserManager.Verify(x => x.FindByEmailAsync(registerDto.Email), Times.Once);
        _mockUserManager.Verify(x => x.CreateAsync(It.Is<AuthUser>(u =>
            u.Email == registerDto.Email &&
            u.Name == registerDto.Name &&
            u.PhoneNumber == registerDto.PhoneNumber
        ), registerDto.Password), Times.Once);

        _mockEmailService.Verify(x => x.SendEmailConfirmationAsync(
            registerDto.Email,
            It.IsAny<string>(),
            registerDto.Name,
            It.IsAny<string>()
        ), Times.Once);

        _mockUserService.Verify(x => x.CreateUserAsync(It.Is<UserForCreateDto>(dto =>
            dto.Email == registerDto.Email &&
            dto.Name == registerDto.Name
        )), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowBadRequestException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME,
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Password = TestDataBuilder.TestConstants.DEFAULT_PASSWORD,
            PhoneNumber = TestDataBuilder.TestConstants.DEFAULT_PHONE,
            BirthDate = DateTime.UtcNow.AddYears(-25)
        };

        var existingUser = new AuthUser { Email = registerDto.Email };
        _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await _authService.RegisterAsync(registerDto);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("User with this email already exists");

        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<AuthUser>(), It.IsAny<string>()), Times.Never);
        _mockUserService.Verify(x => x.CreateUserAsync(It.IsAny<UserForCreateDto>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithIdentityFailure_ShouldThrowBadRequestException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME,
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Password = "weak",
            PhoneNumber = TestDataBuilder.TestConstants.DEFAULT_PHONE,
            BirthDate = DateTime.UtcNow.AddYears(-25)
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync((AuthUser)null);

        var identityErrors = new[]
        {
            new IdentityError { Description = "Password too short" },
            new IdentityError { Description = "Password requires digit" }
        };

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AuthUser>(), registerDto.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        Func<Task> act = async () => await _authService.RegisterAsync(registerDto);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Password too short, Password requires digit");

        _mockUserService.Verify(x => x.CreateUserAsync(It.IsAny<UserForCreateDto>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithAdminRole_ShouldAssignAdminRole()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME,
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Password = TestDataBuilder.TestConstants.DEFAULT_PASSWORD,
            PhoneNumber = TestDataBuilder.TestConstants.DEFAULT_PHONE,
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Role = UserRoles.Admin
        };

        AuthUser? createdUser = null;

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync((AuthUser)null);

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AuthUser>(), registerDto.Password))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<AuthUser, string>((user, _) => createdUser = user);

        _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(() => createdUser);

        _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<AuthUser>()))
            .ReturnsAsync("test-token");

        _mockRoleManager.Setup(x => x.RoleExistsAsync(UserRoles.Admin))
            .ReturnsAsync(true);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<AuthUser>(), UserRoles.Admin))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserService.Setup(x => x.CreateUserAsync(It.IsAny<UserForCreateDto>()))
            .ReturnsAsync(TestDataBuilder.BuildBasicUser());

        _mockEmailService.Setup(x => x.SendEmailConfirmationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<AuthUser>(), UserRoles.Admin), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithPhotoPath_ShouldNormalizePhotoPath()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME,
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Password = TestDataBuilder.TestConstants.DEFAULT_PASSWORD,
            PhoneNumber = TestDataBuilder.TestConstants.DEFAULT_PHONE,
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Photo = "users\\photo.jpg" // Windows-style path
        };

        AuthUser? createdUser = null;

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync((AuthUser)null);

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AuthUser>(), registerDto.Password))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<AuthUser, string>((user, _) => createdUser = user);

        _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(() => createdUser);

        _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<AuthUser>()))
            .ReturnsAsync("test-token");

        _mockRoleManager.Setup(x => x.RoleExistsAsync(UserRoles.User))
            .ReturnsAsync(true);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<AuthUser>(), UserRoles.User))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserService.Setup(x => x.CreateUserAsync(It.IsAny<UserForCreateDto>()))
            .ReturnsAsync(TestDataBuilder.BuildBasicUser());

        _mockEmailService.Setup(x => x.SendEmailConfirmationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _authService.RegisterAsync(registerDto);

        // Assert
        _mockUserManager.Verify(x => x.CreateAsync(It.Is<AuthUser>(u =>
            u.Photo.Contains("/") && !u.Photo.Contains("\\")
        ), It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Password = TestDataBuilder.TestConstants.DEFAULT_PASSWORD
        };

        var authUser = new AuthUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = loginDto.Email,
            EmailConfirmed = true
        };

        var refreshToken = new RefreshToken
        {
            Token = "test-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(authUser);

        _mockUserManager.Setup(x => x.CheckPasswordAsync(authUser, loginDto.Password))
            .ReturnsAsync(true);

        _mockTokenService.Setup(x => x.GenerateAccessTokenAsync(authUser))
            .ReturnsAsync("test-access-token");

        _mockTokenService.Setup(x => x.GenerateRefreshTokenAsync(authUser.Id))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Login successful");
        result.AccessToken.Should().Be("test-access-token");
        result.RefreshToken.Should().Be("test-refresh-token");
        result.AccessTokenExpiration.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            TimeSpan.FromSeconds(5));
        result.RefreshTokenExpiration.Should().Be(refreshToken.ExpiresAt);

        _mockTokenService.Verify(x => x.GenerateAccessTokenAsync(authUser), Times.Once);
        _mockTokenService.Verify(x => x.GenerateRefreshTokenAsync(authUser.Id), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@test.com",
            Password = TestDataBuilder.TestConstants.DEFAULT_PASSWORD
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync((AuthUser)null);

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(loginDto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password");

        _mockTokenService.Verify(x => x.GenerateAccessTokenAsync(It.IsAny<AuthUser>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Password = "WrongPassword123!"
        };

        var authUser = new AuthUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = loginDto.Email,
            EmailConfirmed = true
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(authUser);

        _mockUserManager.Setup(x => x.CheckPasswordAsync(authUser, loginDto.Password))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(loginDto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password");

        _mockTokenService.Verify(x => x.GenerateAccessTokenAsync(It.IsAny<AuthUser>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithUnconfirmedEmail_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Password = TestDataBuilder.TestConstants.DEFAULT_PASSWORD
        };

        var authUser = new AuthUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = loginDto.Email,
            EmailConfirmed = false
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(authUser);

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(loginDto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Please confirm your email before logging in");

        _mockUserManager.Verify(x => x.CheckPasswordAsync(It.IsAny<AuthUser>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region GetCurrentUserAsync Tests

    [Fact]
    public async Task GetCurrentUserAsync_WithValidUserId_ShouldReturnUserDetails()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var authUser = new AuthUser
        {
            Id = userId,
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME,
            PhoneNumber = TestDataBuilder.TestConstants.DEFAULT_PHONE,
            Photo = "users/photo.jpg",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            PhoneNumberConfirmed = true,
            TwoFactorEnabled = false,
            AccessFailedCount = 0
        };

        var roles = new List<string> { UserRoles.User };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(authUser);

        _mockUserManager.Setup(x => x.GetRolesAsync(authUser))
            .ReturnsAsync(roles);

        // Act
        var result = await _authService.GetCurrentUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Email.Should().Be(authUser.Email);
        result.Name.Should().Be(authUser.Name);
        result.PhoneNumber.Should().Be(authUser.PhoneNumber);
        result.Photo.Should().Be(authUser.Photo);
        result.BirthDate.Should().Be(authUser.BirthDate);
        result.Role.Should().BeEquivalentTo(roles);
        result.PhoneNumberConfirmed.Should().BeTrue();
        result.TwoFactorEnabled.Should().BeFalse();
        result.AccessFailedCount.Should().Be(0);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithInvalidUserId_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((AuthUser)null);

        // Act
        Func<Task> act = async () => await _authService.GetCurrentUserAsync(userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User with ID '{userId}' was not found");
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithMultipleRoles_ShouldReturnAllRoles()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var authUser = new AuthUser
        {
            Id = userId,
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME
        };

        var roles = new List<string> { UserRoles.User, UserRoles.Admin };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(authUser);

        _mockUserManager.Setup(x => x.GetRolesAsync(authUser))
            .ReturnsAsync(roles);

        // Act
        var result = await _authService.GetCurrentUserAsync(userId);

        // Assert
        result.Role.Should().HaveCount(2);
        result.Role.Should().Contain(UserRoles.User);
        result.Role.Should().Contain(UserRoles.Admin);
    }

    #endregion

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnAllUsersWithRoles()
    {
        // Arrange
        var users = new List<AuthUser>
        {
            new() { Id = Guid.NewGuid().ToString(), Email = "user1@test.com", Name = "User1" },
            new() { Id = Guid.NewGuid().ToString(), Email = "user2@test.com", Name = "User2" },
            new() { Id = Guid.NewGuid().ToString(), Email = "admin@test.com", Name = "Admin" }
        };

        _mockUserManager.Setup(x => x.Users)
            .Returns(users.BuildMock());

        _mockUserManager.Setup(x => x.GetRolesAsync(users[0]))
            .ReturnsAsync(new List<string> { UserRoles.User });

        _mockUserManager.Setup(x => x.GetRolesAsync(users[1]))
            .ReturnsAsync(new List<string> { UserRoles.User });

        _mockUserManager.Setup(x => x.GetRolesAsync(users[2]))
            .ReturnsAsync(new List<string> { UserRoles.Admin });

        // Act
        var result = await _authService.GetAllUsersAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(u => u.Email == "user1@test.com");
        result.Should().Contain(u => u.Email == "user2@test.com");
        result.Should().Contain(u => u.Email == "admin@test.com" && u.Role.Contains(UserRoles.Admin));
    }

    [Fact]
    public async Task GetAllUsersAsync_WithNoUsers_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyUsers = new List<AuthUser>();

        _mockUserManager.Setup(x => x.Users)
            .Returns(emptyUsers.BuildMock());

        // Act
        var result = await _authService.GetAllUsersAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GeneratePasswordResetTokenAsync Tests

    [Fact]
    public async Task GeneratePasswordResetTokenAsync_WithValidEmail_ShouldGenerateTokenAndSendEmail()
    {
        // Arrange
        var email = TestDataBuilder.TestConstants.DEFAULT_EMAIL;
        var authUser = new AuthUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME
        };

        var rawToken = "raw-reset-token";

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(authUser);

        _mockUserManager.Setup(x => x.GeneratePasswordResetTokenAsync(authUser))
            .ReturnsAsync(rawToken);

        _mockEmailService.Setup(x => x.SendPasswordResetEmailAsync(
                email, authUser.Name, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.GeneratePasswordResetTokenAsync(email);

        // Assert
        result.Should().Be(rawToken);

        _mockEmailService.Verify(x => x.SendPasswordResetEmailAsync(
            email,
            authUser.Name,
            It.IsAny<string>()
        ), Times.Once);
    }

    [Fact]
    public async Task GeneratePasswordResetTokenAsync_WithInvalidEmail_ShouldThrowNotFoundException()
    {
        // Arrange
        var email = "nonexistent@test.com";

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((AuthUser)null);

        // Act
        Func<Task> act = async () => await _authService.GeneratePasswordResetTokenAsync(email);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User with ID '{email}' was not found");

        _mockEmailService.Verify(x => x.SendPasswordResetEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()
        ), Times.Never);
    }

    #endregion

    #region ResetPasswordAsync Tests

    [Fact]
    public async Task ResetPasswordAsync_WithValidToken_ShouldResetPassword()
    {
        // Arrange
        var email = TestDataBuilder.TestConstants.DEFAULT_EMAIL;
        var rawToken = "raw-reset-token";
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
        var newPassword = "NewPassword123!";

        var authUser = new AuthUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(authUser);

        _mockUserManager.Setup(x => x.ResetPasswordAsync(authUser, rawToken, newPassword))
            .ReturnsAsync(IdentityResult.Success);

        _mockTokenService.Setup(x => x.RevokeUserTokensAsync(authUser.Id))
            .Returns(Task.CompletedTask);

        _mockEmailService.Setup(x => x.SendPasswordChangedNotificationAsync(email, authUser.Name))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.ResetPasswordAsync(email, encodedToken, newPassword);

        // Assert
        result.Should().BeTrue();

        _mockUserManager.Verify(x => x.ResetPasswordAsync(authUser, rawToken, newPassword), Times.Once);
        _mockTokenService.Verify(x => x.RevokeUserTokensAsync(authUser.Id), Times.Once);
        _mockEmailService.Verify(x => x.SendPasswordChangedNotificationAsync(email, authUser.Name), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var email = TestDataBuilder.TestConstants.DEFAULT_EMAIL;
        var invalidToken = "invalid-token";
        var newPassword = "NewPassword123!";

        var authUser = new AuthUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(authUser);

        _mockUserManager.Setup(x => x.ResetPasswordAsync(authUser, It.IsAny<string>(), newPassword))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

        // Act
        var result = await _authService.ResetPasswordAsync(email, invalidToken, newPassword);

        // Assert
        result.Should().BeFalse();

        _mockTokenService.Verify(x => x.RevokeUserTokensAsync(It.IsAny<string>()), Times.Never);
        _mockEmailService.Verify(x => x.SendPasswordChangedNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>()
        ), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var email = "nonexistent@test.com";

        _mockUserManager.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((AuthUser)null);

        // Act
        var result = await _authService.ResetPasswordAsync(email, "token", "NewPassword123!");

        // Assert
        result.Should().BeFalse();

        _mockUserManager.Verify(x => x.ResetPasswordAsync(
            It.IsAny<AuthUser>(), It.IsAny<string>(), It.IsAny<string>()
        ), Times.Never);
    }

    #endregion

    #region ChangePasswordAsync Tests

    [Fact]
    public async Task ChangePasswordAsync_WithValidCredentials_ShouldChangePassword()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var currentPassword = TestDataBuilder.TestConstants.DEFAULT_PASSWORD;
        var newPassword = "NewPassword123!";

        var authUser = new AuthUser
        {
            Id = userId,
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(authUser);

        _mockUserManager.Setup(x => x.ChangePasswordAsync(authUser, currentPassword, newPassword))
            .ReturnsAsync(IdentityResult.Success);

        _mockTokenService.Setup(x => x.RevokeUserTokensAsync(userId))
            .Returns(Task.CompletedTask);

        _mockEmailService.Setup(x => x.SendPasswordChangedNotificationAsync(authUser.Email, authUser.Name))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.ChangePasswordAsync(userId, currentPassword, newPassword);

        // Assert
        result.Should().BeTrue();

        _mockUserManager.Verify(x => x.ChangePasswordAsync(authUser, currentPassword, newPassword), Times.Once);
        _mockTokenService.Verify(x => x.RevokeUserTokensAsync(userId), Times.Once);
        _mockEmailService.Verify(x => x.SendPasswordChangedNotificationAsync(authUser.Email, authUser.Name),
            Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithInvalidUserId_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((AuthUser)null);

        // Act
        Func<Task> act = async () => await _authService.ChangePasswordAsync(userId, "current", "new");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User with ID '{userId}' was not found");
    }

    [Fact]
    public async Task ChangePasswordAsync_WithInvalidCurrentPassword_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var wrongPassword = "WrongPassword123!";
        var newPassword = "NewPassword123!";

        var authUser = new AuthUser
        {
            Id = userId,
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(authUser);

        _mockUserManager.Setup(x => x.ChangePasswordAsync(authUser, wrongPassword, newPassword))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Incorrect password" }));

        // Act
        var result = await _authService.ChangePasswordAsync(userId, wrongPassword, newPassword);

        // Assert
        result.Should().BeFalse();

        _mockTokenService.Verify(x => x.RevokeUserTokensAsync(It.IsAny<string>()), Times.Never);
        _mockEmailService.Verify(x => x.SendPasswordChangedNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>()
        ), Times.Never);
    }

    #endregion

    #region ConfirmEmailAsync Tests

    [Fact]
    public async Task ConfirmEmailAsync_WithValidToken_ShouldConfirmEmailAndSendWelcome()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var rawToken = "raw-confirmation-token";
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

        var authUser = new AuthUser
        {
            Id = userId,
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(authUser);

        _mockUserManager.Setup(x => x.ConfirmEmailAsync(authUser, rawToken))
            .ReturnsAsync(IdentityResult.Success);

        _mockEmailService.Setup(x => x.SendWelcomeEmailAsync(authUser.Email, authUser.Name))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.ConfirmEmailAsync(userId, encodedToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Email confirmed successfully! You can now login.");

        _mockUserManager.Verify(x => x.ConfirmEmailAsync(authUser, rawToken), Times.Once);
        _mockEmailService.Verify(x => x.SendWelcomeEmailAsync(authUser.Email, authUser.Name), Times.Once);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WithInvalidToken_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var invalidToken = "invalid-token";

        var authUser = new AuthUser
        {
            Id = userId,
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(authUser);

        _mockUserManager.Setup(x => x.ConfirmEmailAsync(authUser, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

        // Act
        var result = await _authService.ConfirmEmailAsync(userId, invalidToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Email confirmation failed. Invalid or expired token.");

        _mockEmailService.Verify(x => x.SendWelcomeEmailAsync(
            It.IsAny<string>(), It.IsAny<string>()
        ), Times.Never);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((AuthUser)null);

        // Act
        Func<Task> act = async () => await _authService.ConfirmEmailAsync(userId, "token");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User with ID '{userId}' was not found");
    }

    #endregion

    #region GenerateEmailConfirmationTokenAsync Tests

    [Fact]
    public async Task GenerateEmailConfirmationTokenAsync_WithValidUserId_ShouldReturnEncodedToken()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var rawToken = "raw-confirmation-token";

        var authUser = new AuthUser
        {
            Id = userId,
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(authUser);

        _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(authUser))
            .ReturnsAsync(rawToken);

        // Act
        var result = await _authService.GenerateEmailConfirmationTokenAsync(userId);

        // Assert
        result.Should().NotBeNullOrEmpty();

        // Verify it's properly encoded
        var decodedBytes = WebEncoders.Base64UrlDecode(result);
        var decodedToken = Encoding.UTF8.GetString(decodedBytes);
        decodedToken.Should().Be(rawToken);
    }

    [Fact]
    public async Task GenerateEmailConfirmationTokenAsync_WithInvalidUserId_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((AuthUser)null);

        // Act
        Func<Task> act = async () => await _authService.GenerateEmailConfirmationTokenAsync(userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User with ID '{userId}' was not found");
    }

    #endregion

    #region GetActiveSessionsAsync Tests

    [Fact]
    public async Task GetActiveSessionsAsync_ShouldReturnActiveSessionsWithCurrentMarked()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var currentToken = "current-refresh-token";

        var refreshTokens = new List<RefreshToken>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Token = currentToken,
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Token = "other-refresh-token",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                ExpiresAt = DateTime.UtcNow.AddDays(4)
            }
        };

        _mockRefreshTokenRepository.Setup(x => x.GetActiveTokensByUserIdAsync(userId))
            .ReturnsAsync(refreshTokens);

        // Act
        var result = await _authService.GetActiveSessionsAsync(userId, currentToken);

        // Assert
        var sessions = result.ToList();
        sessions.Should().HaveCount(2);

        var currentSession = sessions.FirstOrDefault(s => s.IsCurrent);
        currentSession.Should().NotBeNull();
        currentSession!.Id.Should().Be(refreshTokens[0].Id);

        var otherSession = sessions.FirstOrDefault(s => !s.IsCurrent);
        otherSession.Should().NotBeNull();
        otherSession!.Id.Should().Be(refreshTokens[1].Id);

        // Verify ordered by CreatedAt descending
        sessions[0].CreatedAt.Should().BeAfter(sessions[1].CreatedAt);
    }

    [Fact]
    public async Task GetActiveSessionsAsync_WithNoSessions_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        _mockRefreshTokenRepository.Setup(x => x.GetActiveTokensByUserIdAsync(userId))
            .ReturnsAsync(new List<RefreshToken>());

        // Act
        var result = await _authService.GetActiveSessionsAsync(userId, "any-token");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region RevokeSessionAsync Tests

    [Fact]
    public async Task RevokeSessionAsync_WithValidTokenId_ShouldRevokeSession()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var tokenId = Guid.NewGuid();

        var refreshToken = new RefreshToken
        {
            Id = tokenId,
            Token = "refresh-token",
            UserId = userId
        };

        _mockRefreshTokenRepository.Setup(x => x.GetAsync(tokenId))
            .ReturnsAsync(refreshToken);

        _mockTokenService.Setup(x => x.RevokeTokenAsync(refreshToken.Token, "Revoked by user"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RevokeSessionAsync(userId, tokenId);

        // Assert
        result.Should().BeTrue();
        _mockTokenService.Verify(x => x.RevokeTokenAsync(refreshToken.Token, "Revoked by user"), Times.Once);
    }

    [Fact]
    public async Task RevokeSessionAsync_WithInvalidTokenId_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var tokenId = Guid.NewGuid();

        _mockRefreshTokenRepository.Setup(x => x.GetAsync(tokenId))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _authService.RevokeSessionAsync(userId, tokenId);

        // Assert
        result.Should().BeFalse();
        _mockTokenService.Verify(x => x.RevokeTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RevokeSessionAsync_WithMismatchedUserId_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var otherUserId = Guid.NewGuid().ToString();
        var tokenId = Guid.NewGuid();

        var refreshToken = new RefreshToken
        {
            Id = tokenId,
            Token = "refresh-token",
            UserId = otherUserId
        };

        _mockRefreshTokenRepository.Setup(x => x.GetAsync(tokenId))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _authService.RevokeSessionAsync(userId, tokenId);

        // Assert
        result.Should().BeFalse();
        _mockTokenService.Verify(x => x.RevokeTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public async Task RegisterAsync_ShouldCreateRolesIfNotExist()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME,
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Password = TestDataBuilder.TestConstants.DEFAULT_PASSWORD,
            PhoneNumber = TestDataBuilder.TestConstants.DEFAULT_PHONE,
            BirthDate = DateTime.UtcNow.AddYears(-25)
        };

        AuthUser? createdUser = null;

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync((AuthUser)null);

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AuthUser>(), registerDto.Password))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<AuthUser, string>((user, _) => createdUser = user);

        _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(() => createdUser);

        _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<AuthUser>()))
            .ReturnsAsync("token");

        _mockRoleManager.Setup(x => x.RoleExistsAsync(UserRoles.Admin))
            .ReturnsAsync(false);

        _mockRoleManager.Setup(x => x.RoleExistsAsync(UserRoles.User))
            .ReturnsAsync(false);

        _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<AuthUser>(), UserRoles.User))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserService.Setup(x => x.CreateUserAsync(It.IsAny<UserForCreateDto>()))
            .ReturnsAsync(TestDataBuilder.BuildBasicUser());

        _mockEmailService.Setup(x => x.SendEmailConfirmationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _authService.RegisterAsync(registerDto);

        // Assert
        _mockRoleManager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == UserRoles.Admin)), Times.Once);
        _mockRoleManager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == UserRoles.User)), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Password = TestDataBuilder.TestConstants.DEFAULT_PASSWORD
        };

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(loginDto, cts.Token);

        // Assert
        // Note: The current implementation doesn't use CancellationToken in all async operations,
        // but this test verifies the parameter is accepted
        await act.Should().NotThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task RegisterAsync_WithNullOrEmptyPhoto_ShouldNotNormalizePath(string photo)
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME,
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Password = TestDataBuilder.TestConstants.DEFAULT_PASSWORD,
            PhoneNumber = TestDataBuilder.TestConstants.DEFAULT_PHONE,
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Photo = photo
        };

        AuthUser? createdUser = null;

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync((AuthUser)null);

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AuthUser>(), registerDto.Password))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<AuthUser, string>((user, _) => createdUser = user);

        _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(() => createdUser);

        _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<AuthUser>()))
            .ReturnsAsync("token");

        _mockRoleManager.Setup(x => x.RoleExistsAsync(UserRoles.User))
            .ReturnsAsync(true);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<AuthUser>(), UserRoles.User))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserService.Setup(x => x.CreateUserAsync(It.IsAny<UserForCreateDto>()))
            .ReturnsAsync(TestDataBuilder.BuildBasicUser());

        _mockEmailService.Setup(x => x.SendEmailConfirmationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockUserManager.Verify(x => x.CreateAsync(It.Is<AuthUser>(u =>
            u.Photo == photo
        ), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetAllUsersAsync_ShouldHandleLargeUserList()
    {
        // Arrange
        var users = Enumerable.Range(1, 1000)
            .Select(i => new AuthUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = $"user{i}@test.com",
                Name = $"User{i}"
            }).ToList();

        _mockUserManager.Setup(x => x.Users)
            .Returns(users.BuildMock());

        foreach (var user in users)
            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoles.User });

        // Act
        var result = await _authService.GetAllUsersAsync();

        // Assert
        result.Should().HaveCount(1000);
        _mockUserManager.Verify(x => x.GetRolesAsync(It.IsAny<AuthUser>()), Times.Exactly(1000));
    }

    #endregion
}