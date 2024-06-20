using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FindPet.Domain.DTOs.AuthDTOs;

public class RegisterDto
{
    [Required]
    public string? Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    //[Required]
    //public DateTime? BirthDate { get; set; }

    //[Required]
    //public string? PhoneNumber { get; set; }

    //[Required]
    //public IFormFile? Photo { get; set; }

    public string? Role { get; set; }

    //[Required]
    //public string UserName { get; set; }

    //[EmailAddress]
    //[Required(ErrorMessage = "Email is required")]
    //public string Email { get; set; }

    //public string Password { get; set; } = string.Empty;
}