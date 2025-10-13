using System.ComponentModel.DataAnnotations;

namespace FindPet.Domain.DTOs.FileDTOs;

public class FileUrlDto
{
    [Required]
    public string FilePath { get; set; } = string.Empty;

    public int ExpiryHours { get; set; } = 24;
}