using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.Entities;
using FindPet.Domain.Exceptions;
using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace FindPet.BusinessLogicLayer.Services.AuthService;

public class TokenService(
    UserManager<AuthUser> userManager,
    JwtSettings jwtSettings,
    IUnitOfWork unitOfWork,
    ILoggerManager logger) : ITokenService
{
    public async Task<string> GenerateAccessTokenAsync(AuthUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);
        var roles = await userManager.GetRolesAsync(user);

        var expirationTime = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes);
        var expirationUnixTime = new DateTimeOffset(expirationTime).ToUnixTimeSeconds();

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Name, user.Name ?? ""),
            new(JwtRegisteredClaimNames.NameId, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Aud, jwtSettings.ValidAudience),
            new(JwtRegisteredClaimNames.Iss, jwtSettings.ValidIssuer),
            new(JwtRegisteredClaimNames.Exp, expirationUnixTime.ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        ];

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expirationTime,
            Audience = jwtSettings.ValidAudience,
            Issuer = jwtSettings.ValidIssuer,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId)
    {
        var existingToken = await unitOfWork.RefreshToken.GetActiveTokenByUserIdAsync(userId);

        if (existingToken != null)
        {
            // Update existing token with new values
            existingToken.Token = GenerateSecureToken();
            existingToken.ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationDays);
            existingToken.CreatedAt = DateTime.UtcNow;
            existingToken.RevokedAt = null;
            existingToken.ReplacedByToken = null;
            existingToken.ReasonRevoked = null;

            await unitOfWork.RefreshToken.UpdateAsync(existingToken);
            await unitOfWork.SaveAsync();

            logger.LogInfo($"Refresh token updated for user {userId}");
            return existingToken;
        }


        var refreshToken = new RefreshToken
        {
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        // Store in database
        await unitOfWork.RefreshToken.CreateAsync(refreshToken);
        await unitOfWork.SaveAsync();

        logger.LogInfo($"Refresh token generated for user {userId}");
        return refreshToken;
    }

    public async Task<AuthResponse> RefreshTokenAsync(string token)
    {
        var refreshToken = await unitOfWork.RefreshToken
            .GetByTokenAsync(token);

        if (refreshToken == null || !refreshToken.IsActive)
            throw new UnauthorizedException("Invalid or unactive refresh token");

        // Get user
        var user = await userManager.FindByIdAsync(refreshToken.UserId);
        if (user == null)
            throw new UnauthorizedException("User not found");

        // Revoke old refresh token
        await RevokeTokenAsync(token);

        // Generate new tokens
        var newAccessToken = await GenerateAccessTokenAsync(user);
        var newRefreshToken = await GenerateRefreshTokenAsync(user.Id);

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes),
            RefreshTokenExpiration = newRefreshToken.ExpiresAt,
            IsSuccess = true,
            Message = "Token refreshed successfully"
        };
    }

    public async Task RevokeTokenAsync(string token, string? reason = null)
    {
        if (string.IsNullOrEmpty(token))
            return;

        var refreshToken = await unitOfWork.RefreshToken.GetByTokenAsync(token);

        if (refreshToken != null && refreshToken.IsActive)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.ReasonRevoked = reason ?? "Manually revoked";

            await unitOfWork.RefreshToken.UpdateAsync(refreshToken);
            await unitOfWork.SaveAsync();

            logger.LogInfo($"Token revoked for user {refreshToken.UserId}. Reason: {reason}");
        }
    }

    public async Task RevokeUserTokensAsync(string userId)
    {
        var userTokens = await unitOfWork.RefreshToken
            .GetActiveTokensByUserIdAsync(userId);

        foreach (var token in userTokens) await RevokeTokenAsync(token.Token);
    }

    public async Task CleanupExpiredTokensAsync()
    {
        var expiredTokens = await unitOfWork.RefreshToken
            .GetExpiredTokensAsync();

        foreach (var token in expiredTokens) await unitOfWork.RefreshToken.DeleteAsync(token.Id);

        await unitOfWork.SaveAsync();
        logger.LogInfo($"Cleaned up {expiredTokens.Count()} expired tokens");
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}