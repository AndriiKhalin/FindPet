using FindPet.BusinessLogicLayer.CQRS.Commands.BlobStorage;
using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.Domain.Exceptions;
using FindPet.Media.Interfaces;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.BlobStorage;

public class DeleteFileCommandHandler(IMediaStorageService mediaStorageService)
    : ICommandHandler<DeleteFileCommand, DeleteFileResponse>
{
    public async Task<DeleteFileResponse> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var success = await mediaStorageService.DeleteImageAsync(request.FileUrl);

            return new DeleteFileResponse
            {
                Success = success,
                Message = success ? "File deleted successfully" : "File not found or could not be deleted"
            };
        }
        catch (Exception ex)
        {
            throw new FileProcessingException("delete", ex.Message);
        }
    }
}