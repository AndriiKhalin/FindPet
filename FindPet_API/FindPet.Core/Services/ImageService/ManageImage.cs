using FindPet.Infrastructure.Interfaces.IImageService;
using Microsoft.AspNetCore.Http;

namespace FindPet.Core.Services.ImageService;

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

        return rootImg + fileName;
    }

    public async Task<string> UploadPhotoAsync(string fileName, Guid id)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }
        // Путь к папке, где будут сохраняться изображения
        var rootImg = $"\\Stuff\\Images\\Upload\\{typeof(T).Name}\\";
        var directoryPath = ImgPath + rootImg;

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Получить уникальное имя файла
        var uniqueFileName = GetUniqueFileName(fileName, id);

        // Полный путь к файлу
        var filePath = Path.Combine(directoryPath, uniqueFileName);

        // Копировать файл в папку загрузки
        File.Copy(fileName, filePath, true);

        // Вернуть путь к загруженному файлу
        return rootImg + uniqueFileName;
    }


    public async Task<IFormFile> UploadPhotoIFormFileAsync(string fileName, Guid id)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        var filePath = await UploadPhotoAsync(fileName, id);

        // Пример чтения файла в MemoryStream
        byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
        MemoryStream memoryStream = new MemoryStream(fileBytes);

        // Создание объекта FormFile
        IFormFile formFile = new FormFile(memoryStream, 0, fileBytes.Length, fileName, fileName);

        return formFile;
    }

    public string GetPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var pathToImages = Path.Combine(currentDirectory, @"wwwroot");

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