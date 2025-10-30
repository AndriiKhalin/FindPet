using FindPet.BusinessLogicLayer.CQRS.Commands.BlobStorage;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Media.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.BlobStorage;

public class UploadMultipleFilesCommandHandler(IMediaStorageService mediaStorageService)
    : ICommandHandler<UploadMultipleFilesCommand, UploadMultipleFilesResponse>
{
    public async Task<UploadMultipleFilesResponse> Handle(UploadMultipleFilesCommand request,
        CancellationToken cancellationToken)
    {
        var response = new UploadMultipleFilesResponse();
        try
        {
            // Use the UploadMultipleFilesAsync method for batch upload
            var filePaths =
                await mediaStorageService.UploadMultipleFilesAsync(request.Files, request.EntityId, request.Subfolder);

            // Generate secure URLs for each file
            var secureUrlTasks =
                filePaths.Select(path => mediaStorageService.GetFileUrlAsync(path, TimeSpan.FromHours(24)));
            var secureUrls = await Task.WhenAll(secureUrlTasks);

            // Add results to response
            response.FilePaths.AddRange(filePaths);
            response.FileSecureUrls.AddRange(secureUrls);
            response.SuccessfulUploads = filePaths.Count;
        }
        catch (IOException ioEx)
        {
            response.FailedUploads = request.Files.Count;
            response.ErrorMessages.Add($"IO error during batch upload: {ioEx.Message}");
        }
        catch (ArgumentException argEx)
        {
            response.FailedUploads = request.Files.Count;
            response.ErrorMessages.Add($"Invalid argument: {argEx.Message}");
        }

        return response;
    }

    private async Task<(bool success, string? filePath, string? secureUrl, string? error)> UploadSingleFileAsync(
        IFormFile file,
        Guid? entityId,
        string? subfolder)
    {
        try
        {
            var filePath = await mediaStorageService.UploadFileAsync(file, entityId, subfolder);
            var secureUrl = await mediaStorageService.GetFileUrlAsync(filePath, TimeSpan.FromHours(24));
            return (true, filePath, secureUrl, null);
        }
        catch (IOException ex)
        {
            return (false, null, null, $"Failed to upload {file.FileName}: {ex.Message}");
        }
    }
}