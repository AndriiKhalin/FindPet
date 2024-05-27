using Microsoft.AspNetCore.Http;

namespace FindPet.Domain.DTOs.FinderDTO;

public class FinderForUpdateDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public IFormFile? Photo { get; set; }
}