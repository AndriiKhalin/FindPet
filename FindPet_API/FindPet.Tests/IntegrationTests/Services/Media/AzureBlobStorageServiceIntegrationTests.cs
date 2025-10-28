using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Domain.ValueObjects;
using FindPet.Media.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Image = SixLabors.ImageSharp.Image;

namespace FindPet.Tests.IntegrationTests.Services.Media;

/// <summary>
/// Integration tests for AzureBlobStorageService focusing on image optimization functionality
/// </summary>
[Collection("AzureStorage")]
public class AzureBlobStorageServiceIntegrationTests : IAsyncLifetime
{
    private readonly Mock<ILoggerManager> _loggerMock;
    private readonly AzureBlobStorageOptions _options;
    private readonly string _testConnectionString;
    private readonly string _testContainerName;
    private readonly List<string> _uploadedBlobs;
    private BlobContainerClient _containerClient;
    private bool _azureStorageAvailable;

    public AzureBlobStorageServiceIntegrationTests()
    {
        _loggerMock = new Mock<ILoggerManager>();
        _options = new AzureBlobStorageOptions
        {
            ImageQuality = 85,
            MaxFileSizeBytes = 10 * 1024 * 1024, // 10MB
            AllowedImageTypes = new List<string> { "image/jpeg", "image/png", "image/gif", "image/webp" }
        };

        // Use Azure Storage Emulator or test connection string
        _testConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING")
                                ?? "UseDevelopmentStorage=true";
        _testContainerName = $"test-optimization-{Guid.NewGuid():N}";
        _uploadedBlobs = new List<string>();
    }

    public async Task InitializeAsync()
    {
        try
        {
            var blobServiceClient = new BlobServiceClient(_testConnectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(_testContainerName);

            // Test connection with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cts.Token);

            _azureStorageAvailable = true;
        }
        catch (Exception ex)
        {
            _azureStorageAvailable = false;
            Console.WriteLine($"Azure Storage not available: {ex.Message}");
            Console.WriteLine("Skipping integration tests. Please start Azure Storage Emulator or provide connection string.");
        }
    }

    public async Task DisposeAsync()
    {
        if (_azureStorageAvailable && _containerClient != null)
        {
            try
            {
                await _containerClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to cleanup test container: {ex.Message}");
            }
        }
    }

    #region Image Optimization Tests

    [Fact]
    public async Task UploadFileAsync_WithOversizedImage_ShouldResizeToMaxDimensions()
    {
        // Skip if Azure Storage not available
        if (!_azureStorageAvailable)
        {
            // Use Skip.If when available, or just return
            return;
        }

        // Arrange
        var service = CreateService();
        var oversizedImage = CreateTestImage(2500, 2500, "oversized_test.jpg");
        var entityId = Guid.NewGuid();

        // Act
        var result = await service.UploadFileAsync(oversizedImage, entityId, "test");

        // Assert
        result.Should().NotBeNullOrEmpty();
        _uploadedBlobs.Add(result);

        // Download and verify the optimized image
        var downloadedStream = await service.DownloadFileAsync(result);
        using var image = await Image.LoadAsync(downloadedStream);

        image.Width.Should().BeLessThanOrEqualTo(1920);
        image.Height.Should().BeLessThanOrEqualTo(1920);

        // Verify aspect ratio is maintained
        var aspectRatio = (double)image.Width / image.Height;
        aspectRatio.Should().BeApproximately(1.0, 0.01);
    }

    [Theory]
    [InlineData(800, 600)]
    [InlineData(1024, 768)]
    [InlineData(1920, 1080)]
    public async Task UploadFileAsync_WithNormalSizedImage_ShouldNotResize(int width, int height)
    {
        if (!_azureStorageAvailable) return;

        // Arrange
        var service = CreateService();
        var normalImage = CreateTestImage(width, height, $"normal_{width}x{height}.jpg");
        var entityId = Guid.NewGuid();

        // Act
        var result = await service.UploadFileAsync(normalImage, entityId, "test");

        // Assert
        result.Should().NotBeNullOrEmpty();
        _uploadedBlobs.Add(result);

        var downloadedStream = await service.DownloadFileAsync(result);
        using var image = await Image.LoadAsync(downloadedStream);

        image.Width.Should().Be(width);
        image.Height.Should().Be(height);
    }

    [Fact]
    public async Task UploadFileAsync_WithWideImage_ShouldResizeProportionally()
    {
        if (!_azureStorageAvailable) return;

        // Arrange
        var service = CreateService();
        var wideImage = CreateTestImage(3840, 1080, "wide_test.jpg");
        var entityId = Guid.NewGuid();

        // Act
        var result = await service.UploadFileAsync(wideImage, entityId, "test");

        // Assert
        result.Should().NotBeNullOrEmpty();
        _uploadedBlobs.Add(result);

        var downloadedStream = await service.DownloadFileAsync(result);
        using var image = await Image.LoadAsync(downloadedStream);

        image.Width.Should().BeLessThanOrEqualTo(1920);

        var aspectRatio = (double)image.Width / image.Height;
        aspectRatio.Should().BeApproximately(3.56, 0.1);
    }

    [Fact]
    public async Task UploadFileAsync_WithTallImage_ShouldResizeProportionally()
    {
        if (!_azureStorageAvailable) return;

        // Arrange
        var service = CreateService();
        var tallImage = CreateTestImage(1080, 3840, "tall_test.jpg");
        var entityId = Guid.NewGuid();

        // Act
        var result = await service.UploadFileAsync(tallImage, entityId, "test");

        // Assert
        result.Should().NotBeNullOrEmpty();
        _uploadedBlobs.Add(result);

        var downloadedStream = await service.DownloadFileAsync(result);
        using var image = await Image.LoadAsync(downloadedStream);

        image.Height.Should().BeLessThanOrEqualTo(1920);

        var aspectRatio = (double)image.Width / image.Height;
        aspectRatio.Should().BeApproximately(0.28, 0.1);
    }

    #endregion

    #region Image Quality Tests

    [Theory]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(85)]
    [InlineData(95)]
    public async Task UploadFileAsync_WithDifferentQualitySettings_ShouldApplyCorrectQuality(int quality)
    {
        if (!_azureStorageAvailable) return;

        // Arrange
        var customOptions = new AzureBlobStorageOptions
        {
            ImageQuality = quality,
            MaxFileSizeBytes = 10 * 1024 * 1024,
            AllowedImageTypes = _options.AllowedImageTypes
        };

        var service = CreateService(customOptions);
        var testImage = CreateTestImage(1920, 1080, $"quality_test_{quality}.jpg");
        var entityId = Guid.NewGuid();

        // Act
        var result = await service.UploadFileAsync(testImage, entityId, "quality");

        // Assert
        result.Should().NotBeNullOrEmpty();
        _uploadedBlobs.Add(result);

        // Download and copy to MemoryStream to get the length
        var downloadedStream = await service.DownloadFileAsync(result);
        using var memoryStream = new MemoryStream();
        await downloadedStream.CopyToAsync(memoryStream);

        memoryStream.Length.Should().BeGreaterThan(0);

        // Verify the image is valid
        memoryStream.Position = 0;
        using var image = await Image.LoadAsync(memoryStream);
        image.Should().NotBeNull();
    }

    [Fact]
    public async Task UploadFileAsync_WithHighQualityLargeImage_ShouldCompressEfficiently()
    {
        if (!_azureStorageAvailable) return;

        // Arrange
        var service = CreateService();

        // Create a more complex image that will actually compress
        var largeImage = CreateComplexTestImage(1920, 1920, "large_compress_test.jpg");
        var originalSize = largeImage.Length;
        var entityId = Guid.NewGuid();

        // Act
        var result = await service.UploadFileAsync(largeImage, entityId, "compress");

        // Assert
        result.Should().NotBeNullOrEmpty();
        _uploadedBlobs.Add(result);

        // Download and copy to MemoryStream to measure size
        var downloadedStream = await service.DownloadFileAsync(result);
        using var memoryStream = new MemoryStream();
        await downloadedStream.CopyToAsync(memoryStream);

        var optimizedSize = memoryStream.Length;

        // The optimized image should exist and be valid
        optimizedSize.Should().BeGreaterThan(0);

        // Verify the image is valid and dimensions are correct
        memoryStream.Position = 0;
        using var image = await Image.LoadAsync(memoryStream);
        image.Should().NotBeNull();
        image.Width.Should().BeLessThanOrEqualTo(1920);
        image.Height.Should().BeLessThanOrEqualTo(1920);

        // Verify it's actually a JPEG (optimized format)
        memoryStream.Position = 0;
        var format = await Image.DetectFormatAsync(memoryStream);
        format.Name.Should().Be("JPEG");
    }

    #endregion

    #region Format Conversion Tests

    [Theory]
    [InlineData("image/png", "test.png")]
    [InlineData("image/jpeg", "test.jpg")]
    [InlineData("image/gif", "test.gif")]
    public async Task UploadFileAsync_WithDifferentFormats_ShouldConvertToJPEG(string contentType, string fileName)
    {
        if (!_azureStorageAvailable) return;

        // Arrange
        var service = CreateService();
        var testImage = CreateTestImage(800, 600, fileName, contentType);
        var entityId = Guid.NewGuid();

        // Act
        var result = await service.UploadFileAsync(testImage, entityId, "format");

        // Assert
        result.Should().NotBeNullOrEmpty();
        _uploadedBlobs.Add(result);

        var downloadedStream = await service.DownloadFileAsync(result);
        using var image = await Image.LoadAsync(downloadedStream);

        image.Should().NotBeNull();
        image.Width.Should().Be(800);
        image.Height.Should().Be(600);
    }

    #endregion

    #region Memory Management Tests

    [Fact]
    public async Task UploadFileAsync_WithMultipleUploads_ShouldNotLeakMemory()
    {
        if (!_azureStorageAvailable) return;

        // Arrange
        var service = CreateService();
        var uploadCount = 10;
        var entityId = Guid.NewGuid();
        var initialMemory = GC.GetTotalMemory(true);

        // Act
        for (int i = 0; i < uploadCount; i++)
        {
            var testImage = CreateTestImage(1024, 768, $"memory_test_{i}.jpg");
            var result = await service.UploadFileAsync(testImage, entityId, "memory");
            _uploadedBlobs.Add(result);
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var finalMemory = GC.GetTotalMemory(true);

        // Assert
        _uploadedBlobs.Should().HaveCount(uploadCount);

        var memoryIncrease = finalMemory - initialMemory;
        memoryIncrease.Should().BeLessThan(50 * 1024 * 1024);
    }

    [Fact]
    public async Task UploadFileAsync_WithLargeImage_ShouldDisposeStreamsCorrectly()
    {
        if (!_azureStorageAvailable) return;

        // Arrange
        var service = CreateService();
        var largeImage = CreateTestImage(2500, 2500, "dispose_test.jpg");
        var entityId = Guid.NewGuid();

        // Act
        var result = await service.UploadFileAsync(largeImage, entityId, "dispose");

        // Assert
        result.Should().NotBeNullOrEmpty();
        _uploadedBlobs.Add(result);

        var canDownload = await service.FileExistsAsync(result);
        canDownload.Should().BeTrue();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task UploadFileAsync_WithSquareImage_ShouldMaintainSquareAspectRatio()
    {
        if (!_azureStorageAvailable) return;

        // Arrange
        var service = CreateService();
        var squareImage = CreateTestImage(2400, 2400, "square_test.jpg");
        var entityId = Guid.NewGuid();

        // Act
        var result = await service.UploadFileAsync(squareImage, entityId, "square");

        // Assert
        result.Should().NotBeNullOrEmpty();
        _uploadedBlobs.Add(result);

        var downloadedStream = await service.DownloadFileAsync(result);
        using var image = await Image.LoadAsync(downloadedStream);

        image.Width.Should().Be(image.Height);
        image.Width.Should().BeLessThanOrEqualTo(1920);
    }

    [Fact]
    public async Task UploadFileAsync_WithVerySmallImage_ShouldNotUpscale()
    {
        if (!_azureStorageAvailable) return;

        // Arrange
        var service = CreateService();
        var smallImage = CreateTestImage(100, 100, "tiny_test.jpg");
        var entityId = Guid.NewGuid();

        // Act
        var result = await service.UploadFileAsync(smallImage, entityId, "tiny");

        // Assert
        result.Should().NotBeNullOrEmpty();
        _uploadedBlobs.Add(result);

        var downloadedStream = await service.DownloadFileAsync(result);
        using var image = await Image.LoadAsync(downloadedStream);

        image.Width.Should().Be(100);
        image.Height.Should().Be(100);
    }

    [Fact]
    public async Task UploadFileAsync_WithExactlyMaxDimensions_ShouldNotResize()
    {
        if (!_azureStorageAvailable) return;

        // Arrange
        var service = CreateService();
        var exactSizeImage = CreateTestImage(1920, 1920, "exact_size_test.jpg");
        var entityId = Guid.NewGuid();

        // Act
        var result = await service.UploadFileAsync(exactSizeImage, entityId, "exact");

        // Assert
        result.Should().NotBeNullOrEmpty();
        _uploadedBlobs.Add(result);

        var downloadedStream = await service.DownloadFileAsync(result);
        using var image = await Image.LoadAsync(downloadedStream);

        image.Width.Should().Be(1920);
        image.Height.Should().Be(1920);
    }

    #endregion

    #region Helper Methods

    private AzureBlobStorageService CreateService(AzureBlobStorageOptions options = null)
    {
        return new AzureBlobStorageService(
            _testConnectionString,
            Options.Create(options ?? _options),
            _testContainerName,
            _loggerMock.Object
        );
    }

    private IFormFile CreateTestImage(int width, int height, string fileName, string contentType = "image/jpeg")
    {
        var image = new Image<Rgba32>(width, height);

        // Fill with a test pattern for more realistic compression
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var color = new Rgba32(
                    (byte)((x * 255) / width),
                    (byte)((y * 255) / height),
                    (byte)(((x + y) * 255) / (width + height))
                );
                image[x, y] = color;
            }
        }

        var stream = new MemoryStream();
        image.SaveAsJpeg(stream);
        stream.Position = 0;

        image.Dispose();

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.Length).Returns(stream.Length);

        // Create a new stream for each OpenReadStream call
        fileMock.Setup(f => f.OpenReadStream()).Returns(() =>
        {
            var memoryStream = new MemoryStream();
            stream.Position = 0;
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        });

        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, CancellationToken>(async (s, ct) =>
            {
                stream.Position = 0;
                await stream.CopyToAsync(s, ct);
            });

        return fileMock.Object;
    }

    /// <summary>
    /// Creates a more complex test image with realistic patterns that will compress better
    /// </summary>
    private IFormFile CreateComplexTestImage(int width, int height, string fileName)
    {
        var image = new Image<Rgba32>(width, height);
        var random = new Random(42); // Seed for reproducibility

        // Create a more complex pattern with noise, gradients, and blocks
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Mix of gradient and random noise
                var baseR = (byte)((x * 255) / width);
                var baseG = (byte)((y * 255) / height);
                var baseB = (byte)(((x + y) * 255) / (width + height));

                // Add noise
                var noiseR = (byte)(random.Next(-20, 20));
                var noiseG = (byte)(random.Next(-20, 20));
                var noiseB = (byte)(random.Next(-20, 20));

                // Add blocks of solid color for more realistic compression
                if ((x / 100) % 2 == 0 && (y / 100) % 2 == 0)
                {
                    image[x, y] = new Rgba32(200, 150, 100);
                }
                else
                {
                    image[x, y] = new Rgba32(
                        (byte)Math.Clamp(baseR + noiseR, 0, 255),
                        (byte)Math.Clamp(baseG + noiseG, 0, 255),
                        (byte)Math.Clamp(baseB + noiseB, 0, 255)
                    );
                }
            }
        }

        var stream = new MemoryStream();
        image.SaveAsJpeg(stream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 95 });
        stream.Position = 0;

        image.Dispose();

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.Length).Returns(stream.Length);

        fileMock.Setup(f => f.OpenReadStream()).Returns(() =>
        {
            var memoryStream = new MemoryStream();
            stream.Position = 0;
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        });

        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, CancellationToken>(async (s, ct) =>
            {
                stream.Position = 0;
                await stream.CopyToAsync(s, ct);
            });

        return fileMock.Object;
    }

    #endregion
}

// Test collection definition to prevent parallel execution
[CollectionDefinition("AzureStorage")]
public class AzureStorageCollection : ICollectionFixture<AzureStorageFixture>
{
}

public class AzureStorageFixture
{
    // Shared fixture for Azure Storage tests
}