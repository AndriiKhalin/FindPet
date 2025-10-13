using FindPet.BusinessLogicLayer.CQRS.Common;
using Microsoft.AspNetCore.Http;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.BlobStorage;

public record UploadFileCommand(IFormFile File, string? Subfolder = null, Guid? EntityId = null) : ICommand<UploadFileResponse>;

public class UploadFileResponse
{
    public string FilePath { get; set; } = string.Empty; // Store this in DB
    public string FileSecureUrl { get; set; } = string.Empty; // Use this for immediate display
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; } // When the secure URL expires
}