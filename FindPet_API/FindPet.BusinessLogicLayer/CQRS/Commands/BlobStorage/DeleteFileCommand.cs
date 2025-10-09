using FindPet.BusinessLogicLayer.CQRS.Common;

namespace FindPet.BusinessLogicLayer.CQRS.Commands.BlobStorage;

public record DeleteFileCommand(string FileUrl) : ICommand<DeleteFileResponse>;

public class DeleteFileResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}