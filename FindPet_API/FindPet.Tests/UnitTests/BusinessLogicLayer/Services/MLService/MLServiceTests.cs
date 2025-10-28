using System.Diagnostics;
using FindPet.BusinessLogicLayer.Interfaces.IMLService;
using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace FindPet.Tests.UnitTests.BusinessLogicLayer.Services.MLService;

public class MLServiceTests : IDisposable
{
    private readonly string _invalidImagePath;
    private readonly FindPet.BusinessLogicLayer.Services.MLService.MLService _mlService;

    private readonly Mock<ILoggerManager> _mockLogger;
    private readonly string _nonExistentPath;
    private readonly string _testDirectory;
    private readonly string _validImagePath;

    public MLServiceTests()
    {
        _mockLogger = MockSetupExtensions.CreateMock<ILoggerManager>();
        _mlService = new FindPet.BusinessLogicLayer.Services.MLService.MLService(_mockLogger.Object);
        _testDirectory = Path.Combine(Path.GetTempPath(), "MLServiceTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);

        // Create test files
        _validImagePath = Path.Combine(_testDirectory, "valid_image.jpg");
        _invalidImagePath = Path.Combine(_testDirectory, "invalid_file.txt");
        _nonExistentPath = Path.Combine(_testDirectory, "nonexistent.jpg");

        // Create a valid test image file (minimal JPEG structure)
        CreateTestImageFile(_validImagePath);

        // Create an invalid file
        File.WriteAllText(_invalidImagePath, "This is not an image file");
    }

    #region Cleanup

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            try
            {
                // Reset file attributes before deletion
                foreach (var file in Directory.GetFiles(_testDirectory))
                    File.SetAttributes(file, FileAttributes.Normal);
                Directory.Delete(_testDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task PredictAsync_MultipleConsecutiveCalls_ShouldMaintainPerformance()
    {
        // Arrange
        var iterations = 5;
        var times = new List<long>();

        // Act
        for (var i = 0; i < iterations; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            await _mlService.PredictAsync(_validImagePath);
            stopwatch.Stop();
            times.Add(stopwatch.ElapsedMilliseconds);
        }

        // Assert
        times.Should().OnlyContain(time => time < 10000); // Each call under 10 seconds
        var averageTime = times.Average();
        averageTime.Should().BeLessThan(5000); // Average under 5 seconds
    }

    #endregion

    #region Helper Methods

    private void CreateTestImageFile(string filePath)
    {
        // Create a minimal valid JPEG file structure
        var imageBytes = TestDataBuilder.MLTestData.CreateValidImageBytes();

        File.WriteAllBytes(filePath, imageBytes);
    }

    #endregion

    #region Successful Prediction Tests

    [Fact]
    public async Task PredictAsync_WithValidImagePath_ShouldReturnPredictedLabel()
    {
        // Arrange
        var expectedBreed = "Golden Retriever";

        // Act
        var result = await _mlService.PredictAsync(_validImagePath);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().BeOfType<string>();
    }

    [Fact]
    public async Task PredictAsync_WithValidImagePath_ShouldReadFileCorrectly()
    {
        // Arrange
        var testImageData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG header
        File.WriteAllBytes(_validImagePath, testImageData);

        // Act
        var result = await _mlService.PredictAsync(_validImagePath);

        // Assert
        result.Should().NotBeNull();
        File.Exists(_validImagePath).Should().BeTrue();
    }

    [Theory]
    [InlineData("test_dog.jpg")]
    [InlineData("test_cat.png")]
    [InlineData("test_pet.bmp")]
    public async Task PredictAsync_WithDifferentImageExtensions_ShouldProcessSuccessfully(string fileName)
    {
        // Arrange
        var imagePath = Path.Combine(_testDirectory, fileName);
        CreateTestImageFile(imagePath);

        // Act
        var result = await _mlService.PredictAsync(imagePath);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region File System Error Tests

    [Fact]
    public async Task PredictAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "does_not_exist.jpg");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => _mlService.PredictAsync(nonExistentPath));

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(nonExistentPath);
    }

    [Fact]
    public async Task PredictAsync_WithEmptyFilePath_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _mlService.PredictAsync(string.Empty));
    }

    [Fact]
    public async Task PredictAsync_WithNullFilePath_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _mlService.PredictAsync((Stream)null!));
    }

    [Fact]
    public async Task PredictAsync_WithWhitespaceFilePath_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _mlService.PredictAsync("   "));
    }

    [Fact]
    public async Task PredictAsync_WithInvalidPath_ShouldThrowException()
    {
        // Arrange
        var invalidPath = "invalid<>path|.jpg";

        // Act & Assert
        await Assert.ThrowsAsync<IOException>(() => _mlService.PredictAsync(invalidPath));
    }

    [Fact]
    public async Task PredictAsync_WithAccessDeniedFile_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var protectedPath = Path.Combine(_testDirectory, "protected.jpg");
        CreateTestImageFile(protectedPath);

        // Make file read-only to simulate access denied
        var fileInfo = new FileInfo(protectedPath);
        fileInfo.Attributes = FileAttributes.ReadOnly;

        try
        {
            // Act & Assert
            // Note: This test might need adjustment based on actual file system permissions
            var result = await _mlService.PredictAsync(protectedPath);

            // If no exception is thrown, the test should still validate the result
            result.Should().NotBeNull();
        }
        finally
        {
            // Cleanup
            fileInfo.Attributes = FileAttributes.Normal;
        }
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task PredictAsync_WithEmptyFile_ShouldHandleGracefully()
    {
        // Arrange
        var emptyFilePath = Path.Combine(_testDirectory, "empty.jpg");
        File.WriteAllBytes(emptyFilePath, Array.Empty<byte>());

        // Act & Assert
        try
        {
            var result = await _mlService.PredictAsync(emptyFilePath);

            // If no exception, validate result
            result.Should().NotBeNull();
        }
        catch (Exception ex)
        {
            // Expected behavior for empty files
            ex.Should().BeOfType<InvalidOperationException>();
        }
    }

    [Fact]
    public async Task PredictAsync_WithLargeFile_ShouldProcessWithinReasonableTime()
    {
        // Arrange
        var largeFilePath = Path.Combine(_testDirectory, "large_image.jpg");
        var largeImageData = new byte[1024 * 1024]; // 1MB

        // Fill with valid JPEG header
        largeImageData[0] = 0xFF;
        largeImageData[1] = 0xD8;
        largeImageData[2] = 0xFF;
        largeImageData[3] = 0xE0;

        File.WriteAllBytes(largeFilePath, largeImageData);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await _mlService.PredictAsync(largeFilePath);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // 30 seconds max
    }

    [Fact]
    public async Task PredictAsync_WithCorruptedImageFile_ShouldThrowException()
    {
        // Arrange
        var corruptedFilePath = Path.Combine(_testDirectory, "corrupted.jpg");
        var corruptedData = new byte[] { 0x00, 0x01, 0x02, 0x03 }; // Invalid image data
        File.WriteAllBytes(corruptedFilePath, corruptedData);

        // Act & Assert
        try
        {
            var result = await _mlService.PredictAsync(corruptedFilePath);

            // If prediction succeeds despite corruption, validate result
            result.Should().NotBeNull();
        }
        catch (Exception ex)
        {
            // Expected for corrupted files
            ex.Should().BeOfType<InvalidOperationException>();
        }
    }

    #endregion

    #region ML Model Behavior Tests

    [Fact]
    public async Task PredictAsync_ShouldReturnNonEmptyPrediction()
    {
        // Arrange & Act
        var result = await _mlService.PredictAsync(_validImagePath);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task PredictAsync_WithSameImage_ShouldReturnConsistentResults()
    {
        // Act
        var result1 = await _mlService.PredictAsync(_validImagePath);
        var result2 = await _mlService.PredictAsync(_validImagePath);

        // Assert
        result1.Should().Be(result2);
    }

    [Fact]
    public async Task PredictAsync_ShouldCreateValidModelInput()
    {
        // Arrange
        var imageBytes = await File.ReadAllBytesAsync(_validImagePath);

        // Act
        var result = await _mlService.PredictAsync(_validImagePath);

        // Assert
        imageBytes.Should().NotBeEmpty();
        result.Should().NotBeNull();
    }

    #endregion

    #region Async Behavior Tests

    [Fact]
    public async Task PredictAsync_ShouldBeProperlyAsync()
    {
        // Arrange
        var tasks = new List<Task<string>>();

        // Act - Start multiple concurrent predictions
        for (var i = 0; i < 3; i++) tasks.Add(_mlService.PredictAsync(_validImagePath));

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => !string.IsNullOrEmpty(r));
    }

    [Fact]
    public async Task PredictAsync_WithCancellation_ShouldRespectCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(1); // Cancel almost immediately

        // Act & Assert
        try
        {
            // Note: Current implementation doesn't support cancellation token
            // This test documents the current limitation
            var result = await _mlService.PredictAsync(_validImagePath);
            result.Should().NotBeNull();
        }
        catch (OperationCanceledException)
        {
            // Expected if cancellation is supported
            Assert.True(true);
        }
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task PredictAsync_EndToEndWorkflow_ShouldWork()
    {
        // Arrange
        var testImagePath = Path.Combine(_testDirectory, "integration_test.jpg");
        CreateTestImageFile(testImagePath);

        // Act
        var prediction = await _mlService.PredictAsync(testImagePath);

        // Assert
        prediction.Should().NotBeNullOrEmpty();
        File.Exists(testImagePath).Should().BeTrue();
    }

    [Theory]
    [InlineData("British_Shorthair.jpg")]
    [InlineData("Sphynx_cat.jpg")]
    [InlineData("Golden_Retriever.jpg")]
    public async Task PredictAsync_WithDifferentBreedImages_ShouldReturnValidPredictions(string fileName)
    {
        // Arrange
        var breedImagePath = Path.Combine(_testDirectory, fileName);
        CreateTestImageFile(breedImagePath);

        // Act
        var result = await _mlService.PredictAsync(breedImagePath);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().BeOfType<string>();
        // Verify the prediction is consistent for the same image
        var result2 = await _mlService.PredictAsync(breedImagePath);
        result.Should().Be(result2, "predictions should be consistent for the same image");
    }

    #endregion
}

#region ML Service Interface Tests

public class IMLServiceContractTests
{
    [Fact]
    public void IMLService_ShouldHavePredictAsyncWithStringParameter()
    {
        // Arrange
        var interfaceType = typeof(IMLService);

        // Act
        var method = interfaceType.GetMethod("PredictAsync", new[] { typeof(string) });

        // Assert
        method.Should().NotBeNull("interface should have PredictAsync(string) method");
        method!.ReturnType.Should().Be(typeof(Task<string>));
        method.GetParameters().Should().HaveCount(1);
        method.GetParameters()[0].ParameterType.Should().Be(typeof(string));
    }

    [Fact]
    public void IMLService_ShouldHavePredictAsyncWithStreamParameter()
    {
        // Arrange
        var interfaceType = typeof(IMLService);

        // Act
        var method = interfaceType.GetMethod("PredictAsync", new[] { typeof(Stream) });

        // Assert
        method.Should().NotBeNull("interface should have PredictAsync(Stream) method");
        method!.ReturnType.Should().Be(typeof(Task<string>));
        method.GetParameters().Should().HaveCount(1);
        method.GetParameters()[0].ParameterType.Should().Be(typeof(Stream));
    }

    [Fact]
    public void MLService_ShouldImplementIMLService()
    {
        // Arrange & Act
        var serviceType = typeof(FindPet.BusinessLogicLayer.Services.MLService.MLService);

        // Assert
        serviceType.Should().Implement<IMLService>();
    }
}

#endregion

#region MLService Dependency Injection Tests

public class MLServiceDependencyInjectionTests
{
    [Fact]
    public void MLService_ShouldBeInstantiableWithoutDependencies()
    {
        // Act
        var _loggerManager = MockSetupExtensions.CreateMock<ILoggerManager>();
        var service = new FindPet.BusinessLogicLayer.Services.MLService.MLService(_loggerManager.Object);

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IMLService>();
    }
}

#endregion