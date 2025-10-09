using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.BlobStorage;
using FindPet.Media.Interfaces;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.BlobStorage;

public class GetFileUrlQueryHandler(IMediaStorageService mediaStorageService)
    : IQueryHandler<GetFileUrlQuery, GetFileUrlResponse>
{
    public async Task<GetFileUrlResponse> Handle(GetFileUrlQuery request, CancellationToken cancellationToken)
    {
        var fileUrl = await mediaStorageService.GetImageUrlAsync(request.FileName, request.Subfolder);

        return new GetFileUrlResponse
        {
            FileUrl = fileUrl
        };
    }
}