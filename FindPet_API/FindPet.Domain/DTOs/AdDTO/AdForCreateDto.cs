using Microsoft.AspNetCore.Http;

namespace FindPet.Domain.DTOs.AdDTO;

public class AdForCreateDto
{
    public string? Description { get; set; }
    public string? Location { get; set; }
    public IFormFile? Photo { get; set; }
}