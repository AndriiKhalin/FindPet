using FindPet.BusinessLogicLayer.CQRS.Common;
using Microsoft.AspNetCore.Http;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.BlobStorage;

public record UploadFileCommand(IFormFile File, string? Subfolder = null, Guid? EntityId = null) : ICommand<UploadFileResponse>;

public class UploadFileResponse
{
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
}