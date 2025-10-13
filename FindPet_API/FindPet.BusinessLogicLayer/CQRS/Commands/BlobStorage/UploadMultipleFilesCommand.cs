using FindPet.BusinessLogicLayer.CQRS.Common;
using Microsoft.AspNetCore.Http;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.BlobStorage;

public record UploadMultipleFilesCommand(List<IFormFile> Files, string? Subfolder = null, Guid? EntityId = null) : ICommand<UploadMultipleFilesResponse>;

public class UploadMultipleFilesResponse
{
    public List<string> FilePaths { get; set; } = new();
    public List<string> FileSecureUrls { get; set; } = new();
    public int SuccessfulUploads { get; set; }
    public int FailedUploads { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
}