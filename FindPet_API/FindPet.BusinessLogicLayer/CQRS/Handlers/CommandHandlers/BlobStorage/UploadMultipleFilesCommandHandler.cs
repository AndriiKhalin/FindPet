using FindPet.BusinessLogicLayer.CQRS.Commands.BlobStorage;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Media.Interfaces;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.BlobStorage;

public class UploadMultipleFilesCommandHandler(IMediaStorageService mediaStorageService)
    : ICommandHandler<UploadMultipleFilesCommand, UploadMultipleFilesResponse>
{
    public async Task<UploadMultipleFilesResponse> Handle(UploadMultipleFilesCommand request, CancellationToken cancellationToken)
    {
        var response = new UploadMultipleFilesResponse();
        var uploadTasks = new List<Task<(bool success, string? url, string? error)>>();

        foreach (var file in request.Files)
        {
            uploadTasks.Add(UploadSingleFileAsync(file, request.EntityId, request.Subfolder));
        }

        var results = await Task.WhenAll(uploadTasks);

        foreach (var result in results)
        {
            if (result.success && !string.IsNullOrEmpty(result.url))
            {
                response.FileUrls.Add(result.url);
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

    private async Task<(bool success, string? url, string? error)> UploadSingleFileAsync(
        Microsoft.AspNetCore.Http.IFormFile file,
        Guid? entityId,
        string? subfolder)
    {
        try
        {
            var url = await mediaStorageService.UploadImageAsync(file, entityId, subfolder);
            return (true, url, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"Failed to upload {file.FileName}: {ex.Message}");
        }
    }
}