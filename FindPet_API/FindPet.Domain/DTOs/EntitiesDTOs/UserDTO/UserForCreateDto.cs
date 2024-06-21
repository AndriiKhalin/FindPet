using Microsoft.AspNetCore.Http;

namespace FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;

public class UserForCreateDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public DateTime? BirthDate { get; set; }
    public IFormFile? Photo { get; set; }
}