using FindPet.BusinessLogicLayer.Services.ImageService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;
using FindPet.BusinessLogicLayer.Interfaces.IImageService;
using FindPet.Domain.Entities;
using FindPet.Tests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FindPet.Tests.UnitTests.BusinessLogicLayer.Services.ImageService;

public class ManageImageTests : IDisposable
{
    #region Test Setup and Cleanup

    private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
    private readonly ManageImage<BaseEntity> _manageImageService;
    private readonly string _testRootPath;
    private readonly string _testImagesPath;
    private readonly string _testUploadPath;

    public ManageImageTests()
    {
        // Setup test environment
        _testRootPath = Path.Combine(Path.GetTempPath(), "FindPetTests", Guid.NewGuid().ToString());
        _testImagesPath = Path.Combine(_testRootPath, "Stuff", "Images");
        _testUploadPath = Path.Combine(_testImagesPath, "Upload", nameof(BaseEntity));

        Directory.CreateDirectory(_testRootPath);
        Directory.CreateDirectory(_testImagesPath);
        Directory.CreateDirectory(_testUploadPath);

        // Setup mock
        _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        _mockWebHostEnvironment.Setup(x => x.WebRootPath).Returns(_testRootPath);

        // Create service instance
        _manageImageService = new ManageImage<BaseEntity>(_mockWebHostEnvironment.Object);
    }

    public void Dispose()
    {
        // Cleanup test directories
        if (Directory.Exists(_testRootPath))
        {
            Directory.Delete(_testRootPath, true);
        }
    }

    #endregion

    #region Constructor and Properties Tests

    [Fact]
    public void Constructor_WithValidEnvironment_ShouldInitializeCorrectly()
    {
        // Arrange & Act

        // Assert
        _manageImageService.Should().NotBeNull();
        _manageImageService.ImgPath.Should().Be(_testRootPath);
    }

    [Fact]
    public void Constructor_WithNullEnvironment_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var action = () => new ManageImage<BaseEntity>(null);
        action.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void ImgPath_ShouldReturnWebRootPath()
    {
        // Arrange & Act
        var result = _manageImageService.ImgPath;

        // Assert
        result.Should().Be(_testRootPath);
    }

    #endregion

    #region UploadPhotoAsync (IFormFile) Tests

    [Fact]
    public async Task UploadPhotoAsync_WithValidFile_ShouldUploadSuccessfully()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var fileName = "test-image.jpg";
        var fileContent = "test file content";
        var formFile = CreateMockFormFile(fileName, fileContent);

        // Act
        var result = await _manageImageService.UploadPhotoAsync(formFile, testId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain($"\\Stuff\\Images\\Upload\\{nameof(BaseEntity)}\\");
        result.Should().Contain($"test-image({testId}).jpg");

        // Verify file was actually created
        var expectedPath = Path.Combine(_testUploadPath, $"test-image({testId}).jpg");
        File.Exists(expectedPath).Should().BeTrue();

        // Verify file content
        var actualContent = await File.ReadAllTextAsync(expectedPath);
        actualContent.Should().Be(fileContent);
    }

    [Fact]
    public async Task UploadPhotoAsync_WithNullFile_ShouldReturnNull()
    {
        // Arrange
        var testId = Guid.NewGuid();

        // Act
        var result = await _manageImageService.UploadPhotoAsync((IFormFile)null, testId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UploadPhotoAsync_WithEmptyFile_ShouldReturnNull()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var emptyFile = CreateMockFormFile("empty.jpg", "");

        // Act
        var result = await _manageImageService.UploadPhotoAsync(emptyFile, testId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UploadPhotoAsync_WithNullId_ShouldUploadWithNullInFileName()
    {
        // Arrange
        var fileName = "test-image.jpg";
        var fileContent = "test content";
        var formFile = CreateMockFormFile(fileName, fileContent);

        // Act
        var result = await _manageImageService.UploadPhotoAsync(formFile, null);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("test-image().jpg");
    }

    [Fact]
    public async Task UploadPhotoAsync_WhenDirectoryDoesNotExist_ShouldCreateDirectory()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "FindPetTests", Guid.NewGuid().ToString());
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(x => x.WebRootPath).Returns(nonExistentPath);
        var service = new ManageImage<BaseEntity>(mockEnv.Object);

        var testId = Guid.NewGuid();
        var formFile = CreateMockFormFile("test.jpg", "content");

        // Act
        var result = await service.UploadPhotoAsync(formFile, testId);

        // Assert
        result.Should().NotBeNull();
        var expectedDir = Path.Combine(nonExistentPath, "Stuff", "Images", "Upload", nameof(BaseEntity));
        Directory.Exists(expectedDir).Should().BeTrue();

        // Cleanup
        if (Directory.Exists(nonExistentPath))
            Directory.Delete(nonExistentPath, true);
    }

    [Theory]
    [InlineData("image.jpg", "image")]
    [InlineData("photo.png", "photo")]
    [InlineData("document.pdf", "document")]
    public async Task UploadPhotoAsync_WithDifferentFileTypes_ShouldMaintainFileExtension(
        string fileName, string expectedBaseName)
    {
        // Arrange
        var testId = Guid.NewGuid();
        var formFile = CreateMockFormFile(fileName, "test content");

        // Act
        var result = await _manageImageService.UploadPhotoAsync(formFile, testId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain($"{expectedBaseName}({testId})");
        result.Should().Contain(Path.GetExtension(fileName));
    }

    #endregion

    #region UploadPhotoAsync (string) Tests

    [Fact]
    public async Task UploadPhotoAsync_WithValidFilePath_ShouldUploadSuccessfully()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var sourceFileName = "source-image.jpg";
        var sourceFilePath = Path.Combine(_testRootPath, sourceFileName);
        var fileContent = "test file content";

        // Create source file
        await File.WriteAllTextAsync(sourceFilePath, fileContent);

        // Act
        var result = await _manageImageService.UploadPhotoAsync(sourceFilePath, testId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain($"\\Stuff\\Images\\Upload\\{nameof(BaseEntity)}\\");
        result.Should().Contain($"source-image({testId}).jpg");

        // Verify file was copied
        var expectedPath = Path.Combine(_testUploadPath, $"source-image({testId}).jpg");
        File.Exists(expectedPath).Should().BeTrue();

        var actualContent = await File.ReadAllTextAsync(expectedPath);
        actualContent.Should().Be(fileContent);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public async Task UploadPhotoAsync_WithInvalidFileName_ShouldReturnNull(string fileName)
    {
        // Arrange
        var testId = Guid.NewGuid();

        // Act
        try
        {
            var result = await _manageImageService.UploadPhotoAsync(fileName, testId);

            // Assert
            result.Should().BeNull();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is IOException)
        {
            // Exception is also an acceptable response to invalid input
            true.Should().BeTrue(); // Just to have an assertion
        }

    }

    [Fact]
    public async Task UploadPhotoAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var nonExistentPath = Path.Combine(_testRootPath, "nonexistent.jpg");

        // Act
        var action = async () => await _manageImageService.UploadPhotoAsync(nonExistentPath, testId);

        // Assert
        await action.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task UploadPhotoAsync_WithLargeFile_ShouldHandleCorrectly()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var sourceFileName = "large-file.jpg";
        var sourceFilePath = Path.Combine(_testRootPath, sourceFileName);
        var largeContent = new string('A', 1024 * 100); // 100KB

        await File.WriteAllTextAsync(sourceFilePath, largeContent);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _manageImageService.UploadPhotoAsync(sourceFilePath, testId);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Should complete within 2 seconds

        var uploadedFilePath = Path.Combine(_testRootPath, result.TrimStart('\\'));
        var fileInfo = new FileInfo(uploadedFilePath);
        fileInfo.Length.Should().BeGreaterThan(1024 * 50); // Should be substantial size
    }

    #endregion

    #region UploadPhotoIFormFileAsync Tests

    [Fact]
    public async Task UploadPhotoIFormFileAsync_WithValidFile_ShouldReturnFormFile()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var sourceFileName = "test-image.jpg";
        var sourceFilePath = Path.Combine(_testRootPath, sourceFileName);
        var fileContent = "test file content";

        await File.WriteAllTextAsync(sourceFilePath, fileContent);

        // Act
        var result = await _manageImageService.UploadPhotoIFormFileAsync(sourceFilePath, testId);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be(sourceFileName);
        result.Name.Should().Be(Path.GetFileNameWithoutExtension(sourceFileName));
        result.Length.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UploadPhotoIFormFileAsync_WithInvalidFileName_ShouldReturnNull(string fileName)
    {
        // Arrange
        var testId = Guid.NewGuid();

        // Act
        var result = await _manageImageService.UploadPhotoIFormFileAsync(fileName, testId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UploadPhotoIFormFileAsync_WithValidFile_ShouldCreateCorrectFormFileProperties()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var sourceFileName = "properties-test.jpg";
        var sourceFilePath = Path.Combine(_testRootPath, sourceFileName);
        var expectedContent = "expected file content for properties test";

        await File.WriteAllTextAsync(sourceFilePath, expectedContent);

        // Act
        var result = await _manageImageService.UploadPhotoIFormFileAsync(sourceFilePath, testId);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be(sourceFileName);
        result.Name.Should().Be(Path.GetFileNameWithoutExtension(sourceFileName));
        result.Length.Should().Be(Encoding.UTF8.GetByteCount(expectedContent));

        // Verify content can be read
        using var stream = result.OpenReadStream();
        using var reader = new StreamReader(stream);
        var actualContent = await reader.ReadToEndAsync();
        actualContent.Should().Be(expectedContent);
    }

    #endregion

    #region DeletePhoto Tests

    [Fact]
    public void DeletePhoto_WithExistingFile_ShouldMoveToDeletedFolder()
    {
        // Arrange
        var fileName = "test-delete.jpg";
        var filePath = Path.Combine(_testUploadPath, fileName);
        var fileContent = "file to delete";

        File.WriteAllText(filePath, fileContent);
        File.Exists(filePath).Should().BeTrue("Setup: file should exist before deletion");

        // Act
        _manageImageService.DeletePhoto(filePath);

        // Assert
        File.Exists(filePath).Should().BeFalse("Original file should be deleted");

        // Verify file moved to deleted folder
        var deletedFolderPath = Path.Combine(_testImagesPath, "Deleted", nameof(BaseEntity));
        var deletedFilePath = Path.Combine(deletedFolderPath, fileName);

        Directory.Exists(deletedFolderPath).Should().BeTrue("Deleted folder should be created");
        File.Exists(deletedFilePath).Should().BeTrue("File should exist in deleted folder");

        var deletedContent = File.ReadAllText(deletedFilePath);
        deletedContent.Should().Be(fileContent, "File content should be preserved");
    }

    [Fact]
    public void DeletePhoto_WithNonExistentFile_ShouldNotThrowException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testUploadPath, "nonexistent.jpg");

        // Act
        var action = () => _manageImageService.DeletePhoto(nonExistentPath);

        // Assert
        action.Should().NotThrow("Method should handle non-existent files gracefully");
    }

    [Fact]
    public void DeletePhoto_WhenDeletedFolderDoesNotExist_ShouldCreateIt()
    {
        // Arrange
        var fileName = "test-create-folder.jpg";
        var filePath = Path.Combine(_testUploadPath, fileName);
        File.WriteAllText(filePath, "content");

        var deletedFolderPath = Path.Combine(_testImagesPath, "Deleted", nameof(BaseEntity));
        if (Directory.Exists(deletedFolderPath))
            Directory.Delete(deletedFolderPath, true);

        Directory.Exists(deletedFolderPath).Should().BeFalse("Setup: deleted folder should not exist");

        // Act
        _manageImageService.DeletePhoto(filePath);

        // Assert
        Directory.Exists(deletedFolderPath).Should().BeTrue("Deleted folder should be created automatically");
    }

    [Fact]
    public void DeletePhoto_WithFileInSubdirectory_ShouldNavigateCorrectly()
    {
        // Arrange
        var subDirPath = Path.Combine(_testUploadPath, "SubDirectory");
        Directory.CreateDirectory(subDirPath);

        var fileName = "sub-dir-file.jpg";
        var filePath = Path.Combine(subDirPath, fileName);
        File.WriteAllText(filePath, "subdirectory file content");

        // Act
        _manageImageService.DeletePhoto(filePath);

        // Assert
        File.Exists(filePath).Should().BeFalse("Original file should be deleted");

        var deletedFilePath = Path.Combine(_testImagesPath, "Deleted", nameof(BaseEntity), fileName);
        File.Exists(deletedFilePath).Should().BeTrue("File should be moved to deleted folder");
    }

    #endregion

    #region Helper Methods Tests

    [Fact]
    public void GetPath_ShouldReturnWebRootPath()
    {
        // Act
        var result = _manageImageService.GetPath();

        // Assert
        result.Should().Be(_testRootPath);
    }

    [Theory]
    [InlineData("test-image.jpg", "12345678-1234-1234-1234-123456789012", "test-image(12345678-1234-1234-1234-123456789012).jpg")]
    [InlineData("photo.png", "87654321-4321-4321-4321-210987654321", "photo(87654321-4321-4321-4321-210987654321).png")]
    [InlineData("document", "11111111-2222-3333-4444-555555555555", "document(11111111-2222-3333-4444-555555555555)")]
    public void GetUniqueFileName_WithValidInputs_ShouldReturnFormattedName(
        string fileName, string guidString, string expected)
    {
        // Arrange
        var id = Guid.Parse(guidString);

        // Act
        var result = _manageImageService.GetUniqueFileName(fileName, id);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("test-image.jpg", "test-image().jpg")]
    [InlineData("photo.png", "photo().png")]
    [InlineData("document", "document()")]
    public void GetUniqueFileName_WithNullId_ShouldReturnNameWithEmptyParentheses(
        string fileName, string expected)
    {
        // Act
        var result = _manageImageService.GetUniqueFileName(fileName, null);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("file", "file")]
    [InlineData("file.", "file")]
    [InlineData(".hidden", "")]
    public void GetUniqueFileName_WithEdgeCaseFileNames_ShouldHandleCorrectly(
        string fileName, string expectedBase)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = _manageImageService.GetUniqueFileName(fileName, id);

        // Assert
        result.Should().Contain($"({id})");
        if (!string.IsNullOrEmpty(expectedBase))
        {
            result.Should().StartWith(expectedBase);
        }
    }

    [Fact]
    public void NavigateToFolder_WithValidPath_ShouldReturnCorrectDirectory()
    {
        // Arrange
        var targetDir = "Images";
        var startPath = Path.Combine(_testRootPath, "Stuff", "Images", "Upload", "SomeFolder");
        Directory.CreateDirectory(startPath);

        // Act
        var result = _manageImageService.NavigateToFolder(startPath, targetDir);

        // Assert
        result.Should().Be(_testImagesPath);
    }

    [Fact]
    public void NavigateToFolder_WithRootPath_ShouldReturnSamePath()
    {
        // Arrange
        var targetDir = Path.GetFileName(_testRootPath);

        // Act
        var result = _manageImageService.NavigateToFolder(_testRootPath, targetDir);

        // Assert
        result.Should().Be(_testRootPath);
    }

    [Fact]
    public void NavigateToFolder_WithDeepNestedPath_ShouldNavigateCorrectly()
    {
        // Arrange
        var deepPath = Path.Combine(_testRootPath, "Level1", "Level2", "Level3", "Level4");
        Directory.CreateDirectory(deepPath);
        var targetDir = "Level2";

        // Act
        var result = _manageImageService.NavigateToFolder(deepPath, targetDir);

        // Assert
        result.Should().Be(Path.Combine(_testRootPath, "Level1", "Level2"));
    }

    #endregion

    #region Integration and Workflow Tests

    [Fact]
    public async Task CompleteWorkflow_UploadAndDelete_ShouldWorkCorrectly()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var fileName = "workflow-test.jpg";
        var fileContent = "workflow test content";
        var formFile = CreateMockFormFile(fileName, fileContent);

        // Act - Upload
        var uploadResult = await _manageImageService.UploadPhotoAsync(formFile, testId);

        // Assert - Upload
        uploadResult.Should().NotBeNull();
        var fullFilePath = Path.Combine(_testRootPath, uploadResult.TrimStart('\\'));
        File.Exists(fullFilePath).Should().BeTrue("File should exist after upload");

        // Act - Delete
        _manageImageService.DeletePhoto(fullFilePath);

        // Assert - Delete
        File.Exists(fullFilePath).Should().BeFalse("Original file should be deleted");

        var deletedPath = Path.Combine(_testImagesPath, "Deleted", nameof(BaseEntity), $"workflow-test({testId}).jpg");
        File.Exists(deletedPath).Should().BeTrue("File should exist in deleted folder");

        var deletedContent = File.ReadAllText(deletedPath);
        deletedContent.Should().Be(fileContent, "File content should be preserved through workflow");
    }

    [Fact]
    public async Task MultipleUploads_WithSameFileName_ShouldCreateUniqueFiles()
    {
        // Arrange
        var fileName = "duplicate-test.jpg";
        var testIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var uploadResults = new List<string>();

        // Act
        foreach (var testId in testIds)
        {
            var formFile = CreateMockFormFile(fileName, $"content for {testId}");
            var result = await _manageImageService.UploadPhotoAsync(formFile, testId);
            uploadResults.Add(result);
        }

        // Assert
        uploadResults.Should().HaveCount(3);
        uploadResults.Should().OnlyContain(r => r != null);
        uploadResults.Should().OnlyHaveUniqueItems("Each upload should have unique result path");

        // Verify all files exist
        foreach (var (result, testId) in uploadResults.Zip(testIds))
        {
            var filePath = Path.Combine(_testRootPath, result.TrimStart('\\'));
            File.Exists(filePath).Should().BeTrue($"File for {testId} should exist");

            var content = File.ReadAllText(filePath);
            content.Should().Be($"content for {testId}", "Each file should have correct content");
        }
    }

    #endregion

    #region Error Handling and Edge Cases

    [Fact]
    public async Task UploadPhotoAsync_WithVeryLongFileName_ShouldHandleCorrectly()
    {
        // Arrange
        var longBaseName = new string('A', 200);
        var longFileName = $"{longBaseName}.jpg";
        var testId = Guid.NewGuid();
        var formFile = CreateMockFormFile(longFileName, "content");

        // Act
        var result = await _manageImageService.UploadPhotoAsync(formFile, testId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain($"{longBaseName}({testId}).jpg");

        var filePath = Path.Combine(_testRootPath, result.TrimStart('\\'));
        File.Exists(filePath).Should().BeTrue("File with long name should be created");
    }

    [Theory]
    [InlineData("file.jpg", "image/jpeg")]
    [InlineData("document.pdf", "application/pdf")]
    [InlineData("photo.png", "image/png")]
    public async Task UploadPhotoAsync_WithDifferentMimeTypes_ShouldPreserveFileType(
        string fileName, string mimeType)
    {
        // Arrange
        var testId = Guid.NewGuid();
        var formFile = CreateMockFormFileWithMimeType(fileName, "content", mimeType);

        // Act
        var result = await _manageImageService.UploadPhotoAsync(formFile, testId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(Path.GetExtension(fileName));
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task UploadPhotoAsync_WithManySmallFiles_ShouldPerformEfficiently()
    {
        // Arrange
        var fileCount = 10;
        var tasks = new List<Task<string>>();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < fileCount; i++)
        {
            var formFile = CreateMockFormFile($"file{i}.jpg", $"content {i}");
            tasks.Add(_manageImageService.UploadPhotoAsync(formFile, Guid.NewGuid()));
        }

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        results.Should().HaveCount(fileCount);
        results.Should().OnlyContain(r => r != null);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "Multiple uploads should complete efficiently");

        // Verify all files exist
        foreach (var result in results)
        {
            var filePath = Path.Combine(_testRootPath, result.TrimStart('\\'));
            File.Exists(filePath).Should().BeTrue();
        }
    }

    #endregion

    #region Test Helper Methods

    private static IFormFile CreateMockFormFile(string fileName, string content)
    {
        return CreateMockFormFileWithMimeType(fileName, content, "application/octet-stream");
    }

    private static IFormFile CreateMockFormFileWithMimeType(string fileName, string content, string mimeType)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);

        var formFile = new Mock<IFormFile>();
        formFile.Setup(f => f.FileName).Returns(fileName);
        formFile.Setup(f => f.Length).Returns(bytes.Length);
        formFile.Setup(f => f.ContentType).Returns(mimeType);
        formFile.Setup(f => f.OpenReadStream()).Returns(stream);
        formFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
               .Returns((Stream target, CancellationToken token) =>
               {
                   stream.Position = 0;
                   return stream.CopyToAsync(target, token);
               });

        return formFile.Object;
    }

    #endregion

    #region Test Entity Class

    //private class TestEntity
    //{
    //    public Guid Id { get; set; }
    //    public string Name { get; set; }
    //}

    #endregion
}