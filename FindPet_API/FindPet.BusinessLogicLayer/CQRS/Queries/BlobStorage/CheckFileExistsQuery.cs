using FindPet.BusinessLogicLayer.CQRS.Common;

namespace FindPet.BusinessLogicLayer.CQRS.Queries.BlobStorage;

public record CheckFileExistsQuery(string FilePath) : IQuery<CheckFileExistsResponse>;

public class CheckFileExistsResponse
{
    public bool Exists { get; set; }
    public string FilePath { get; set; } = string.Empty;
}