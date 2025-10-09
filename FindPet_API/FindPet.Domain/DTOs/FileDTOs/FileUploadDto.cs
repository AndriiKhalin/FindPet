using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FindPet.Domain.DTOs.FileDTOs;

public class FileUploadDto
{
    [Required]
    public IFormFile File { get; set; }

    public string? Subfolder { get; set; }

    public Guid? EntityId { get; set; }
}