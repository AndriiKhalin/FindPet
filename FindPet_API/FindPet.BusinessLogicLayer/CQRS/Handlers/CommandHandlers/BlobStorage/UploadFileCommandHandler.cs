using FindPet.BusinessLogicLayer.CQRS.Commands.BlobStorage;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.Exceptions;
using FindPet.Media.Interfaces;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.BlobStorage;

public class UploadFileCommandHandler(IMediaStorageService mediaStorageService)
    : ICommandHandler<UploadFileCommand, UploadFileResponse>
{
    public async Task<UploadFileResponse> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var fileUrl = await mediaStorageService.UploadImageAsync(
                request.File,
                request.EntityId,
                request.Subfolder);

            return new UploadFileResponse
            {
                FileUrl = fileUrl,
                FileName = request.File.FileName,
                FileSize = request.File.Length,
                ContentType = request.File.ContentType
            };
        }
        catch (ArgumentException ex)
        {
            throw new BadRequestException($"File upload failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new FileProcessingException("upload", ex.Message, request.File.FileName);
        }
    }
}