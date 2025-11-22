using System.ComponentModel.DataAnnotations;

namespace FindPet.Domain.DTOs.AuthDTOs;

/// <summary>
///     DTO for requesting password reset
/// </summary>
public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;
}