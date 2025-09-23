using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FindPet.Media.Interfaces;

namespace FindPet.Media.Services;

public class AzureBlobStorageService : IMediaStorageService
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public AzureBlobStorageService(string connectionString, string containerName)
    {
        _connectionString = connectionString;
        _containerName = containerName;
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(imageUrl));
            var response = await blobClient.DeleteIfExistsAsync();
            return response.Value;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Stream> GetImageAsync(string imageUrl)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(imageUrl));
            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }
        catch
        {
            return Stream.Null;
        }
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(imageStream, true);
        return blobClient.Uri.ToString();
    }
}