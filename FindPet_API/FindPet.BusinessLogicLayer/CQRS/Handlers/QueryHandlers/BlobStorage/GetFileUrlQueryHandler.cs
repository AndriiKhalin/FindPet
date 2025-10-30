using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.BlobStorage;
using FindPet.Media.Interfaces;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.BlobStorage;

public class GetFileUrlQueryHandler(IMediaStorageService mediaStorageService)
    : IQueryHandler<GetFileUrlQuery, GetFileUrlResponse>
{
    public async Task<GetFileUrlResponse> Handle(GetFileUrlQuery request, CancellationToken cancellationToken)
    {
        var expiryTime = TimeSpan.FromHours(request.ExpiryHours);

        var fileUrl = await mediaStorageService.GetFileUrlAsync(request.FilePath, expiryTime);

        return new GetFileUrlResponse
        {
            FileUrl = fileUrl,
            ExpiresAt = DateTime.UtcNow.Add(expiryTime),
            FilePath = request.FilePath
        };
    }
}