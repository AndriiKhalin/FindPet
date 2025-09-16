namespace FindPet.Domain.DTOs.AuthDTOs;

public class UserDetailDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Photo { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Role { get; set; }
    public string? PhoneNumber { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public int AccessFailedCount { get; set; }
}