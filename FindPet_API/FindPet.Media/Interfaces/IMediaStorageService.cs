using Microsoft.AspNetCore.Http;

namespace FindPet.Media.Interfaces;

public interface IMediaStorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName, string? subfolder = null);

    Task<string> UploadImageAsync(IFormFile file, Guid? entityId = null, string? subfolder = null);

    Task<List<string>> UploadMultipleImagesAsync(List<IFormFile> files, Guid? entityId = null, string? subfolder = null);

    Task<Stream> DownloadFileAsync(string imageUrl);

    Task<bool> DeleteImageAsync(string imageUrl);

    Task<Stream> GetImageAsync(string imageUrl);

    Task<string> GetImageUrlAsync(string fileName, string? subfolder = null);

    Task<bool> ImageExistsAsync(string imageUrl);
}