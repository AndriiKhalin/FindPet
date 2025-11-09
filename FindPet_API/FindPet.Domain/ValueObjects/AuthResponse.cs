namespace FindPet.Domain.ValueObjects;

public class AuthResponse
{
    public string? AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiration { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
}