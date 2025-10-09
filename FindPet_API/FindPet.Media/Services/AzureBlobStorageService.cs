using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FindPet.Domain.ValueObjects;
using FindPet.Media.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Text.RegularExpressions;
using Size = System.Drawing.Size;

namespace FindPet.Media.Services;

public class AzureBlobStorageService : IMediaStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;
    private readonly string _connectionString;
    private readonly string _containerName;
    private readonly AzureBlobStorageOptions _options;
    //private readonly Logger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(string connectionString, IOptions<AzureBlobStorageOptions> options, string containerName)
    {
        _options = options.Value;
        _connectionString = connectionString;
        _containerName = containerName;
        _blobServiceClient = new BlobServiceClient(_connectionString);
        _containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        //try
        //{
        //    var blobClient = new BlobClient(new Uri(imageUrl));
        //    var response = await blobClient.DeleteIfExistsAsync();
        //    return response.Value;
        //}
        //catch
        //{
        //    return false;
        //}

        try
        {
            var blobName = ExtractBlobNameFromUrl(imageUrl);
            var blobClient = _containerClient.GetBlobClient(blobName);

            var response = await blobClient.DeleteIfExistsAsync();

            //if (response.Value)
            //{
            //    _logger.LogInformation("Successfully deleted image: {BlobName}", blobName);
            //}

            return response.Value;
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Failed to delete image: {ImageUrl}", imageUrl);
            return false;
        }
    }

    public Task<Stream> DownloadFileAsync(string imageUrl)
    {
        throw new NotImplementedException();
    }

    public async Task<Stream> GetImageAsync(string imageUrl)
    {
        //try
        //{
        //    var blobClient = new BlobClient(new Uri(imageUrl));
        //    var response = await blobClient.DownloadAsync();
        //    return response.Value.Content;
        //}
        //catch
        //{
        //    return Stream.Null;
        //}

        try
        {
            var blobName = ExtractBlobNameFromUrl(imageUrl);
            var blobClient = _containerClient.GetBlobClient(blobName);

            var response = await blobClient.DownloadStreamingAsync();
            return response.Value.Content;
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Failed to get image: {ImageUrl}", imageUrl);
            return Stream.Null;
        }
    }

    public async Task<string> GetImageUrlAsync(string fileName, string? subfolder = null)
    {
        var blobName = GenerateBlobName(fileName, subfolder);
        var blobClient = _containerClient.GetBlobClient(blobName);

        return blobClient.Uri.ToString();
    }

    public async Task<bool> ImageExistsAsync(string imageUrl)
    {
        try
        {
            var blobName = ExtractBlobNameFromUrl(imageUrl);
            var blobClient = _containerClient.GetBlobClient(blobName);

            var response = await blobClient.ExistsAsync();
            return response.Value;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string? subfolder = null)
    {
        try
        {
            await EnsureContainerExistsAsync();

            var blobName = GenerateBlobName(fileName, subfolder);

            var blobClient = _containerClient.GetBlobClient(blobName);

            // Optimize image before upload
            using var optimizedStream = await OptimizeImageAsync(imageStream);

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = GetContentType(fileName),
                    CacheControl = "public, max-age=31536000" // 1 year cache
                },
                Metadata = new Dictionary<string, string>
                {
                    ["OriginalFileName"] = fileName,
                    ["UploadedAt"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    ["Subfolder"] = subfolder ?? "general"
                }
            };

            await blobClient.UploadAsync(optimizedStream, uploadOptions);

            //_logger.LogInformation("Successfully uploaded image: {BlobName}", blobName);
            return blobClient.Uri.ToString();


        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        //var blobServiceClient = new BlobServiceClient(_connectionString);
        //var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        //await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        //var blobClient = containerClient.GetBlobClient(fileName);
        //await blobClient.UploadAsync(imageStream, true);
        //return blobClient.Uri.ToString();
    }

    public async Task<string> UploadImageAsync(IFormFile file, Guid? entityId = null, string? subfolder = null)
    {
        ValidateFile(file);

        var fileName = GenerateUniqueFileName(file.FileName, entityId);

        using var stream = file.OpenReadStream();
        return await UploadImageAsync(stream, fileName, subfolder);
    }

    public async Task<List<string>> UploadMultipleImagesAsync(List<IFormFile> files, Guid? entityId = null, string? subfolder = null)
    {
        var uploadTasks = files.Select(file => UploadImageAsync(file, entityId, subfolder));
        return (await Task.WhenAll(uploadTasks)).ToList();
    }

    private async Task EnsureContainerExistsAsync()
    {
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
    }

    private string GenerateBlobName(string fileName, string? subfolder)
    {
        var folder = string.IsNullOrEmpty(subfolder) ? "general" : subfolder;
        return $"{folder}/{fileName}";
    }

    private async Task<MemoryStream> OptimizeImageAsync(Stream imageStream)
    {
        using var image = await Image.LoadAsync(imageStream);

        // Resize if too large
        if (image.Width > 1920 || image.Height > 1920)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new SixLabors.ImageSharp.Size(1920, 1920)
            }));
        }

        var outputStream = new MemoryStream();
        await image.SaveAsJpegAsync(outputStream, new JpegEncoder
        {
            Quality = _options.ImageQuality
        });

        outputStream.Position = 0;
        return outputStream;
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
    }

    private void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is null or empty");

        if (file.Length > _options.MaxFileSizeBytes)
            throw new ArgumentException($"File size exceeds maximum allowed size of {_options.MaxFileSizeBytes} bytes");

        if (!_options.AllowedImageTypes.Contains(file.ContentType))
            throw new ArgumentException($"File type {file.ContentType} is not allowed");
    }

    private string GenerateUniqueFileName(string originalFileName, Guid? entityId)
    {
        var extension = Path.GetExtension(originalFileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var sanitizedName = SanitizeFileName(nameWithoutExtension);

        var uniqueId = entityId?.ToString("N") ?? Guid.NewGuid().ToString("N"); // Use "N" format to remove hyphens
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        return $"{sanitizedName}_{uniqueId}_{timestamp}{extension}";
    }

    private string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return fileName;

        // Replace problematic characters that cause URL encoding issues
        var sanitized = fileName
            .Replace("(", "_")
            .Replace(")", "_")
            .Replace("%", "_")
            .Replace(" ", "_")
            .Replace("#", "_")
            .Replace("&", "_")
            .Replace("?", "_")
            .Replace("+", "_");

        // Remove any other potentially problematic characters
        sanitized = Regex.Replace(sanitized, @"[^\w\-_.]+", "_");

        // Remove multiple consecutive underscores
        sanitized = Regex.Replace(sanitized, @"_{2,}", "_");

        // Remove leading/trailing underscores
        sanitized = sanitized.Trim('_');

        return sanitized;
    }

    private string ExtractBlobNameFromUrl(string imageUrl)
    {
        var uri = new Uri(imageUrl);
        return uri.Segments.Skip(2).Aggregate(string.Empty, (current, segment) => current + segment);
    }
}