using FindPet.BusinessLogicLayer.Interfaces.IAuthService;
using FindPet.Domain.Entities;
using FindPet.Domain.Exceptions;
using FindPet.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.Interfaces.ILoggerService;
using TokenValidationResult = FindPet.Domain.ValueObjects.TokenValidationResult;

namespace FindPet.BusinessLogicLayer.Services.AuthService;

public class TokenService(UserManager<AuthUser> userManager, JwtSettings jwtSettings, IUnitOfWork unitOfWork, ILoggerManager logger) : ITokenService
{
    public async Task<string> GenerateAccessTokenAsync(AuthUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);
        var roles = await userManager.GetRolesAsync(user);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Name, user.Name ?? ""),
            new(JwtRegisteredClaimNames.NameId, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Aud, jwtSettings.ValidAudience),
            new(JwtRegisteredClaimNames.Iss, jwtSettings.ValidIssuer)
        ];

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes),
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

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string ipAddress)
    {
        var refreshToken = new RefreshToken
        {
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            UserId = userId
        };

        // Store in database
        await unitOfWork.RefreshToken.CreateAsync(refreshToken);
        await unitOfWork.SaveAsync();

        logger.LogInfo($"Refresh token generated for user {userId}");
        return refreshToken;
    }

    public async Task<AuthResponse> RefreshTokenAsync(string token, string ipAddress)
    {
        //var refreshToken = await unitOfWork.RefreshToken
        //    .GetByConditionAsync(rt => rt.Token == token)
        //    .Result
        //    .FirstOrDefaultAsync();

        //if (refreshToken == null || !refreshToken.IsActive)
        //{
        //    logger.LogWarn($"Invalid refresh token attempt from IP: {ipAddress}");
        //    throw new UnauthorizedException("Invalid or expired refresh token");
        //}

        //// Get user
        //var user = await userManager.FindByIdAsync(refreshToken.UserId);
        //if (user == null)
        //    throw new UnauthorizedException("User not found");

        //// Revoke old refresh token
        //await RevokeTokenAsync(token, ipAddress, "Replaced by new token");

        //// Generate new tokens
        //var newAccessToken = await GenerateAccessTokenAsync(user);
        //var newRefreshToken = await GenerateRefreshTokenAsync(user.Id, ipAddress);

        //return new AuthResponse
        //{
        //    AccessToken = newAccessToken,
        //    RefreshToken = newRefreshToken.Token,
        //    AccessTokenExpiration = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes),
        //    RefreshTokenExpiration = newRefreshToken.ExpiresAt,
        //    IsSuccess = true,
        //    Message = "Token refreshed successfully"
        //};
        throw new NotImplementedException();
    }

    public async Task<bool> RevokeTokenAsync(string token, string ipAddress, string? reason = null)
    {
        //var refreshToken = await unitOfWork.RefreshToken
        //    .GetByConditionAsync(rt => rt.Token == token)
        //    .Result
        //    .FirstOrDefaultAsync();

        //if (refreshToken == null || !refreshToken.IsActive)
        //    return false;

        //refreshToken.RevokedAt = DateTime.UtcNow;
        //refreshToken.RevokedByIp = ipAddress;
        //refreshToken.ReasonRevoked = reason ?? "Revoked without reason";

        //await unitOfWork.RefreshToken.UpdateAsync(refreshToken);
        //await unitOfWork.SaveAsync();

        //logger.LogInfo($"Token revoked for user {refreshToken.UserId}");
        //return true;
        throw new NotImplementedException();
    }

    public async Task RevokeUserTokensAsync(string userId, string ipAddress)
    {
        //var userTokens = await unitOfWork.RefreshToken
        //    .GetByConditionAsync(rt => rt.UserId == userId && rt.IsActive)
        //    .Result
        //    .ToListAsync();

        //foreach (var token in userTokens)
        //{
        //    await RevokeTokenAsync(token.Token, ipAddress, "User logout");
        //}
        throw new NotImplementedException();
    }

    public async Task CleanupExpiredTokensAsync()
    {
        //var expiredTokens = await unitOfWork.RefreshToken
        //    .GetByConditionAsync(rt => rt.ExpiresAt < DateTime.UtcNow)
        //    .Result
        //    .ToListAsync();

        //foreach (var token in expiredTokens)
        //{
        //    await unitOfWork.RefreshToken.DeleteAsync(token.Id);
        //}

        //await unitOfWork.SaveAsync();
        //logger.LogInfo($"Cleaned up {expiredTokens.Count} expired tokens");
        throw new NotImplementedException();
    }

    public async Task<TokenValidationResult> ValidateTokenAsync(string token)
    {
        //var tokenHandler = new JwtSecurityTokenHandler();
        //var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

        //try
        //{
        //    var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        //    {
        //        ValidateIssuerSigningKey = true,
        //        IssuerSigningKey = new SymmetricSecurityKey(key),
        //        ValidateIssuer = true,
        //        ValidIssuer = jwtSettings.ValidIssuer,
        //        ValidateAudience = true,
        //        ValidAudience = jwtSettings.ValidAudience,
        //        ValidateLifetime = true,
        //        ClockSkew = TimeSpan.Zero
        //    }, out var validatedToken);

        //    var jwtToken = (JwtSecurityToken)validatedToken;
        //    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        //    var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

        //    return new TokenValidationResult
        //    {
        //        IsValid = true,
        //        UserId = userId,
        //        Email = email,
        //        Roles = roles
        //    };
        //}
        //catch (Exception ex)
        //{
        //    return new TokenValidationResult
        //    {
        //        IsValid = false,
        //        ErrorMessage = ex.Message
        //    };
        //}
        throw new NotImplementedException();
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}