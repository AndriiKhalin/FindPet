using FindPet.BusinessLogicLayer.CQRS.Common;

namespace FindPet.BusinessLogicLayer.CQRS.Queries.BlobStorage;

public record CheckFileExistsQuery(string FileUrl) : IQuery<CheckFileExistsResponse>;

public class CheckFileExistsResponse
{
    public bool Exists { get; set; }
    public string FileUrl { get; set; } = string.Empty;
}