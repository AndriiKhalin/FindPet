namespace FindPet.Domain.ValueObjects;

public class AuthResponse
{
    public string? Token { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
}