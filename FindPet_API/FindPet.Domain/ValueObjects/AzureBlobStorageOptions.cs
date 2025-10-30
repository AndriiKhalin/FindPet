namespace FindPet.Domain.ValueObjects;

public class AzureBlobStorageOptions
{
    public const string SectionName = "AzureBlobStorage";
    public List<string> AllowedImageTypes { get; set; } = new();
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024; // 5MB
    public int ImageQuality { get; set; } = 85;
    public ThumbnailSizeOptions ThumbnailSize { get; set; } = new();
}

public class ThumbnailSizeOptions
{
    public int Width { get; set; } = 300;
    public int Height { get; set; } = 300;
}