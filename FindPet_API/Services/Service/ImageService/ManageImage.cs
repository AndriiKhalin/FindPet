using Interfaces.IImageService;
using Microsoft.AspNetCore.Http;

namespace Services.Service.ImageService;

public class ManageImage<T> : IManageImage<T> where T : class
{
    public string ImgPath { get; set; }

    public ManageImage()
    {
        ImgPath = GetPath();
    }
    public void DeletePhoto(string filePath)
    {
        if (File.Exists(filePath))
        {
            string nameFile = Path.GetFileName(filePath);

            string navigationPath = NavigateToFolder(filePath, "Images");
            string deletedFolderPath = Path.Combine(navigationPath, "Deleted");
            string entityFolderPath = Path.Combine(deletedFolderPath, typeof(T).Name);


            if (!Directory.Exists(entityFolderPath))
            {
                Directory.CreateDirectory(entityFolderPath);
            }

            string newFilePath = Path.Combine(entityFolderPath, Path.GetFileName(filePath));

            File.Move(filePath, newFilePath);

            File.Delete(filePath);
        }
    }

    public async Task<string> UploadPhotoAsync(IFormFile file, Guid id)
    {

        if (file == null || file.Length == 0)
        {
            return null;
        }

        var rootImg = $"\\Stuff\\Images\\Upload\\{typeof(T).Name}\\";
        var fileName = GetUniqueFileName(file.FileName, id);
        var directoryPath = ImgPath + rootImg;

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var filePath = Path.Combine(directoryPath, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return ImgPath + rootImg + fileName;
    }
    public string GetPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var projectRoot = Path.GetFullPath(Path.Combine(currentDirectory, "..", ".."));
        var pathToImages = Path.Combine(projectRoot, @"FindPet_UI\src\assets\");

        return pathToImages;
    }
    public string GetUniqueFileName(string fileName, Guid id)
    {
        var extension = Path.GetExtension(fileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var newFileName = $"{fileNameWithoutExtension}({id}){extension}";
        return newFileName;
    }

    public string NavigateToFolder(string currentPath, string targetDirectoryName)
    {
        while (Path.GetFileNameWithoutExtension(currentPath) != targetDirectoryName)
        {
            currentPath = Path.GetFullPath(Path.Combine(currentPath, ".."));
        }

        return currentPath;
    }
}