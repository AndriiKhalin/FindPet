using Microsoft.AspNetCore.Http;

namespace FindPet.Media.Interfaces;

public interface IMediaStorageService
{
    Task<string> UploadFileAsync(IFormFile file, Guid? entityId = null, string? subfolder = null);

    Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, Guid? entityId = null, string? subfolder = null);

    Task<Stream> DownloadFileAsync(string filePath);

    Task<bool> DeleteFileAsync(string filePath);

    Task<Stream> GetFileAsync(string filePath);

    Task<string> GetFileUrlAsync(string filePath, TimeSpan? expiryTime = null);

    Task<bool> FileExistsAsync(string filePath);
}