namespace FindPet.Media.Interfaces;

public interface IMediaStorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName);

    Task<bool> DeleteImageAsync(string imageUrl);

    Task<Stream> GetImageAsync(string imageUrl);
}