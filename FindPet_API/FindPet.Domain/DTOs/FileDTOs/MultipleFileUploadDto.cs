using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FindPet.Domain.DTOs.FileDTOs;

public class MultipleFileUploadDto
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one file is required")]
    public List<IFormFile> Files { get; set; } = new();

    public string? Subfolder { get; set; }

    public Guid? EntityId { get; set; }
}