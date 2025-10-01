using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.Image;

public record UploadImageCommand(IFormFile ImageFile, EntityType EntityType) : ICommand<UploadImageResponse>;

public class UploadImageResponse
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}