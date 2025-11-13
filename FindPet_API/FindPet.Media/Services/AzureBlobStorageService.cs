using System.Text.RegularExpressions;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Domain.ValueObjects;
using FindPet.Media.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace FindPet.Media.Services;

public class AzureBlobStorageService : IMediaStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _connectionString;
    private readonly BlobContainerClient _containerClient;
    private readonly string _containerName;
    private readonly ILoggerManager _logger;
    private readonly AzureBlobStorageOptions _options;

    public AzureBlobStorageService(
        string connectionString,
        IOptions<AzureBlobStorageOptions> options,
        string containerName,
        ILoggerManager logger)
    {
        _options = options.Value;
        _connectionString = connectionString;
        _containerName = containerName;
        _blobServiceClient = new BlobServiceClient(_connectionString);
        _containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        _logger = logger;
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var blobName = ExtractBlobNameFromUrl(filePath);
            var blobClient = _containerClient.GetBlobClient(blobName);

            var response = await blobClient.DeleteIfExistsAsync();

            if (response.Value) _logger.LogInfo("Successfully deleted image: {FilePath}", blobName);

            return response.Value;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError($"Azure request failed while deleting file: {filePath}. Error: {ex.Message}");
            return false;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError($"Invalid argument for file deletion: {filePath}. Error: {ex.Message}");
            return false;
        }
    }

    public async Task<Stream> DownloadFileAsync(string filePath)
    {
        try
        {
            var blobName = ExtractBlobNameFromUrl(filePath);
            var blobClient = _containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync()) throw new FileNotFoundException($"File {filePath} not found");

            var response = await blobClient.DownloadStreamingAsync();
            return response.Value.Content;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarn($"{ex.Message}. Returning null.");
            return Stream.Null;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError($"Azure request failed while downloading file: {filePath}. Error: {ex.Message}");
            throw new InvalidOperationException($"Failed to download file: {filePath}", ex);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError($"Invalid argument for file download: {filePath}. Error: {ex.Message}");
            throw new InvalidOperationException($"Failed to download file: {filePath}", ex);
        }
    }

    public async Task<Stream> GetFileAsync(string filePath)
    {
        try
        {
            var blobName = ExtractBlobNameFromUrl(filePath);
            var blobClient = _containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync()) return Stream.Null;

            var response = await blobClient.DownloadStreamingAsync();
            return response.Value.Content;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError($"Azure request failed while getting file: {filePath}. Error: {ex.Message}");
            return Stream.Null;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError($"Invalid argument for file retrieval: {filePath}. Error: {ex.Message}");
            return Stream.Null;
        }
    }

    public async Task<string?> GetFileUrlAsync(string filePath, TimeSpan? expiryTime = null)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(filePath);

            // Check if the blob exists
            if (!await blobClient.ExistsAsync()) throw new FileNotFoundException($"File {filePath} not found");

            // Generate SAS token for secure access
            if (blobClient.CanGenerateSasUri)
            {
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _containerName,
                    BlobName = filePath,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.Add(expiryTime ?? TimeSpan.FromHours(1))
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                return blobClient.GenerateSasUri(sasBuilder).ToString();
            }

            // Fallback to regular URL if SAS is not available
            return blobClient.Uri.ToString();
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarn($"{ex.Message}. Returning null.");
            return null;
        }
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        try
        {
            var blobName = ExtractBlobNameFromUrl(filePath);
            var blobClient = _containerClient.GetBlobClient(blobName);

            var response = await blobClient.ExistsAsync();
            return response.Value;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError($"Azure request failed while checking file existence: {filePath}. Error: {ex.Message}");
            return false;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError($"Invalid argument for file existence check: {filePath}. Error: {ex.Message}");
            return false;
        }
    }

    public async Task<string> UploadFileAsync(IFormFile file, Guid? entityId = null, string? subfolder = null)
    {
        try
        {
            ValidateFile(file);

            var fileName = GenerateUniqueFileName(file.FileName, entityId);
            var blobName = GenerateBlobName(fileName, subfolder);

            using var stream = file.OpenReadStream();
            using var optimizedStream = await OptimizeImageAsync(stream);

            await EnsureContainerExistsAsync();
            var blobClient = _containerClient.GetBlobClient(blobName);

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = GetContentType(fileName),
                    CacheControl = "public, max-age=31536000"
                },
                Metadata = new Dictionary<string, string>
                {
                    ["OriginalFileName"] = file.FileName,
                    ["UploadedAt"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    ["Subfolder"] = subfolder ?? "general",
                    ["EntityId"] = entityId?.ToString() ?? string.Empty
                }
            };

            await blobClient.UploadAsync(optimizedStream, uploadOptions);

            // Return the blob name, not the full URL
            return blobName;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }

    public async Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, Guid? entityId = null,
        string? subfolder = null)
    {
        var uploadTasks = files.Select(file => UploadFileAsync(file, entityId, subfolder));
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
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(1920, 1920)
            }));

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

    private string ExtractBlobNameFromUrl(string fileUrl)
    {
        string[] urlSchemes = { "https://", "http://", "ftp://" };

        if (!urlSchemes.Any(x => fileUrl.Contains(x))) return fileUrl;

        // If it's a full URL, extract the blob name
        var uri = new Uri(fileUrl);
        return uri.Segments.Skip(2).Aggregate(string.Empty, (current, segment) => current + segment);
    }
}