using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.BlobStorage;
using FindPet.Domain.Exceptions;
using FindPet.Media.Interfaces;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.BlobStorage;

public class GetFileQueryHandler : IQueryHandler<GetFileQuery, GetFileResponse>
{
    private readonly IMediaStorageService _mediaStorageService;

    public GetFileQueryHandler(IMediaStorageService mediaStorageService)
    {
        _mediaStorageService = mediaStorageService;
    }

    public async Task<GetFileResponse> Handle(GetFileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var fileStream = await _mediaStorageService.GetFileAsync(request.FilePath);

            if (fileStream == Stream.Null) throw new NotFoundException("File", request.FilePath);

            //var uri = new Uri(request.FilePath);
            var fileName = Path.GetFileName(request.FilePath);
            var contentType = GetContentType(fileName);

            return new GetFileResponse
            {
                FileStream = fileStream,
                ContentType = contentType,
                FileName = fileName
            };
        }
        catch (IOException ex)
        {
            throw new FileProcessingException("download", ex.Message);
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
            _ => "application/octet-stream"
        };
    }
}