using FindPet.BusinessLogicLayer.CQRS.Common;

namespace FindPet.BusinessLogicLayer.CQRS.Queries.BlobStorage;

public record GetFileUrlQuery(string FileName, string? Subfolder = null) : IQuery<GetFileUrlResponse>;

public class GetFileUrlResponse
{
    public string FileUrl { get; set; } = string.Empty;
}