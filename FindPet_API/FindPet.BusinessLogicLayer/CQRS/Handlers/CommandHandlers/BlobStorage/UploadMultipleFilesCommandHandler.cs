using FindPet.BusinessLogicLayer.CQRS.Commands.BlobStorage;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Media.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.BlobStorage;

public class UploadMultipleFilesCommandHandler(IMediaStorageService mediaStorageService)
    : ICommandHandler<UploadMultipleFilesCommand, UploadMultipleFilesResponse>
{
    public async Task<UploadMultipleFilesResponse> Handle(UploadMultipleFilesCommand request, CancellationToken cancellationToken)
    {
        var response = new UploadMultipleFilesResponse();
        var uploadTasks = new List<Task<(bool success, string? filePath, string? secureUrl, string? error)>>();

        foreach (var file in request.Files)
        {
            uploadTasks.Add(UploadSingleFileAsync(file, request.EntityId, request.Subfolder));
        }

        var results = await Task.WhenAll(uploadTasks);

        foreach (var result in results)
        {
            if (result.success && !string.IsNullOrEmpty(result.filePath) && !string.IsNullOrEmpty(result.secureUrl))
            {
                response.FilePaths.Add(result.filePath);
                response.FileSecureUrls.Add(result.secureUrl);
                response.SuccessfulUploads++;
            }
            else
            {
                response.FailedUploads++;
                if (!string.IsNullOrEmpty(result.error))
                {
                    response.ErrorMessages.Add(result.error);
                }
            }
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
        catch (Exception ex)
        {
            return (false, null, null, $"Failed to upload {file.FileName}: {ex.Message}");
        }
    }
}