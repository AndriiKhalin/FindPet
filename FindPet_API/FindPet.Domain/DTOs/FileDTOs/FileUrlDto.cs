using System.ComponentModel.DataAnnotations;

namespace FindPet.Domain.DTOs.FileDTOs;

public class FileUrlDto
{
    [Required]
    public string FileName { get; set; } = string.Empty;

    public string? Subfolder { get; set; }
}