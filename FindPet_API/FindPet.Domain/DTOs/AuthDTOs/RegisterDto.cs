using System.ComponentModel.DataAnnotations;

namespace FindPet.Domain.DTOs.AuthDTOs;

public class RegisterDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public DateTime? BirthDate { get; set; }

    [Required]
    public string? PhoneNumber { get; set; }

    public List<string>? Roles { get; set; }

    //[Required]
    //public string UserName { get; set; }

    //[EmailAddress]
    //[Required(ErrorMessage = "Email is required")]
    //public string Email { get; set; }

    //public string Password { get; set; } = string.Empty;
}