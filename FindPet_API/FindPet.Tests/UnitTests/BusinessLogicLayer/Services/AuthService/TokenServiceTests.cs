using System.IdentityModel.Tokens.Jwt;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.BusinessLogicLayer.Services.AuthService;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.Entities;
using FindPet.Domain.Exceptions;
using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Domain.ValueObjects;
using FindPet.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace FindPet.Tests.UnitTests.BusinessLogicLayer.Services.AuthService;

public class TokenServiceTests
{
    private readonly JwtSettings _jwtSettings;
    private readonly Mock<ILoggerManager> _mockLogger;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<UserManager<AuthUser>> _mockUserManager;
    private readonly ITokenService _tokenService;

    public TokenServiceTests()
    {
        // Setup UserManager mock
        var userStore = new Mock<IUserStore<AuthUser>>();
        _mockUserManager = new Mock<UserManager<AuthUser>>(
            userStore.Object, null, null, null, null, null, null, null, null);

        _mockUnitOfWork = MockSetupExtensions.SetupUnitOfWorkMock();
        _mockLogger = MockSetupExtensions.SetupLoggerMock();
        _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();

        _mockUnitOfWork.Setup(x => x.RefreshToken).Returns(_mockRefreshTokenRepository.Object);

        _jwtSettings = new JwtSettings("TestAudience", "TestIssuer", "test-secret-key-with-at-least-32-characters", 15,
            7);

        _tokenService = new TokenService(
            _mockUserManager.Object,
            _jwtSettings,
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    #region GenerateAccessTokenAsync Tests

    //[Fact]
    //public async Task GenerateAccessTokenAsync_WithValidUser_ShouldReturnValidJwtToken()
    //{
    //    // Arrange
    //    var userId = Guid.NewGuid().ToString();
    //    var authUser = new AuthUser
    //    {
    //        Id = userId,
    //        Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
    //        Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME,
    //        UserName = TestDataBuilder.TestConstants.DEFAULT_EMAIL
    //    };

    //    var roles = new List<string> { UserRoles.User };
    //    _mockUserManager.Setup(x => x.GetRolesAsync(authUser)).ReturnsAsync(roles);

    //    // Act
    //    var token = await _tokenService.GenerateAccessTokenAsync(authUser);

    //    // Assert
    //    token.Should().NotBeNullOrEmpty();

    //    // Validate token structure
    //    var handler = new JwtSecurityTokenHandler();
    //    handler.CanReadToken(token).Should().BeTrue();

    //    var jwtToken = handler.ReadJwtToken(token);
    //    jwtToken.Should().NotBeNull();
    //    jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId);
    //    jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == authUser.Email);
    //    jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == authUser.Name);
    //    jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == UserRoles.User);
    //    jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Aud && c.Value == _jwtSettings.ValidAudience);
    //    jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Iss && c.Value == _jwtSettings.ValidIssuer);

    //    _mockUserManager.Verify(x => x.GetRolesAsync(authUser), Times.Once);
    //}

    //[Fact]
    //public async Task GenerateAccessTokenAsync_WithMultipleRoles_ShouldIncludeAllRoles()
    //{
    //    // Arrange
    //    var userId = Guid.NewGuid().ToString();
    //    var authUser = new AuthUser
    //    {
    //        Id = userId,
    //        Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
    //        Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME
    //    };

    //    var roles = new List<string> { UserRoles.User, UserRoles.Admin };
    //    _mockUserManager.Setup(x => x.GetRolesAsync(authUser)).ReturnsAsync(roles);

    //    // Act
    //    var token = await _tokenService.GenerateAccessTokenAsync(authUser);

    //    // Assert
    //    var handler = new JwtSecurityTokenHandler();
    //    var jwtToken = handler.ReadJwtToken(token);

    //    var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
    //    roleClaims.Should().HaveCount(2);
    //    roleClaims.Should().Contain(c => c.Value == UserRoles.User);
    //    roleClaims.Should().Contain(c => c.Value == UserRoles.Admin);
    //}

    //[Fact]
    //public async Task GenerateAccessTokenAsync_WithNullEmail_ShouldHandleGracefully()
    //{
    //    // Arrange
    //    var authUser = new AuthUser
    //    {
    //        Id = Guid.NewGuid().ToString(),
    //        Email = null,
    //        Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME
    //    };

    //    _mockUserManager.Setup(x => x.GetRolesAsync(authUser)).ReturnsAsync(new List<string> { UserRoles.User });

    //    // Act
    //    var token = await _tokenService.GenerateAccessTokenAsync(authUser);

    //    // Assert
    //    var handler = new JwtSecurityTokenHandler();
    //    var jwtToken = handler.ReadJwtToken(token);
    //    jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "");
    //}

    [Fact]
    public async Task GenerateAccessTokenAsync_ShouldGenerateUniqueJti()
    {
        // Arrange
        var authUser = new AuthUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME
        };

        _mockUserManager.Setup(x => x.GetRolesAsync(authUser)).ReturnsAsync(new List<string> { UserRoles.User });

        // Act
        var token1 = await _tokenService.GenerateAccessTokenAsync(authUser);
        var token2 = await _tokenService.GenerateAccessTokenAsync(authUser);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken1 = handler.ReadJwtToken(token1);
        var jwtToken2 = handler.ReadJwtToken(token2);

        var jti1 = jwtToken1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = jwtToken2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        jti1.Should().NotBe(jti2);
    }

    [Fact]
    public async Task GenerateAccessTokenAsync_ShouldUseHmacSha256Signature()
    {
        // Arrange
        var authUser = new AuthUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME
        };

        _mockUserManager.Setup(x => x.GetRolesAsync(authUser)).ReturnsAsync(new List<string> { UserRoles.User });

        // Act
        var token = await _tokenService.GenerateAccessTokenAsync(authUser);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.SignatureAlgorithm.Should().Be("HS256");
        jwtToken.Header.Alg.Should().Be("HS256");
    }

    #endregion

    #region GenerateRefreshTokenAsync Tests

    [Fact]
    public async Task GenerateRefreshTokenAsync_WithNewUser_ShouldCreateNewToken()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        _mockRefreshTokenRepository.Setup(x => x.GetActiveTokenByUserIdAsync(userId))
            .ReturnsAsync((RefreshToken?)null);

        _mockRefreshTokenRepository.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(userId);

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.Token.Should().NotBeNullOrEmpty();
        refreshToken.UserId.Should().Be(userId);
        refreshToken.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        refreshToken.ExpiresAt.Should().BeCloseTo(
            DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            TimeSpan.FromSeconds(2));
        refreshToken.RevokedAt.Should().BeNull();
        refreshToken.ReplacedByToken.Should().BeNull();
        refreshToken.ReasonRevoked.Should().BeNull();

        _mockRefreshTokenRepository.Verify(x => x.GetActiveTokenByUserIdAsync(userId), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.CreateAsync(It.Is<RefreshToken>(t =>
            t.UserId == userId &&
            !string.IsNullOrEmpty(t.Token)
        )), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);

        // Verify logger was called
        _mockLogger.Verify(x => x.LogInfo(
            It.Is<string>(s => s.Contains($"Refresh token generated for user {userId}")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()
        ), Times.Once);
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_WithExistingToken_ShouldUpdateToken()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var existingToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "old-token",
            UserId = userId,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            RevokedAt = null
        };

        _mockRefreshTokenRepository.Setup(x => x.GetActiveTokenByUserIdAsync(userId))
            .ReturnsAsync(existingToken);

        _mockRefreshTokenRepository.Setup(x => x.UpdateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(userId);

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.Id.Should().Be(existingToken.Id);
        refreshToken.Token.Should().NotBe("old-token");
        refreshToken.UserId.Should().Be(userId);
        refreshToken.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        refreshToken.ExpiresAt.Should().BeCloseTo(
            DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            TimeSpan.FromSeconds(2));
        refreshToken.RevokedAt.Should().BeNull();
        refreshToken.ReplacedByToken.Should().BeNull();
        refreshToken.ReasonRevoked.Should().BeNull();

        _mockRefreshTokenRepository.Verify(x => x.GetActiveTokenByUserIdAsync(userId), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.UpdateAsync(existingToken), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.CreateAsync(It.IsAny<RefreshToken>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);

        _mockLogger.Verify(x => x.LogInfo(
            It.Is<string>(s => s.Contains($"Refresh token updated for user {userId}")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()
        ), Times.Once);
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldGenerateSecureRandomToken()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        _mockRefreshTokenRepository.Setup(x => x.GetActiveTokenByUserIdAsync(userId))
            .ReturnsAsync((RefreshToken?)null);

        _mockRefreshTokenRepository.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        var token1 = await _tokenService.GenerateRefreshTokenAsync(userId);

        // Setup again for second call
        _mockRefreshTokenRepository.Setup(x => x.GetActiveTokenByUserIdAsync(userId))
            .ReturnsAsync((RefreshToken?)null);

        var token2 = await _tokenService.GenerateRefreshTokenAsync(userId);

        // Assert
        token1.Token.Should().NotBeNullOrEmpty();
        token2.Token.Should().NotBeNullOrEmpty();
        token1.Token.Should().NotBe(token2.Token);

        // Verify Base64 format
        var isValidBase64 = IsBase64String(token1.Token);
        isValidBase64.Should().BeTrue();
    }

    private static bool IsBase64String(string base64)
    {
        try
        {
            Convert.FromBase64String(base64);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region RefreshTokenAsync Tests

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var oldToken = "old-refresh-token";

        var existingRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = oldToken,
            UserId = userId,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            RevokedAt = null
        };

        var authUser = new AuthUser
        {
            Id = userId,
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME
        };

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(oldToken))
            .ReturnsAsync(existingRefreshToken);

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(authUser);

        _mockUserManager.Setup(x => x.GetRolesAsync(authUser))
            .ReturnsAsync(new List<string> { UserRoles.User });

        // Setup for revocation
        _mockRefreshTokenRepository.Setup(x => x.UpdateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        // Setup for new token generation
        _mockRefreshTokenRepository.Setup(x => x.GetActiveTokenByUserIdAsync(userId))
            .ReturnsAsync((RefreshToken?)null);

        _mockRefreshTokenRepository.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _tokenService.RefreshTokenAsync(oldToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Token refreshed successfully");
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe(oldToken);
        result.AccessTokenExpiration.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            TimeSpan.FromSeconds(5));

        _mockRefreshTokenRepository.Verify(x => x.GetByTokenAsync(oldToken), Times.AtLeast(2));
        _mockUserManager.Verify(x => x.FindByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithNullToken_ShouldThrowUnauthorizedException()
    {
        // Arrange
        string nullToken = null;

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(nullToken))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        Func<Task> act = async () => await _tokenService.RefreshTokenAsync(nullToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid or unactive refresh token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var invalidToken = "invalid-token";

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(invalidToken))
            .ReturnsAsync((RefreshToken)null);

        // Act
        Func<Task> act = async () => await _tokenService.RefreshTokenAsync(invalidToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid or unactive refresh token");

        _mockRefreshTokenRepository.Verify(x => x.GetByTokenAsync(invalidToken), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedToken_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var revokedToken = "revoked-token";
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = revokedToken,
            UserId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            RevokedAt = DateTime.UtcNow.AddHours(-1)
        };

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(revokedToken))
            .ReturnsAsync(refreshToken);

        // Act
        Func<Task> act = async () => await _tokenService.RefreshTokenAsync(revokedToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid or unactive refresh token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var expiredToken = "expired-token";
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = expiredToken,
            UserId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(-3),
            RevokedAt = null
        };

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(expiredToken))
            .ReturnsAsync(refreshToken);

        // Act
        Func<Task> act = async () => await _tokenService.RefreshTokenAsync(expiredToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid or unactive refresh token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithNonExistentUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var token = "valid-token";
        var userId = Guid.NewGuid().ToString();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            RevokedAt = null
        };

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(token))
            .ReturnsAsync(refreshToken);

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((AuthUser)null);

        // Act
        Func<Task> act = async () => await _tokenService.RefreshTokenAsync(token);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User not found");

        _mockUserManager.Verify(x => x.FindByIdAsync(userId), Times.Once);
    }

    #endregion

    #region RevokeTokenAsync Tests

    [Fact]
    public async Task RevokeTokenAsync_WithValidToken_ShouldRevokeSuccessfully()
    {
        // Arrange
        var token = "valid-token";
        var userId = Guid.NewGuid().ToString();
        var reason = "User logout";

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            RevokedAt = null
        };

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(token))
            .ReturnsAsync(refreshToken);

        _mockRefreshTokenRepository.Setup(x => x.UpdateAsync(refreshToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        await _tokenService.RevokeTokenAsync(token, reason);

        // Assert
        refreshToken.RevokedAt.Should().NotBeNull();
        refreshToken.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        refreshToken.ReasonRevoked.Should().Be(reason);

        _mockRefreshTokenRepository.Verify(x => x.GetByTokenAsync(token), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.UpdateAsync(refreshToken), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);

        _mockLogger.Verify(x => x.LogInfo(
            It.Is<string>(s => s.Contains($"Token revoked for user {userId}") && s.Contains(reason)),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()
        ), Times.Once);
    }

    [Fact]
    public async Task RevokeTokenAsync_WithNullToken_ShouldDoNothing()
    {
        // Arrange
        string nullToken = null;

        // Act
        await _tokenService.RevokeTokenAsync(nullToken);

        // Assert
        _mockRefreshTokenRepository.Verify(x => x.GetByTokenAsync(It.IsAny<string>()), Times.Never);
        _mockRefreshTokenRepository.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task RevokeTokenAsync_WithEmptyToken_ShouldDoNothing()
    {
        // Arrange
        var emptyToken = string.Empty;

        // Act
        await _tokenService.RevokeTokenAsync(emptyToken);

        // Assert
        _mockRefreshTokenRepository.Verify(x => x.GetByTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RevokeTokenAsync_WithNonExistentToken_ShouldDoNothing()
    {
        // Arrange
        var token = "non-existent-token";

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(token))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        await _tokenService.RevokeTokenAsync(token);

        // Assert
        _mockRefreshTokenRepository.Verify(x => x.GetByTokenAsync(token), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task RevokeTokenAsync_WithAlreadyRevokedToken_ShouldDoNothing()
    {
        // Arrange
        var token = "already-revoked-token";
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            RevokedAt = DateTime.UtcNow.AddDays(-1),
            ReasonRevoked = "Previous revocation"
        };

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(token))
            .ReturnsAsync(refreshToken);

        // Act
        await _tokenService.RevokeTokenAsync(token);

        // Assert
        _mockRefreshTokenRepository.Verify(x => x.GetByTokenAsync(token), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task RevokeTokenAsync_WithoutReason_ShouldUseDefaultReason()
    {
        // Arrange
        var token = "valid-token";
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            RevokedAt = null
        };

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(token))
            .ReturnsAsync(refreshToken);

        _mockRefreshTokenRepository.Setup(x => x.UpdateAsync(refreshToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        await _tokenService.RevokeTokenAsync(token);

        // Assert
        refreshToken.ReasonRevoked.Should().Be("Manually revoked");
    }

    #endregion

    #region RevokeUserTokensAsync Tests

    [Fact]
    public async Task RevokeUserTokensAsync_ShouldRevokeAllActiveTokens()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var activeTokens = new List<RefreshToken>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Token = "token1",
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                ExpiresAt = DateTime.UtcNow.AddDays(4),
                RevokedAt = null
            },
            new()
            {
                Id = Guid.NewGuid(),
                Token = "token2",
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ExpiresAt = DateTime.UtcNow.AddDays(5),
                RevokedAt = null
            },
            new()
            {
                Id = Guid.NewGuid(),
                Token = "token3",
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ExpiresAt = DateTime.UtcNow.AddDays(6),
                RevokedAt = null
            }
        };

        _mockRefreshTokenRepository.Setup(x => x.GetActiveTokensByUserIdAsync(userId))
            .ReturnsAsync(activeTokens);

        foreach (var token in activeTokens)
        {
            _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(token.Token))
                .ReturnsAsync(token);

            _mockRefreshTokenRepository.Setup(x => x.UpdateAsync(token))
                .Returns(Task.CompletedTask);
        }

        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        await _tokenService.RevokeUserTokensAsync(userId);

        // Assert
        _mockRefreshTokenRepository.Verify(x => x.GetActiveTokensByUserIdAsync(userId), Times.Once);

        foreach (var token in activeTokens)
        {
            _mockRefreshTokenRepository.Verify(x => x.GetByTokenAsync(token.Token), Times.Once);
            _mockRefreshTokenRepository.Verify(x => x.UpdateAsync(token), Times.Once);
        }

        _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Exactly(activeTokens.Count));
    }

    [Fact]
    public async Task RevokeUserTokensAsync_WithNoActiveTokens_ShouldDoNothing()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var emptyTokenList = new List<RefreshToken>();

        _mockRefreshTokenRepository.Setup(x => x.GetActiveTokensByUserIdAsync(userId))
            .ReturnsAsync(emptyTokenList);

        // Act
        await _tokenService.RevokeUserTokensAsync(userId);

        // Assert
        _mockRefreshTokenRepository.Verify(x => x.GetActiveTokensByUserIdAsync(userId), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Never);
    }

    #endregion

    #region CleanupExpiredTokensAsync Tests

    [Fact]
    public async Task CleanupExpiredTokensAsync_ShouldDeleteExpiredTokens()
    {
        // Arrange
        var expiredTokens = new List<RefreshToken>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Token = "expired-token-1",
                UserId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                ExpiresAt = DateTime.UtcNow.AddDays(-3),
                RevokedAt = null
            },
            new()
            {
                Id = Guid.NewGuid(),
                Token = "expired-token-2",
                UserId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                ExpiresAt = DateTime.UtcNow.AddDays(-8),
                RevokedAt = null
            }
        };

        _mockRefreshTokenRepository.Setup(x => x.GetExpiredTokensAsync())
            .ReturnsAsync(expiredTokens);

        foreach (var token in expiredTokens)
            _mockRefreshTokenRepository.Setup(x => x.DeleteAsync(token.Id))
                .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        await _tokenService.CleanupExpiredTokensAsync();

        // Assert
        _mockRefreshTokenRepository.Verify(x => x.GetExpiredTokensAsync(), Times.Once);

        foreach (var token in expiredTokens)
            _mockRefreshTokenRepository.Verify(x => x.DeleteAsync(token.Id), Times.Once);

        _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);

        _mockLogger.Verify(x => x.LogInfo(
            It.Is<string>(s => s.Contains($"Cleaned up {expiredTokens.Count} expired tokens")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()
        ), Times.Once);
    }

    [Fact]
    public async Task CleanupExpiredTokensAsync_WithNoExpiredTokens_ShouldLogZero()
    {
        // Arrange
        var emptyList = new List<RefreshToken>();

        _mockRefreshTokenRepository.Setup(x => x.GetExpiredTokensAsync())
            .ReturnsAsync(emptyList);

        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        await _tokenService.CleanupExpiredTokensAsync();

        // Assert
        _mockRefreshTokenRepository.Verify(x => x.GetExpiredTokensAsync(), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);

        _mockLogger.Verify(x => x.LogInfo(
            It.Is<string>(s => s.Contains("Cleaned up 0 expired tokens")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()
        ), Times.Once);
    }

    #endregion

    #region Integration and Edge Case Tests

    [Fact]
    public async Task TokenService_CompleteWorkflow_ShouldWorkEndToEnd()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var authUser = new AuthUser
        {
            Id = userId,
            Email = TestDataBuilder.TestConstants.DEFAULT_EMAIL,
            Name = TestDataBuilder.TestConstants.DEFAULT_USERNAME
        };

        _mockUserManager.Setup(x => x.GetRolesAsync(authUser))
            .ReturnsAsync(new List<string> { UserRoles.User });

        _mockRefreshTokenRepository.Setup(x => x.GetActiveTokenByUserIdAsync(userId))
            .ReturnsAsync((RefreshToken?)null);

        _mockRefreshTokenRepository.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        // Act - Generate tokens
        var accessToken = await _tokenService.GenerateAccessTokenAsync(authUser);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(userId);

        // Assert - Tokens created
        accessToken.Should().NotBeNullOrEmpty();
        refreshToken.Should().NotBeNull();
        refreshToken.Token.Should().NotBeNullOrEmpty();

        // Setup for refresh
        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(refreshToken.Token))
            .ReturnsAsync(refreshToken);

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(authUser);

        _mockRefreshTokenRepository.Setup(x => x.UpdateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        // Act - Refresh tokens
        var refreshedTokens = await _tokenService.RefreshTokenAsync(refreshToken.Token);

        // Assert - New tokens received
        refreshedTokens.Should().NotBeNull();
        refreshedTokens.IsSuccess.Should().BeTrue();
        refreshedTokens.AccessToken.Should().NotBe(accessToken);
        refreshedTokens.RefreshToken.Should().NotBe(refreshToken.Token);

        // Setup for revocation
        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshedTokens.RefreshToken,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = null
        };

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(refreshedTokens.RefreshToken))
            .ReturnsAsync(newRefreshToken);

        // Act - Revoke token
        await _tokenService.RevokeTokenAsync(refreshedTokens.RefreshToken, "User logout");

        // Assert - Token revoked
        newRefreshToken.RevokedAt.Should().NotBeNull();
        newRefreshToken.ReasonRevoked.Should().Be("User logout");
    }

    //[Fact]
    //public async Task GenerateAccessTokenAsync_WithSpecialCharactersInName_ShouldEncodeCorrectly()
    //{
    //    // Arrange
    //    var authUser = new AuthUser
    //    {
    //        Id = Guid.NewGuid().ToString(),
    //        Email = "test+user@example.com",
    //        Name = "Test O'User & <Special>"
    //    };

    //    _mockUserManager.Setup(x => x.GetRolesAsync(authUser))
    //        .ReturnsAsync(new List<string> { UserRoles.User });

    //    // Act
    //    var token = await _tokenService.GenerateAccessTokenAsync(authUser);

    //    // Assert
    //    token.Should().NotBeNullOrEmpty();

    //    var handler = new JwtSecurityTokenHandler();
    //    var jwtToken = handler.ReadJwtToken(token);
    //    jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == authUser.Email);
    //    jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == authUser.Name);
    //}

    [Fact]
    public async Task TokenService_ConcurrentRefreshTokenGeneration_ShouldHandleCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        _mockRefreshTokenRepository.Setup(x => x.GetActiveTokenByUserIdAsync(userId))
            .ReturnsAsync((RefreshToken?)null);

        _mockRefreshTokenRepository.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        // Act - Simulate concurrent calls
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => _tokenService.GenerateRefreshTokenAsync(userId))
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(5);
        results.Should().OnlyContain(r => r != null && !string.IsNullOrEmpty(r.Token));

        // All tokens should be unique
        var uniqueTokens = results.Select(r => r.Token).Distinct().ToList();
        uniqueTokens.Should().HaveCount(5);
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_TokenLength_ShouldBeConsistent()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        _mockRefreshTokenRepository.Setup(x => x.GetActiveTokenByUserIdAsync(userId))
            .ReturnsAsync((RefreshToken?)null);

        _mockRefreshTokenRepository.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        var token1 = await _tokenService.GenerateRefreshTokenAsync(userId);

        _mockRefreshTokenRepository.Setup(x => x.GetActiveTokenByUserIdAsync(userId))
            .ReturnsAsync((RefreshToken?)null);

        var token2 = await _tokenService.GenerateRefreshTokenAsync(userId);

        // Assert - Base64 encoded 64 bytes should give us ~88 characters
        token1.Token.Length.Should().BeGreaterThan(80);
        token2.Token.Length.Should().BeGreaterThan(80);
        token1.Token.Length.Should().Be(token2.Token.Length);
    }

    #endregion
}