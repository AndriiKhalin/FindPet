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
            var blobName = await mediaStorageService.UploadFileAsync(
                request.File,
                request.EntityId,
                request.Subfolder);

            var secureFileUrl = await mediaStorageService.GetFileUrlAsync(blobName, TimeSpan.FromHours(24));

            return new UploadFileResponse
            {
                FilePath = blobName, // Store this in your database
                FileSecureUrl = secureFileUrl, // Use this for immediate display
                FileName = request.File.FileName,
                FileSize = request.File.Length,
                ContentType = request.File.ContentType,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }
        catch (ArgumentException ex)
        {
            throw new BadRequestException($"File upload failed: {ex.Message}");
        }
        catch (IOException ex)
        {
            throw new FileProcessingException("upload", ex.Message, request.File.FileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new FileProcessingException("upload", ex.Message, request.File.FileName);
        }
        catch (InvalidOperationException ex)
        {
            throw new FileProcessingException("upload", ex.Message, request.File.FileName);
        }
    }
}