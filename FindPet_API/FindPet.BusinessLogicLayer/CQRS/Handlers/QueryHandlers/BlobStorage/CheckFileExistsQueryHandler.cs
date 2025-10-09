using FindPet.BusinessLogicLayer.CQRS.Common;
using FindPet.BusinessLogicLayer.CQRS.Queries.BlobStorage;
using FindPet.Media.Interfaces;

namespace FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.BlobStorage;

public class CheckFileExistsQueryHandler(IMediaStorageService mediaStorageService)
    : IQueryHandler<CheckFileExistsQuery, CheckFileExistsResponse>
{
    public async Task<CheckFileExistsResponse> Handle(CheckFileExistsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var exists = await mediaStorageService.ImageExistsAsync(request.FileUrl);

            return new CheckFileExistsResponse
            {
                Exists = exists,
                FileUrl = request.FileUrl
            };
        }
        catch (Exception)
        {
            return new CheckFileExistsResponse
            {
                Exists = false,
                FileUrl = request.FileUrl
            };
        }
    }
}