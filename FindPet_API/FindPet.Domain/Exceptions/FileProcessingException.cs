namespace FindPet.Domain.Exceptions;

public class FileProcessingException : BaseException
{
    public FileProcessingException(string fileOperation, string message, string? fileName = null)
        : base($"File {fileOperation} failed: {message}", 400, "FILE_PROCESSING_ERROR",
            new { FileName = fileName, Operation = fileOperation })
    {
        FileName = fileName;
        FileOperation = fileOperation;
    }

    public string? FileName { get; }
    public string FileOperation { get; }
}