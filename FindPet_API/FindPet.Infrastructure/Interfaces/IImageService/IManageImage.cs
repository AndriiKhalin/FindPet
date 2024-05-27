using Microsoft.AspNetCore.Http;

namespace FindPet.Infrastructure.Interfaces.IImageService;

public interface IManageImage<T> where T : class
{
    string ImgPath { get; set; }
    void DeletePhoto(string filePath);
    Task<string> UploadPhotoAsync(IFormFile file, Guid id);
    string GetPath();
    string GetUniqueFileName(string fileName, Guid id);
}