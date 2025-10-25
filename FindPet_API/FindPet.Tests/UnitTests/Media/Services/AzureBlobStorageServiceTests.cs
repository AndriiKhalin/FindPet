//using Azure;
//using Azure.Storage.Blobs;
//using Azure.Storage.Blobs.Models;
//using FindPet.Domain.Interfaces.ILoggerService;
//using FindPet.Domain.ValueObjects;
//using FindPet.Media.Services;
//using FindPet.Tests.TestHelpers;
//using FluentAssertions;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Options;
//using Moq;
//using System.Text;
//using Xunit;

//namespace FindPet.Tests.UnitTests.Media.Services;

//public class AzureBlobStorageServiceTests
//{
//    private readonly Mock<ILoggerManager> _mockLogger;
//    private readonly Mock<IOptions<AzureBlobStorageOptions>> _mockOptions;
//    private readonly Mock<BlobServiceClient> _mockBlobServiceClient;
//    private readonly Mock<BlobContainerClient> _mockContainerClient;
//    private readonly Mock<BlobClient> _mockBlobClient;
//    private readonly AzureBlobStorageOptions _options;
//    private readonly string _testConnectionString = "UseDevelopmentStorage=true";
//    private readonly string _containerName = "test-container";
//    private readonly AzureBlobStorageService _sut;

//    public AzureBlobStorageServiceTests()
//    {
//        // Setup mocks
//        _mockLogger = MockSetupExtensions.SetupLoggerMock();
//        _mockBlobServiceClient = new Mock<BlobServiceClient>();
//        _mockContainerClient = new Mock<BlobContainerClient>();
//        _mockBlobClient = new Mock<BlobClient>();

//        // Configure options
//        _options = new AzureBlobStorageOptions
//        {
//            AllowedImageTypes = new List<string> { "image/jpeg", "image/png", "image/gif" },
//            MaxFileSizeBytes = 5 * 1024 * 1024,
//            ImageQuality = 85
//        };

//        _mockOptions = new Mock<IOptions<AzureBlobStorageOptions>>();
//        _mockOptions.Setup(o => o.Value).Returns(_options);

//        // Setup mocked blob service structure using reflection and private field access
//        _sut = new AzureBlobStorageService(_testConnectionString, _mockOptions.Object, _containerName, _mockLogger.Object);

//        // Use reflection to replace the internal BlobServiceClient with our mock
//        // Note: This is necessary because AzureBlobStorageService creates its own BlobServiceClient in the constructor
//        // In a real application, this would be refactored for better testability with dependency injection
//        var blobServiceField = typeof(AzureBlobStorageService).GetField("_blobServiceClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//        blobServiceField?.SetValue(_sut, _mockBlobServiceClient.Object);

//        var containerClientField = typeof(AzureBlobStorageService).GetField("_containerClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//        containerClientField?.SetValue(_sut, _mockContainerClient.Object);

//        // Setup container client to return blob client
//        _mockContainerClient.Setup(c => c.GetBlobClient(It.IsAny<string>()))
//            .Returns(_mockBlobClient.Object);
//    }

//    #region DeleteFileAsync Tests

//    [Fact]
//    public async Task DeleteFileAsync_WithValidFilePath_ShouldDeleteFile()
//    {
//        // Arrange
//        var filePath = "folder/test-image.jpg";
//        var response = new Mock<Response<bool>>();
//        response.Setup(r => r.Value).Returns(true);

//        _mockBlobClient.Setup(c => c.DeleteIfExistsAsync(It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync(response.Object);

//        // Act
//        var result = await _sut.DeleteFileAsync(filePath);

//        // Assert
//        result.Should().BeTrue();
//        _mockBlobClient.Verify(c => c.DeleteIfExistsAsync(
//            It.IsAny<DeleteOptions>(),
//            It.IsAny<CancellationToken>()),
//            Times.Once);
//        _mockLogger.Verify(l => l.LogInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
//    }

//    [Fact]
//    public async Task DeleteFileAsync_WithFullUrl_ShouldExtractBlobNameAndDeleteFile()
//    {
//        // Arrange
//        var fileUrl = "https://teststorage.blob.core.windows.net/container/folder/test-image.jpg";
//        var expectedBlobName = "folder/test-image.jpg";

//        var response = new Mock<Response<bool>>();
//        response.Setup(r => r.Value).Returns(true);

//        _mockBlobClient.Setup(c => c.DeleteIfExistsAsync(It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync(response.Object);

//        // Act
//        var result = await _sut.DeleteFileAsync(fileUrl);

//        // Assert
//        result.Should().BeTrue();
//        _mockContainerClient.Verify(c => c.GetBlobClient(It.Is<string>(s => s.EndsWith(expectedBlobName))), Times.Once);
//    }

//    [Fact]
//    public async Task DeleteFileAsync_WhenExceptionOccurs_ShouldReturnFalseAndLogError()
//    {
//        // Arrange
//        var filePath = "folder/test-image.jpg";
//        _mockBlobClient.Setup(c => c.DeleteIfExistsAsync(It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()))
//            .ThrowsAsync(new RequestFailedException("Storage error"));

//        // Act
//        var result = await _sut.DeleteFileAsync(filePath);

//        // Assert
//        result.Should().BeFalse();
//        _mockLogger.Verify(l => l.LogError(
//            It.Is<string>(s => s.Contains("Failed to delete file")),
//            It.IsAny<string>(),
//            It.IsAny<string>(),
//            It.IsAny<int>()),
//            Times.Once);
//    }

//    #endregion

//    #region DownloadFileAsync Tests

//    [Fact]
//    public async Task DownloadFileAsync_WithValidFilePath_ShouldReturnStream()
//    {
//        // Arrange
//        var filePath = "folder/test-image.jpg";
//        var testStream = new MemoryStream(Encoding.UTF8.GetBytes("test content"));

//        var response = new Mock<Response<BlobDownloadStreamingResult>>();
//        var downloadResult = new Mock<BlobDownloadStreamingResult>();

//        downloadResult.Setup(d => d.Content).Returns(testStream);
//        response.Setup(r => r.Value).Returns(downloadResult.Object);

//        _mockBlobClient.Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(Response.FromValue(true, new Mock<Response>().Object));

//        _mockBlobClient.Setup(c => c.DownloadStreamingAsync(It.IsAny<BlobDownloadOptions>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync(response.Object);

//        // Act
//        var result = await _sut.DownloadFileAsync(filePath);

//        // Assert
//        result.Should().NotBeNull();
//        result.Should().BeSameAs(testStream);
//        _mockBlobClient.Verify(c => c.ExistsAsync(It.IsAny<CancellationToken>()), Times.Once);
//        _mockBlobClient.Verify(c => c.DownloadStreamingAsync(
//            It.IsAny<BlobDownloadOptions>(),
//            It.IsAny<CancellationToken>()),
//            Times.Once);
//    }

//    [Fact]
//    public async Task DownloadFileAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
//    {
//        // Arrange
//        var filePath = "folder/nonexistent-image.jpg";

//        _mockBlobClient.Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(Response.FromValue(false, new Mock<Response>().Object));

//        // Act & Assert
//        await Assert.ThrowsAsync<FileNotFoundException>(() => _sut.DownloadFileAsync(filePath));
//        _mockBlobClient.Verify(c => c.ExistsAsync(It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task DownloadFileAsync_WhenExceptionOccurs_ShouldLogErrorAndRethrow()
//    {
//        // Arrange
//        var filePath = "folder/test-image.jpg";

//        _mockBlobClient.Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(Response.FromValue(true, new Mock<Response>().Object));

//        _mockBlobClient.Setup(c => c.DownloadStreamingAsync(It.IsAny<BlobDownloadOptions>(), It.IsAny<CancellationToken>()))
//            .ThrowsAsync(new RequestFailedException("Storage error"));

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DownloadFileAsync(filePath));

//        exception.Message.Should().Contain("Failed to download file");
//        _mockLogger.Verify(l => l.LogError(
//            It.Is<string>(s => s.Contains("Failed to download file")),
//            It.IsAny<string>(),
//            It.IsAny<string>(),
//            It.IsAny<int>()),
//            Times.Once);
//    }

//    #endregion

//    #region GetFileAsync Tests

//    [Fact]
//    public async Task GetFileAsync_WithValidFilePath_ShouldReturnStream()
//    {
//        // Arrange
//        var filePath = "folder/test-image.jpg";
//        var testStream = new MemoryStream(Encoding.UTF8.GetBytes("test content"));

//        var response = new Mock<Response<BlobDownloadStreamingResult>>();
//        var downloadResult = new Mock<BlobDownloadStreamingResult>();

//        downloadResult.Setup(d => d.Content).Returns(testStream);
//        response.Setup(r => r.Value).Returns(downloadResult.Object);

//        _mockBlobClient.Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(Response.FromValue(true, new Mock<Response>().Object));

//        _mockBlobClient.Setup(c => c.DownloadStreamingAsync(It.IsAny<BlobDownloadOptions>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync(response.Object);

//        // Act
//        var result = await _sut.GetFileAsync(filePath);

//        // Assert
//        result.Should().NotBeNull();
//        result.Should().BeSameAs(testStream);
//    }

//    [Fact]
//    public async Task GetFileAsync_WithNonExistentFile_ShouldReturnNullStream()
//    {
//        // Arrange
//        var filePath = "folder/nonexistent-image.jpg";

//        _mockBlobClient.Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(Response.FromValue(false, new Mock<Response>().Object));

//        // Act
//        var result = await _sut.GetFileAsync(filePath);

//        // Assert
//        result.Should().Be(Stream.Null);
//    }

//    [Fact]
//    public async Task GetFileAsync_WhenExceptionOccurs_ShouldReturnNullStreamAndLogError()
//    {
//        // Arrange
//        var filePath = "folder/test-image.jpg";

//        _mockBlobClient.Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
//            .ThrowsAsync(new RequestFailedException("Storage error"));

//        // Act
//        var result = await _sut.GetFileAsync(filePath);

//        // Assert
//        result.Should().Be(Stream.Null);
//        _mockLogger.Verify(l => l.LogError(
//            It.Is<string>(s => s.Contains("Failed to get file")),
//            It.IsAny<string>(),
//            It.IsAny<string>(),
//            It.IsAny<int>()),
//            Times.Once);
//    }

//    #endregion

//    #region GetFileUrlAsync Tests

//    [Fact]
//    public async Task GetFileUrlAsync_WithValidFilePath_ShouldReturnSasUrl()
//    {
//        // Arrange
//        var filePath = "folder/test-image.jpg";
//        var sasUri = new Uri("https://teststorage.blob.core.windows.net/container/folder/test-image.jpg?sv=2020-08-04&sr=b&sig=abc123");

//        _mockBlobClient.Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(Response.FromValue(true, new Mock<Response>().Object));

//        _mockBlobClient.Setup(c => c.CanGenerateSasUri)
//            .Returns(true);

//        _mockBlobClient.Setup(c => c.GenerateSasUri(It.IsAny<BlobSasBuilder>()))
//            .Returns(sasUri);

//        // Act
//        var result = await _sut.GetFileUrlAsync(filePath);

//        // Assert
//        result.Should().Be(sasUri.ToString());
//        _mockBlobClient.Verify(c => c.GenerateSasUri(It.IsAny<BlobSasBuilder>()), Times.Once);
//    }

//    [Fact]
//    public async Task GetFileUrlAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
//    {
//        // Arrange
//        var filePath = "folder/nonexistent-image.jpg";

//        _mockBlobClient.Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(Response.FromValue(false, new Mock<Response>().Object));

//        // Act & Assert
//        await Assert.ThrowsAsync<FileNotFoundException>(() => _sut.GetFileUrlAsync(filePath));
//    }

//    [Fact]
//    public async Task GetFileUrlAsync_WithCustomExpiry_ShouldCreateSasWithCorrectExpiry()
//    {
//        // Arrange
//        var filePath = "folder/test-image.jpg";
//        var customExpiry = TimeSpan.FromHours(2);
//        var sasUri = new Uri("https://teststorage.blob.core.windows.net/container/folder/test-image.jpg?sv=2020-08-04&sr=b&sig=abc123");

//        _mockBlobClient.Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(Response.FromValue(true, new Mock<Response>().Object));

//        _mockBlobClient.Setup(c => c.CanGenerateSasUri)
//            .Returns(true);

//        _mockBlobClient.Setup(c => c.GenerateSasUri(It.Is<BlobSasBuilder>(b =>
//            b.ExpiresOn > DateTimeOffset.UtcNow.Add(TimeSpan.FromHours(1.9)) &&
//            b.ExpiresOn < DateTimeOffset.UtcNow.Add(TimeSpan.FromHours(2.1)))))
//            .Returns(sasUri);

//        // Act
//        var result = await _sut.GetFileUrlAsync(filePath, customExpiry);

//        // Assert
//        result.Should().Be(sasUri.ToString());
//        _mockBlobClient.Verify(c => c.GenerateSasUri(It.IsAny<BlobSasBuilder>()), Times.Once);
//    }

//    [Fact]
//    public async Task GetFileUrlAsync_WithoutSasCapability_ShouldReturnBlobUri()
//    {
//        // Arrange
//        var filePath = "folder/test-image.jpg";
//        var blobUri = new Uri("https://teststorage.blob.core.windows.net/container/folder/test-image.jpg");

//        _mockBlobClient.Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(Response.FromValue(true, new Mock<Response>().Object));

//        _mockBlobClient.Setup(c => c.CanGenerateSasUri)
//            .Returns(false);

//        _mockBlobClient.Setup(c => c.Uri)
//            .Returns(blobUri);

//        // Act
//        var result = await _sut.GetFileUrlAsync(filePath);

//        // Assert
//        result.Should().Be(blobUri.ToString());
//    }

//    #endregion

//    #region FileExistsAsync Tests

//    [Fact]
//    public async Task FileExistsAsync_WithExistingFile_ShouldReturnTrue()
//    {
//        // Arrange
//        var filePath = "folder/test-image.jpg";

//        _mockBlobClient.Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(Response.FromValue(true, new Mock<Response>().Object));

//        // Act
//        var result = await _sut.FileExistsAsync(filePath);

//        // Assert
//        result.Should().BeTrue();
//    }

//    [Fact]
//    public async Task FileExistsAsync_WithNonExistentFile_ShouldReturnFalse()
//    {
//        // Arrange
//        var filePath = "folder/nonexistent-image.jpg";

//        _mockBlobClient.Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(Response.FromValue(false, new Mock<Response>().Object));

//        // Act
//        var result = await _sut.FileExistsAsync(filePath);

//        // Assert
//        result.Should().BeFalse();
//    }

//    [Fact]
//    public async Task FileExistsAsync_WhenExceptionOccurs_ShouldReturnFalse()
//    {
//        // Arrange
//        var filePath = "folder/test-image.jpg";

//        _mockBlobClient.Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
//            .ThrowsAsync(new RequestFailedException("Storage error"));

//        // Act
//        var result = await _sut.FileExistsAsync(filePath);

//        // Assert
//        result.Should().BeFalse();
//    }

//    #endregion

//    #region UploadFileAsync Tests

//    [Fact]
//    public async Task UploadFileAsync_WithValidFile_ShouldUploadAndReturnBlobName()
//    {
//        // Arrange
//        var mockFile = TestDataBuilder.MLTestData.CreateValidImageFile();
//        var entityId = Guid.NewGuid();
//        var subfolder = "pets";
//        var blobName = $"{subfolder}/test-pet_{entityId.ToString("N")}_";

//        // Setup container
//        _mockContainerClient.Setup(c => c.CreateIfNotExistsAsync(
//            PublicAccessType.Blob,
//            It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new Mock<Response<BlobContainerInfo>>().Object);

//        // Setup blob upload
//        _mockBlobClient.Setup(c => c.UploadAsync(
//            It.IsAny<Stream>(),
//            It.IsAny<BlobUploadOptions>(),
//            It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new Mock<Response<BlobContentInfo>>().Object);

//        // Act
//        var result = await _sut.UploadFileAsync(mockFile, entityId, subfolder);

//        // Assert
//        result.Should().NotBeNullOrEmpty();
//        result.Should().StartWith(subfolder + "/");
//        _mockContainerClient.Verify(c => c.CreateIfNotExistsAsync(
//            PublicAccessType.Blob,
//            It.IsAny<CancellationToken>()),
//            Times.Once);
//        _mockBlobClient.Verify(c => c.UploadAsync(
//            It.IsAny<Stream>(),
//            It.IsAny<BlobUploadOptions>(),
//            It.IsAny<CancellationToken>()),
//            Times.Once);
//    }

//    [Fact]
//    public async Task UploadFileAsync_WithNullFile_ShouldThrowArgumentException()
//    {
//        // Arrange
//        IFormFile nullFile = null;

//        // Act & Assert
//        await Assert.ThrowsAsync<ArgumentException>(() =>
//            _sut.UploadFileAsync(nullFile, Guid.NewGuid(), "pets"));
//    }

//    [Fact]
//    public async Task UploadFileAsync_WithEmptyFile_ShouldThrowArgumentException()
//    {
//        // Arrange
//        var emptyFile = MockSetupExtensions.SetupMockFormFile("empty.jpg", "image/jpeg", 0).Object;

//        // Act & Assert
//        await Assert.ThrowsAsync<ArgumentException>(() =>
//            _sut.UploadFileAsync(emptyFile, Guid.NewGuid(), "pets"));
//    }

//    [Fact]
//    public async Task UploadFileAsync_WithInvalidFileType_ShouldThrowArgumentException()
//    {
//        // Arrange
//        var invalidFile = MockSetupExtensions.SetupMockFormFile("document.pdf", "application/pdf", 1024).Object;

//        // Act & Assert
//        await Assert.ThrowsAsync<ArgumentException>(() =>
//            _sut.UploadFileAsync(invalidFile, Guid.NewGuid(), "pets"));
//    }

//    [Fact]
//    public async Task UploadFileAsync_WithOversizedFile_ShouldThrowArgumentException()
//    {
//        // Arrange
//        var overSizedFile = MockSetupExtensions.SetupMockFormFile("large.jpg", "image/jpeg", _options.MaxFileSizeBytes + 1).Object;

//        // Act & Assert
//        await Assert.ThrowsAsync<ArgumentException>(() =>
//            _sut.UploadFileAsync(overSizedFile, Guid.NewGuid(), "pets"));
//    }

//    [Fact]
//    public async Task UploadFileAsync_WithUploadError_ShouldPropagateException()
//    {
//        // Arrange
//        var mockFile = TestDataBuilder.MLTestData.CreateValidImageFile();

//        _mockContainerClient.Setup(c => c.CreateIfNotExistsAsync(
//            It.IsAny<PublicAccessType>(),
//            It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new Mock<Response<BlobContainerInfo>>().Object);

//        _mockBlobClient.Setup(c => c.UploadAsync(
//            It.IsAny<Stream>(),
//            It.IsAny<BlobUploadOptions>(),
//            It.IsAny<CancellationToken>()))
//            .ThrowsAsync(new RequestFailedException("Storage error"));

//        // Act & Assert
//        await Assert.ThrowsAsync<RequestFailedException>(() =>
//            _sut.UploadFileAsync(mockFile, Guid.NewGuid(), "pets"));
//    }

//    #endregion

//    #region UploadMultipleFilesAsync Tests

//    [Fact]
//    public async Task UploadMultipleFilesAsync_WithValidFiles_ShouldUploadAllAndReturnPaths()
//    {
//        // Arrange
//        var mockFiles = new List<IFormFile>
//            {
//                TestDataBuilder.MLTestData.CreateValidImageFile("test1.jpg"),
//                TestDataBuilder.MLTestData.CreateValidImageFile("test2.jpg")
//            };

//        var entityId = Guid.NewGuid();
//        var subfolder = "pets";

//        // Setup to return different blob names for each upload
//        _mockContainerClient.Setup(c => c.CreateIfNotExistsAsync(
//            PublicAccessType.Blob,
//            It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new Mock<Response<BlobContainerInfo>>().Object);

//        _mockBlobClient.Setup(c => c.UploadAsync(
//            It.IsAny<Stream>(),
//            It.IsAny<BlobUploadOptions>(),
//            It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new Mock<Response<BlobContentInfo>>().Object);

//        // Act
//        var results = await _sut.UploadMultipleFilesAsync(mockFiles, entityId, subfolder);

//        // Assert
//        results.Should().NotBeNull();
//        results.Count.Should().Be(2);
//        results.ForEach(path => path.Should().StartWith(subfolder + "/"));
//    }

//    [Fact]
//    public async Task UploadMultipleFilesAsync_WithEmptyList_ShouldReturnEmptyList()
//    {
//        // Arrange
//        var emptyList = new List<IFormFile>();

//        // Act
//        var results = await _sut.UploadMultipleFilesAsync(emptyList, Guid.NewGuid(), "pets");

//        // Assert
//        results.Should().NotBeNull();
//        results.Should().BeEmpty();
//    }

//    [Fact]
//    public async Task UploadMultipleFilesAsync_WithSomeInvalidFiles_ShouldThrowException()
//    {
//        // Arrange
//        var mockFiles = new List<IFormFile>
//            {
//                TestDataBuilder.MLTestData.CreateValidImageFile("test1.jpg"),
//                MockSetupExtensions.SetupMockFormFile("document.pdf", "application/pdf", 1024).Object
//            };

//        // Act & Assert
//        await Assert.ThrowsAsync<ArgumentException>(() =>
//            _sut.UploadMultipleFilesAsync(mockFiles, Guid.NewGuid(), "pets"));
//    }

//    #endregion

//    #region Private Method Tests (Using Reflection)

//    [Theory]
//    [InlineData("test file.jpg", "test_file.jpg")]
//    [InlineData("test#file.jpg", "test_file.jpg")]
//    [InlineData("test?file&name+.jpg", "test_file_name_.jpg")]
//    [InlineData("__test__file__.jpg", "test_file_.jpg")]
//    public void SanitizeFileName_WithVariousInputs_ShouldReturnSanitizedName(string input, string expected)
//    {
//        // Arrange
//        var method = typeof(AzureBlobStorageService).GetMethod("SanitizeFileName",
//            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

//        // Act
//        var result = method.Invoke(_sut, new object[] { input }) as string;

//        // Assert
//        result.Should().Be(expected);
//    }

//    [Fact]
//    public void GenerateUniqueFileName_ShouldIncludeEntityIdAndTimestamp()
//    {
//        // Arrange
//        var originalFileName = "test-image.jpg";
//        var entityId = Guid.NewGuid();
//        var method = typeof(AzureBlobStorageService).GetMethod("GenerateUniqueFileName",
//            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

//        // Act
//        var result = method.Invoke(_sut, new object[] { originalFileName, entityId }) as string;

//        // Assert
//        result.Should().StartWith("test-image_");
//        result.Should().Contain(entityId.ToString("N"));
//        result.Should().EndWith(".jpg");
//        result.Should().Match(s => System.Text.RegularExpressions.Regex.IsMatch(s, @"_\d{14}\.jpg$"));
//    }

//    [Theory]
//    [InlineData("image.jpg", "image/jpeg")]
//    [InlineData("image.jpeg", "image/jpeg")]
//    [InlineData("image.png", "image/png")]
//    [InlineData("image.gif", "image/gif")]
//    [InlineData("image.webp", "image/webp")]
//    [InlineData("image.unknown", "image/jpeg")]
//    public void GetContentType_WithVariousExtensions_ShouldReturnCorrectMimeType(string fileName, string expected)
//    {
//        // Arrange
//        var method = typeof(AzureBlobStorageService).GetMethod("GetContentType",
//            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

//        // Act
//        var result = method.Invoke(null, new object[] { fileName }) as string;

//        // Assert
//        result.Should().Be(expected);
//    }

//    [Theory]
//    [InlineData("https://storage.blob.core.windows.net/container/folder/file.jpg", "folder/file.jpg")]
//    [InlineData("http://storage.blob.core.windows.net/container/file.jpg", "file.jpg")]
//    [InlineData("folder/file.jpg", "folder/file.jpg")]
//    [InlineData("file.jpg", "file.jpg")]
//    public void ExtractBlobNameFromUrl_WithVariousUrls_ShouldExtractCorrectName(string input, string expected)
//    {
//        // Arrange
//        var method = typeof(AzureBlobStorageService).GetMethod("ExtractBlobNameFromUrl",
//            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

//        // Act
//        var result = method.Invoke(_sut, new object[] { input }) as string;

//        // Assert
//        result.Should().Be(expected);
//    }

//    #endregion
//}

// -------------------------------------------------------------------------------------

using System.Reflection;
using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Domain.ValueObjects;
using FindPet.Media.Services;
using FindPet.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FindPet.Tests.UnitTests.Media.Services;

/// <summary>
///     Unit tests for AzureBlobStorageService
///     Tests all public methods and private method behavior through reflection
/// </summary>
public class AzureBlobStorageServiceTests : IDisposable
{
    #region Constructor

    public AzureBlobStorageServiceTests()
    {
        // Setup mocks
        _mockLogger = MockSetupExtensions.SetupLoggerMock();
        _mockBlobServiceClient = new Mock<BlobServiceClient>();
        _mockContainerClient = new Mock<BlobContainerClient>();
        _mockBlobClient = new Mock<BlobClient>();

        // Configure options
        _options = CreateTestOptions();
        _mockOptions = new Mock<IOptions<AzureBlobStorageOptions>>();
        _mockOptions.Setup(o => o.Value).Returns(_options);

        // Create SUT and setup mocks
        _azureBlobStorageService = new AzureBlobStorageService(
            TEST_CONNECTION_STRING,
            _mockOptions.Object,
            CONTAINER_NAME,
            _mockLogger.Object);

        SetupInternalMocks();
        SetupCommonMockBehaviors();
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        // Clean up any resources if needed
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Fields and Constants

    private const string TEST_CONNECTION_STRING = "UseDevelopmentStorage=true";
    private const string CONTAINER_NAME = "test-container";
    private const string TEST_FILE_PATH = "folder/test-image.jpg";
    private const string TEST_FILE_CONTENT = "test content";
    private const string STORAGE_ERROR_MESSAGE = "Storage error";

    private readonly Mock<ILoggerManager> _mockLogger;
    private readonly Mock<IOptions<AzureBlobStorageOptions>> _mockOptions;
    private readonly Mock<BlobServiceClient> _mockBlobServiceClient;
    private readonly Mock<BlobContainerClient> _mockContainerClient;
    private readonly Mock<BlobClient> _mockBlobClient;
    private readonly AzureBlobStorageOptions _options;
    private readonly AzureBlobStorageService _azureBlobStorageService;

    #endregion

    #region Setup Helpers

    private static AzureBlobStorageOptions CreateTestOptions()
    {
        return new AzureBlobStorageOptions
        {
            AllowedImageTypes = new List<string> { "image/jpeg", "image/png", "image/gif", "image/webp" },
            MaxFileSizeBytes = 5 * 1024 * 1024, // 5MB
            ImageQuality = 85
        };
    }

    private void SetupInternalMocks()
    {
        // Use reflection to replace internal clients with mocks
        // Note: In production code, consider using dependency injection for better testability
        var blobServiceField = typeof(AzureBlobStorageService)
            .GetField("_blobServiceClient", BindingFlags.NonPublic | BindingFlags.Instance);
        blobServiceField?.SetValue(_azureBlobStorageService, _mockBlobServiceClient.Object);

        var containerClientField = typeof(AzureBlobStorageService)
            .GetField("_containerClient", BindingFlags.NonPublic | BindingFlags.Instance);
        containerClientField?.SetValue(_azureBlobStorageService, _mockContainerClient.Object);
    }

    private void SetupCommonMockBehaviors()
    {
        _mockContainerClient.Setup(c => c.GetBlobClient(It.IsAny<string>()))
            .Returns(_mockBlobClient.Object);
    }

    #endregion

    #region DeleteFileAsync Tests

    [Fact]
    public async Task DeleteFileAsync_WithValidFilePath_ShouldDeleteFileAndReturnTrue()
    {
        // Arrange
        var response = Response.FromValue(true, new Mock<Response>().Object);
        _mockBlobClient.Setup(c => c.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(),
                It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _azureBlobStorageService.DeleteFileAsync(TEST_FILE_PATH);

        // Assert
        result.Should().BeTrue();
        VerifyDeleteOperation();
        VerifySuccessfulDeletionLogged();
    }

    [Fact]
    public async Task DeleteFileAsync_WithFullUrl_ShouldExtractBlobNameAndDeleteFile()
    {
        // Arrange
        var fileUrl = "https://teststorage.blob.core.windows.net/container/folder/test-image.jpg";
        var response = Response.FromValue(true, new Mock<Response>().Object);

        _mockBlobClient.Setup(c => c.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(),
                It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _azureBlobStorageService.DeleteFileAsync(fileUrl);

        // Assert
        result.Should().BeTrue();
        _mockContainerClient.Verify(c => c.GetBlobClient(It.Is<string>(s => s.Contains("folder/test-image.jpg"))),
            Times.Once);
    }

    [Fact]
    public async Task DeleteFileAsync_WhenFileNotFound_ShouldReturnFalseWithoutError()
    {
        // Arrange
        var response = Response.FromValue(false, new Mock<Response>().Object);
        _mockBlobClient.Setup(c => c.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(),
                It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _azureBlobStorageService.DeleteFileAsync(TEST_FILE_PATH);

        // Assert
        result.Should().BeFalse();
        VerifyDeleteOperation();
        // Should not log success when file doesn't exist
        _mockLogger.Verify(l => l.LogInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteFileAsync_WhenExceptionOccurs_ShouldReturnFalseAndLogError()
    {
        // Arrange
        _mockBlobClient.Setup(c => c.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(),
                It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(STORAGE_ERROR_MESSAGE));

        // Act
        var result = await _azureBlobStorageService.DeleteFileAsync(TEST_FILE_PATH);

        // Assert
        result.Should().BeFalse();
        VerifyErrorLogged("Failed to delete file");
    }

    #endregion

    #region DownloadFileAsync Tests

    [Fact]
    public async Task DownloadFileAsync_WithValidFilePath_ShouldReturnStream()
    {
        // Arrange
        var testContent = Encoding.UTF8.GetBytes(TEST_FILE_CONTENT);
        var testStream = new MemoryStream(testContent);

        SetupFileExistenceCheck(true);

        // Create a real BlobDownloadStreamingResult using BinaryData
        var binaryData = BinaryData.FromBytes(testContent);
        var downloadInfo = BlobsModelFactory.BlobDownloadStreamingResult(binaryData.ToStream());
        var response = Response.FromValue(downloadInfo, new Mock<Response>().Object);

        _mockBlobClient.Setup(c => c.DownloadStreamingAsync(
                It.IsAny<BlobDownloadOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _azureBlobStorageService.DownloadFileAsync(TEST_FILE_PATH);

        // Assert
        result.Should().NotBeNull();

        // Verify the content is correct
        using var reader = new StreamReader(result);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(TEST_FILE_CONTENT);

        VerifyFileExistenceCheck();
        VerifyDownloadStreamingCall();
    }

    [Fact]
    public async Task DownloadFileAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        SetupFileExistenceCheck(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _azureBlobStorageService.DownloadFileAsync(TEST_FILE_PATH));

        exception.Message.Should().Contain(TEST_FILE_PATH);
        VerifyFileExistenceCheck();
        VerifyDownloadStreamingNotCalled();
    }

    [Fact]
    public async Task DownloadFileAsync_WhenDownloadFails_ShouldLogErrorAndRethrow()
    {
        // Arrange
        SetupFileExistenceCheck(true);
        _mockBlobClient.Setup(c =>
                c.DownloadStreamingAsync(It.IsAny<BlobDownloadOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(STORAGE_ERROR_MESSAGE));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _azureBlobStorageService.DownloadFileAsync(TEST_FILE_PATH));

        exception.Message.Should().Contain("Failed to download file");
        exception.InnerException.Should().BeOfType<RequestFailedException>();
        VerifyErrorLogged("Failed to download file");
    }

    #endregion

    #region GetFileAsync Tests

    [Fact]
    public async Task GetFileAsync_WithValidFilePath_ShouldReturnStream()
    {
        // Arrange
        var testContent = Encoding.UTF8.GetBytes(TEST_FILE_CONTENT);

        SetupFileExistenceCheck(true);

        // Create a real BlobDownloadStreamingResult using BinaryData
        var binaryData = BinaryData.FromBytes(testContent);
        var downloadInfo = BlobsModelFactory.BlobDownloadStreamingResult(binaryData.ToStream());
        var response = Response.FromValue(downloadInfo, new Mock<Response>().Object);

        _mockBlobClient.Setup(c => c.DownloadStreamingAsync(
                It.IsAny<BlobDownloadOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _azureBlobStorageService.GetFileAsync(TEST_FILE_PATH);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeSameAs(Stream.Null);

        // Verify the content is correct
        using var reader = new StreamReader(result);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(TEST_FILE_CONTENT);
    }

    [Fact]
    public async Task GetFileAsync_WithNonExistentFile_ShouldReturnNullStream()
    {
        // Arrange
        SetupFileExistenceCheck(false);

        // Act
        var result = await _azureBlobStorageService.GetFileAsync(TEST_FILE_PATH);

        // Assert
        result.Should().BeSameAs(Stream.Null);
    }

    [Fact]
    public async Task GetFileAsync_WhenExceptionOccurs_ShouldReturnNullStreamAndLogError()
    {
        // Arrange
        _mockBlobClient.Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(STORAGE_ERROR_MESSAGE));

        // Act
        var result = await _azureBlobStorageService.GetFileAsync(TEST_FILE_PATH);

        // Assert
        result.Should().BeSameAs(Stream.Null);
        VerifyErrorLogged("Failed to get file");
    }

    #endregion

    #region GetFileUrlAsync Tests

    [Fact]
    public async Task GetFileUrlAsync_WithValidFilePath_ShouldReturnSasUrl()
    {
        // Arrange
        var sasUri =
            new Uri(
                "https://teststorage.blob.core.windows.net/container/folder/test-image.jpg?sv=2020-08-04&sr=b&sig=abc123");
        SetupSasUriGeneration(sasUri, true);

        // Act
        var result = await _azureBlobStorageService.GetFileUrlAsync(TEST_FILE_PATH);

        // Assert
        result.Should().Be(sasUri.ToString());
        VerifyFileExistenceCheck();
        VerifySasUriGeneration();
    }

    [Fact]
    public async Task GetFileUrlAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        SetupFileExistenceCheck(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _azureBlobStorageService.GetFileUrlAsync(TEST_FILE_PATH));

        exception.Message.Should().Contain(TEST_FILE_PATH);
    }

    [Fact]
    public async Task GetFileUrlAsync_WithCustomExpiry_ShouldCreateSasWithCorrectExpiry()
    {
        // Arrange
        var customExpiry = TimeSpan.FromHours(2);
        var sasUri =
            new Uri(
                "https://teststorage.blob.core.windows.net/container/folder/test-image.jpg?sv=2020-08-04&sr=b&sig=abc123");

        SetupFileExistenceCheck(true);
        _mockBlobClient.Setup(c => c.CanGenerateSasUri).Returns(true);
        _mockBlobClient.Setup(c => c.GenerateSasUri(It.Is<BlobSasBuilder>(b =>
                IsExpiryWithinExpectedRange(b.ExpiresOn, customExpiry))))
            .Returns(sasUri);

        // Act
        var result = await _azureBlobStorageService.GetFileUrlAsync(TEST_FILE_PATH, customExpiry);

        // Assert
        result.Should().Be(sasUri.ToString());
        VerifySasUriGeneration();
    }

    [Fact]
    public async Task GetFileUrlAsync_WithoutSasCapability_ShouldReturnBlobUri()
    {
        // Arrange
        var blobUri = new Uri("https://teststorage.blob.core.windows.net/container/folder/test-image.jpg");
        SetupFileExistenceCheck(true);
        _mockBlobClient.Setup(c => c.CanGenerateSasUri).Returns(false);
        _mockBlobClient.Setup(c => c.Uri).Returns(blobUri);

        // Act
        var result = await _azureBlobStorageService.GetFileUrlAsync(TEST_FILE_PATH);

        // Assert
        result.Should().Be(blobUri.ToString());
    }

    #endregion

    #region FileExistsAsync Tests

    [Fact]
    public async Task FileExistsAsync_WithExistingFile_ShouldReturnTrue()
    {
        // Arrange
        SetupFileExistenceCheck(true);

        // Act
        var result = await _azureBlobStorageService.FileExistsAsync(TEST_FILE_PATH);

        // Assert
        result.Should().BeTrue();
        VerifyFileExistenceCheck();
    }

    [Fact]
    public async Task FileExistsAsync_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        SetupFileExistenceCheck(false);

        // Act
        var result = await _azureBlobStorageService.FileExistsAsync(TEST_FILE_PATH);

        // Assert
        result.Should().BeFalse();
        VerifyFileExistenceCheck();
    }

    [Fact]
    public async Task FileExistsAsync_WhenExceptionOccurs_ShouldReturnFalse()
    {
        // Arrange
        _mockBlobClient.Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(STORAGE_ERROR_MESSAGE));

        // Act
        var result = await _azureBlobStorageService.FileExistsAsync(TEST_FILE_PATH);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UploadFileAsync Tests

    [Fact]
    public async Task UploadFileAsync_WithValidFile_ShouldUploadAndReturnBlobName()
    {
        // Arrange
        var mockFile = TestDataBuilder.MLTestData.CreateValidImageFile();
        var entityId = Guid.NewGuid();
        var subfolder = "pets";

        SetupUploadMocks();

        // Act
        var result = await _azureBlobStorageService.UploadFileAsync(mockFile, entityId, subfolder);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith(subfolder + "/");
        result.Should().Contain(entityId.ToString("N"));
        VerifyContainerCreation();
        VerifyFileUpload();
    }

    [Theory]
    [InlineData(null, "File is null or empty")]
    public async Task UploadFileAsync_WithInvalidFile_ShouldThrowArgumentException(IFormFile file,
        string expectedMessage)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _azureBlobStorageService.UploadFileAsync(file, Guid.NewGuid(), "pets"));

        exception.Message.Should().Contain(expectedMessage);
    }

    [Fact]
    public async Task UploadFileAsync_WithEmptyFile_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyFile = MockSetupExtensions.SetupMockFormFile("empty.jpg", "image/jpeg", 0).Object;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _azureBlobStorageService.UploadFileAsync(emptyFile, Guid.NewGuid(), "pets"));

        exception.Message.Should().Contain("File is null or empty");
    }

    [Fact]
    public async Task UploadFileAsync_WithInvalidFileType_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidFile = MockSetupExtensions.SetupMockFormFile("document.pdf", "application/pdf", 1024).Object;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _azureBlobStorageService.UploadFileAsync(invalidFile, Guid.NewGuid(), "pets"));

        exception.Message.Should().Contain("File type application/pdf is not allowed");
    }

    [Fact]
    public async Task UploadFileAsync_WithOversizedFile_ShouldThrowArgumentException()
    {
        // Arrange
        var oversizedFile = MockSetupExtensions
            .SetupMockFormFile("large.jpg", "image/jpeg", _options.MaxFileSizeBytes + 1).Object;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _azureBlobStorageService.UploadFileAsync(oversizedFile, Guid.NewGuid(), "pets"));

        exception.Message.Should()
            .Contain($"File size exceeds maximum allowed size of {_options.MaxFileSizeBytes} bytes");
    }

    [Fact]
    public async Task UploadFileAsync_WithUploadError_ShouldPropagateException()
    {
        // Arrange
        var mockFile = TestDataBuilder.MLTestData.CreateValidImageFile("test-upload-error.jpg");

        _mockContainerClient.Setup(c => c.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(),
                It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<Response<BlobContainerInfo>>().Object);

        _mockBlobClient.Setup(c => c.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<BlobUploadOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(STORAGE_ERROR_MESSAGE));

        // Act & Assert
        await Assert.ThrowsAsync<RequestFailedException>(() =>
            _azureBlobStorageService.UploadFileAsync(mockFile, Guid.NewGuid(), "pets"));
    }

    #endregion

    #region UploadMultipleFilesAsync Tests

    [Fact]
    public async Task UploadMultipleFilesAsync_WithValidFiles_ShouldUploadAllAndReturnPaths()
    {
        // Arrange
        var mockFiles = new List<IFormFile>
        {
            TestDataBuilder.MLTestData.CreateValidImageFile("test1.jpg"),
            TestDataBuilder.MLTestData.CreateValidImageFile("test2.jpg")
        };

        var entityId = Guid.NewGuid();
        var subfolder = "pets";

        SetupUploadMocks();

        // Act
        var results = await _azureBlobStorageService.UploadMultipleFilesAsync(mockFiles, entityId, subfolder);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(2);
        results.Should().OnlyContain(path => path.StartsWith(subfolder + "/"));
        results.Should().OnlyContain(path => path.Contains(entityId.ToString("N")));
    }

    [Fact]
    public async Task UploadMultipleFilesAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyList = new List<IFormFile>();

        // Act
        var results = await _azureBlobStorageService.UploadMultipleFilesAsync(emptyList, Guid.NewGuid(), "pets");

        // Assert
        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task UploadMultipleFilesAsync_WithSomeInvalidFiles_ShouldThrowExceptionForFirstInvalidFile()
    {
        // Arrange
        var mockFiles = new List<IFormFile>
        {
            TestDataBuilder.MLTestData.CreateValidImageFile("test1.jpg"),
            MockSetupExtensions.SetupMockFormFile("document.pdf", "application/pdf", 1024).Object
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _azureBlobStorageService.UploadMultipleFilesAsync(mockFiles, Guid.NewGuid(), "pets"));

        exception.Message.Should().Contain("File type application/pdf is not allowed");
    }

    #endregion

    #region Private Method Tests (Using Reflection)

    [Theory]
    [InlineData("test file.jpg", "test_file.jpg")]
    [InlineData("test#file.jpg", "test_file.jpg")]
    [InlineData("test?file&name+.jpg", "test_file_name_.jpg")]
    [InlineData("__test__file__.jpg", "test_file_.jpg")]
    [InlineData("test(file).jpg", "test_file_.jpg")]
    [InlineData("test%20file.jpg", "test_20file.jpg")]
    public void SanitizeFileName_WithVariousInputs_ShouldReturnSanitizedName(string input, string expected)
    {
        // Arrange
        var method = GetPrivateMethod("SanitizeFileName");

        // Act
        var result = method.Invoke(_azureBlobStorageService, new object[] { input }) as string;

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GenerateUniqueFileName_ShouldIncludeEntityIdAndTimestamp()
    {
        // Arrange
        var originalFileName = "test-image.jpg";
        var entityId = Guid.NewGuid();
        var method = GetPrivateMethod("GenerateUniqueFileName");

        // Act
        var result = method.Invoke(_azureBlobStorageService, new object[] { originalFileName, entityId }) as string;

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("test-image_");
        result.Should().Contain(entityId.ToString("N"));
        result.Should().EndWith(".jpg");
        result.Should().MatchRegex(@"test-image_[a-f0-9]{32}_\d{14}\.jpg");
    }

    [Theory]
    [InlineData("image.jpg", "image/jpeg")]
    [InlineData("image.jpeg", "image/jpeg")]
    [InlineData("image.png", "image/png")]
    [InlineData("image.gif", "image/gif")]
    [InlineData("image.webp", "image/webp")]
    [InlineData("image.unknown", "image/jpeg")]
    [InlineData("IMAGE.JPG", "image/jpeg")]
    public void GetContentType_WithVariousExtensions_ShouldReturnCorrectMimeType(string fileName, string expected)
    {
        // Arrange
        var method = GetPrivateStaticMethod("GetContentType");

        // Act
        var result = method.Invoke(null, new object[] { fileName }) as string;

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("https://storage.blob.core.windows.net/container/folder/file.jpg", "folder/file.jpg")]
    [InlineData("http://storage.blob.core.windows.net/container/file.jpg", "file.jpg")]
    [InlineData("folder/file.jpg", "folder/file.jpg")]
    [InlineData("file.jpg", "file.jpg")]
    public void ExtractBlobNameFromUrl_WithVariousUrls_ShouldExtractCorrectName(string input, string expected)
    {
        // Arrange
        var method = GetPrivateMethod("ExtractBlobNameFromUrl");

        // Act
        var result = method.Invoke(_azureBlobStorageService, new object[] { input }) as string;

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Helper Methods

    private void SetupUploadMocks()
    {
        //_mockContainerClient.Setup(c => c.CreateIfNotExistsAsync(PublicAccessType.Blob,
        //        It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(new Mock<Response<BlobContainerInfo>>().Object);

        //_mockBlobClient.Setup(c => c.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(new Mock<Response<BlobContentInfo>>().Object);

        _mockContainerClient.Setup(c => c.CreateIfNotExistsAsync(
                PublicAccessType.Blob,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<Response<BlobContainerInfo>>().Object);

        _mockBlobClient.Setup(c =>
                c.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<Response<BlobContentInfo>>().Object);
    }

    private void SetupFileExistenceCheck(bool exists)
    {
        _mockBlobClient.Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(exists, new Mock<Response>().Object));
    }

    private void SetupDownloadStreamingMocks(MemoryStream stream, bool fileExists)
    {
        SetupFileExistenceCheck(fileExists);

        if (fileExists)
        {
            var mockDownloadResult = new Mock<BlobDownloadStreamingResult>();
            mockDownloadResult.Setup(r => r.Content).Returns(stream);

            var mockResponse = new Mock<Response<BlobDownloadStreamingResult>>();
            mockResponse.Setup(r => r.Value).Returns(mockDownloadResult.Object);

            _mockBlobClient.Setup(c => c.DownloadStreamingAsync(
                    It.IsAny<BlobDownloadOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mockResponse.Object));
        }
    }

    private void SetupSasUriGeneration(Uri sasUri, bool fileExists)
    {
        SetupFileExistenceCheck(fileExists);
        _mockBlobClient.Setup(c => c.CanGenerateSasUri).Returns(true);
        _mockBlobClient.Setup(c => c.GenerateSasUri(It.IsAny<BlobSasBuilder>())).Returns(sasUri);
    }

    private static bool IsExpiryWithinExpectedRange(DateTimeOffset expiresOn, TimeSpan expectedDuration)
    {
        var expectedExpiry = DateTimeOffset.UtcNow.Add(expectedDuration);
        var tolerance = TimeSpan.FromMinutes(1);

        return Math.Abs((expiresOn - expectedExpiry).TotalMinutes) <= tolerance.TotalMinutes;
    }

    private MethodInfo GetPrivateMethod(string methodName)
    {
        return typeof(AzureBlobStorageService).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
               ?? throw new InvalidOperationException($"Method '{methodName}' not found");
    }

    private MethodInfo GetPrivateStaticMethod(string methodName)
    {
        return typeof(AzureBlobStorageService).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
               ?? throw new InvalidOperationException($"Static method '{methodName}' not found");
    }

    #endregion

    #region Verification Methods

    private void VerifyDeleteOperation()
    {
        _mockBlobClient.Verify(c => c.DeleteIfExistsAsync(
            It.IsAny<DeleteSnapshotsOption>(),
            It.IsAny<BlobRequestConditions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private void VerifySuccessfulDeletionLogged()
    {
        _mockLogger.Verify(l => l.LogInfo(
            It.Is<string>(s => s.Contains("Successfully deleted image")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);
    }

    private void VerifyFileExistenceCheck()
    {
        _mockBlobClient.Verify(c => c.ExistsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private void VerifyDownloadStreamingCall()
    {
        _mockBlobClient.Verify(c => c.DownloadStreamingAsync(
            It.IsAny<BlobDownloadOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private void VerifyDownloadStreamingNotCalled()
    {
        _mockBlobClient.Verify(c => c.DownloadStreamingAsync(
            It.IsAny<BlobDownloadOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private void VerifySasUriGeneration()
    {
        _mockBlobClient.Verify(c => c.GenerateSasUri(It.IsAny<BlobSasBuilder>()), Times.Once);
    }

    private void VerifyContainerCreation()
    {
        //_mockContainerClient.Verify(c => c.CreateIfNotExistsAsync(
        //    It.IsAny<PublicAccessType>(),
        //    It.IsAny<IDictionary<string, string>>(),
        //    It.IsAny<CancellationToken>()), Times.Once);

        _mockContainerClient.Verify(c => c.CreateIfNotExistsAsync(
                PublicAccessType.Blob,
                null,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void VerifyFileUpload()
    {
        _mockBlobClient.Verify(c => c.UploadAsync(
            It.IsAny<Stream>(),
            It.IsAny<BlobUploadOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private void VerifyErrorLogged(string expectedMessage)
    {
        _mockLogger.Verify(l => l.LogError(
            It.Is<string>(s => s.Contains(expectedMessage)),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);
    }

    #endregion
}

// ---------------------------------------------------------------------------------------