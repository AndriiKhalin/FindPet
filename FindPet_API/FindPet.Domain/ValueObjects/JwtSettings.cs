namespace FindPet.Domain.ValueObjects;

public record JwtSettings(
    string ValidAudience,
    string ValidIssuer,
    string Secret,
    int AccessTokenExpirationMinutes,
    int RefreshTokenExpirationDays);