using FindPet.Media.Interfaces;

namespace FindPet.Media.Services;

public class AzureBlobStorageService : IMediaStorageService
{
    public Task<bool> DeleteImageAsync(string imageUrl)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> GetImageAsync(string imageUrl)
    {
        throw new NotImplementedException();
    }

    public Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        throw new NotImplementedException();
    }
}