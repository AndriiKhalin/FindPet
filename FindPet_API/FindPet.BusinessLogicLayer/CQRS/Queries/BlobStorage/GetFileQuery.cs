using FindPet.BusinessLogicLayer.CQRS.Common;

namespace FindPet.BusinessLogicLayer.CQRS.Queries.BlobStorage;

public record GetFileQuery(string FileUrl) : IQuery<GetFileResponse>;

public class GetFileResponse
{
    public Stream FileStream { get; set; } = Stream.Null;
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}