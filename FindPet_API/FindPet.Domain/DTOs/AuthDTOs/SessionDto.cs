namespace FindPet.Domain.DTOs.AuthDTOs;

/// <summary>
///     DTO for user session information
/// </summary>
public class SessionDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsCurrent { get; set; }
}