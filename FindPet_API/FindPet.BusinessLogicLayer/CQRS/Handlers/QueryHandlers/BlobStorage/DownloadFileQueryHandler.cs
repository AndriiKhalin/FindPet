using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.BlobStorage;
using FindPet.Domain.Exceptions;
using FindPet.Media.Interfaces;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.BlobStorage;

public class DownloadFileQueryHandler : IQueryHandler<DownloadFileQuery, DownloadFileResponse>
{
    private readonly IMediaStorageService _mediaStorageService;

    public DownloadFileQueryHandler(IMediaStorageService mediaStorageService)
    {
        _mediaStorageService = mediaStorageService;
    }

    public async Task<DownloadFileResponse> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var fileStream = await _mediaStorageService.DownloadFileAsync(request.FilePath);

            if (fileStream == Stream.Null) throw new NotFoundException("File", request.FilePath);

            var fileName = Path.GetFileName(request.FilePath);
            var contentType = GetContentType(fileName);

            return new DownloadFileResponse
            {
                FileStream = fileStream,
                ContentType = contentType,
                FileName = fileName
            };
        }
        catch (FileNotFoundException)
        {
            throw new NotFoundException("File", request.FilePath);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FileProcessingException("download", ex.Message, request.FilePath);
        }
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".zip" => "application/zip",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }
}