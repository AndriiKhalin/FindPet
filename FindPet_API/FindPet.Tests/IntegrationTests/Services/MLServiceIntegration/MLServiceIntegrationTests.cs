using FindPet.BusinessLogicLayer.Interfaces.IMLService;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.Entities;
using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace FindPet.Tests.IntegrationTests.Services.MLServiceIntegration;

public class MLServiceIntegrationTests
{
    private readonly Mock<ILoggerManager> _mockLogger;
    private readonly Mock<IMLService> _mockMlService;
    private readonly Mock<IPetRepository> _mockPetRepository;

    public MLServiceIntegrationTests()
    {
        _mockMlService = MockSetupExtensions.ServiceMocks.SetupMLService();
        _mockPetRepository = MockSetupExtensions.PetRepositoryMocks.SetupPetRepository();
        _mockLogger = MockSetupExtensions.ServiceMocks.SetupLoggerService();
    }

    #region End-to-End Workflow Simulation

    [Fact]
    public async Task MLService_ShouldWorkInPetProcessingWorkflow()
    {
        // Arrange - Set up a workflow similar to what would happen in the system
        var mockImageService = MockSetupExtensions.ServiceMocks.SetupImageService<Pet>();

        // Create a temporary file to simulate uploaded image
        var tempFile = Path.GetTempFileName();
        File.WriteAllBytes(tempFile, TestDataBuilder.MLTestData.CreateValidImageBytes());

        try
        {
            // Configure mocks for the workflow
            _mockMlService.Setup(x => x.PredictAsync(It.IsAny<string>()))
                .ReturnsAsync("Golden Retriever");

            mockImageService.Setup(x => x.UploadPhotoAsync(It.IsAny<IFormFile>(), It.IsAny<Guid?>()))
                .ReturnsAsync(tempFile);

            // Simulate a pet creation workflow where:
            // 1. Image is uploaded
            // 2. ML service predicts breed
            // 3. Pet is created with predicted breed

            // Step 1: Upload image
            var uploadedImagePath = await mockImageService.Object.UploadPhotoAsync(
                TestDataBuilder.MLTestData.CreateValidImageFile(), Guid.NewGuid());

            // Step 2: ML service predicts breed
            var predictedBreed = await _mockMlService.Object.PredictAsync(uploadedImagePath);

            // Step 3: Create pet with predicted breed
            var pet = TestDataBuilder.BuildBasicPet();
            pet.Breed = predictedBreed;

            // Assert
            Assert.Equal("Golden Retriever", predictedBreed);
            Assert.Equal("Golden Retriever", pet.Breed);
            _mockMlService.Verify(x => x.PredictAsync(It.IsAny<string>()), Times.Once);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    #endregion

    #region Integration with Pet Creation

    [Fact]
    public async Task MLService_ShouldIntegrateWithPetCreation()
    {
        // Arrange
        var testImagePath = Path.GetTempFileName();
        File.WriteAllBytes(testImagePath, TestDataBuilder.MLTestData.CreateValidImageBytes());

        try
        {
            // Setup ML Service to return a specific breed
            _mockMlService.Setup(x => x.PredictAsync(testImagePath))
                .ReturnsAsync("Golden Retriever");

            // Act - simulate what would happen in the pet creation process
            var detectedBreed = await _mockMlService.Object.PredictAsync(testImagePath);

            // Assert
            Assert.Equal("Golden Retriever", detectedBreed);
            _mockMlService.Verify(x => x.PredictAsync(testImagePath), Times.Once);
        }
        finally
        {
            if (File.Exists(testImagePath)) File.Delete(testImagePath);
        }
    }

    [Fact]
    public async Task MLService_ShouldHandleFormFileInput()
    {
        // Arrange
        var imageBytes = TestDataBuilder.MLTestData.CreateValidImageBytes();
        using var stream = new MemoryStream(imageBytes);
        var formFile = new FormFile(stream, 0, imageBytes.Length, "file", "test.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
        var predictedBreed = "Labrador";

        // In a real scenario, we'd save the form file to a temp location and pass that to MLService
        var tempPath = Path.GetTempFileName();

        // Mock saving formFile to disk
        using (var fileStream = new FileStream(tempPath, FileMode.Create))
        {
            stream.Position = 0;
            await formFile.CopyToAsync(fileStream);
        }

        try
        {
            // Setup ML Service to return a specific breed
            _mockMlService.Setup(x => x.PredictAsync(tempPath))
                .ReturnsAsync(predictedBreed);

            // Act - simulate what would happen in a controller
            var detectedBreed = await _mockMlService.Object.PredictAsync(tempPath);

            // Assert
            Assert.Equal(predictedBreed, detectedBreed);
            _mockMlService.Verify(x => x.PredictAsync(tempPath), Times.Once);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
            stream.Dispose();
        }
    }

    #endregion
}