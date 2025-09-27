using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FindPet.Domain.DTOs;

public class FileUploadDto
{
    [Required] public IFormFile ImageFile { get; set; }
}