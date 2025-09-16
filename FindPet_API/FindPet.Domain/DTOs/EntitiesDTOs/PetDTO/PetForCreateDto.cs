using Microsoft.AspNetCore.Http;

namespace FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;

public class PetForCreateDto
{
    //public string? Type { get; set; }
    public string? Breed { get; set; }
    public string? Nickname { get; set; }
    public string? Gender { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? SpecialMarks { get; set; }
    public string? Photo { get; set; }
    public string? Description { get; set; }
    public DateTime? LostDate { get; set; }
}