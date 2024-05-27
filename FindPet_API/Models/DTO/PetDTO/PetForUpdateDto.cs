using Microsoft.AspNetCore.Http;

namespace Models.DTO.PetDTO;

public class PetForUpdateDto
{
    public string? Type { get; set; }
    public string? Breed { get; set; }
    public string? Nickname { get; set; }
    public string? Gender { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? SpecialMarks { get; set; }
    public IFormFile? Photo { get; set; }
    public DateTime? LostDate { get; set; }
    public string? LostLocation { get; set; }
    public DateTime? FoundDate { get; set; }
    public string? FoundLocation { get; set; }
    public string? Status { get; set; }
}