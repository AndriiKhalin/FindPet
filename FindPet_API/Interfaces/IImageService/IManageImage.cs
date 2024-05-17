using Microsoft.AspNetCore.Http;

namespace Interfaces.IImageService;

public interface IManageImage<T> where T : class
{
    string ImgPath { get; set; }
    void DeleteFile(string filePath);
    Task<string> UploadFileAsync(IFormFile file);
    string GetPath();
    string GetUniqueFileName(string fileName);
}