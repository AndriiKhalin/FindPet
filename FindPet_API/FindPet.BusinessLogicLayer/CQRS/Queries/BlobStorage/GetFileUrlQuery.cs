using FindPet.BusinessLogicLayer.CQRS.Common;

namespace FindPet.BusinessLogicLayer.CQRS.Queries.BlobStorage;

public record GetFileUrlQuery(string FilePath, int ExpiryHours = 24) : IQuery<GetFileUrlResponse>;

public class GetFileUrlResponse
{
    public string FileUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string FilePath { get; set; } = string.Empty;
}