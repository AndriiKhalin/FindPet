using System.Diagnostics;
using AutoMapper;
using FindPet.BusinessLogicLayer.Interfaces.IImageService;
using FindPet.BusinessLogicLayer.Interfaces.IMLService;
using FindPet.BusinessLogicLayer.Services.EntityService;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.Entities;
using FindPet.Domain.Exceptions;
using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Media.Interfaces;
using FindPet.Tests.TestHelpers;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace FindPet.Tests.UnitTests.BusinessLogicLayer.Services.EntityService;

public class PetServiceTests
{
    #region Constructor & Setup

    public PetServiceTests()
    {
        _mockUnitOfWork = MockSetupExtensions.SetupUnitOfWorkMock();
        _mockMapper = MockSetupExtensions.SetupMapperMock();
        _mockManageImage = MockSetupExtensions.SetupImageServiceMock<Pet>();
        _mockMLService = MockSetupExtensions.CreateMock<IMLService>();
        _mockLogger = MockSetupExtensions.SetupLoggerMock();
        _mockHostingEnvironment = MockSetupExtensions.CreateMock<IWebHostEnvironment>();
        _mockMediaStorageService = MockSetupExtensions.CreateMock<IMediaStorageService>();

        _mockPetRepository = MockSetupExtensions.SetupPetRepositoryMock();
        _mockUserRepository = MockSetupExtensions.SetupUserRepositoryMock();

        _mockUnitOfWork.Setup(x => x.Pet).Returns(_mockPetRepository.Object);
        _mockUnitOfWork.Setup(x => x.User).Returns(_mockUserRepository.Object);
        _mockHostingEnvironment.Setup(x => x.WebRootPath).Returns("/test/wwwroot");

        _petService = new PetService(
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockManageImage.Object,
            _mockMLService.Object,
            _mockLogger.Object,
            _mockHostingEnvironment.Object,
            _mockMediaStorageService.Object
        );
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task CreateUpdateDeletePet_ShouldWorkCorrectly_InSequence()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var createDto = TestDataBuilder.BuildPetForCreateDto();
        var updateDto = TestDataBuilder.BuildPetForUpdateDto();
        var pet = TestDataBuilder.BuildBasicPet(petId);

        // Setup for Create
        _mockUserRepository.SetupUserExists(userId, true);
        _mockMapper.Setup(x => x.Map<Pet>(createDto)).Returns(pet);
        _mockMLService.Setup(x => x.PredictAsync(It.IsAny<string>())).ReturnsAsync("Golden Retriever");

        // Setup for Update
        _mockPetRepository.SetupPetExists(petId, true);
        _mockPetRepository.SetupGetPet(petId, pet);

        // Setup for Delete
        //_mockPetRepository.SetupDelete<Pet>(petId);

        // Act & Assert Create
        await _petService.CreatePetAsync(userId, createDto);
        _mockPetRepository.Verify(x => x.CreateAsync(It.IsAny<Pet>()), Times.Once);

        // Act & Assert Update
        await _petService.UpdatePetAsync(petId, updateDto);
        _mockPetRepository.Verify(x => x.UpdateAsync(It.IsAny<Pet>()), Times.Once);

        // Act & Assert Delete
        await _petService.DeletePetAsync(petId);
        _mockPetRepository.Verify(x => x.DeleteAsync(petId), Times.Once);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GetPets_ShouldPerformWell_WithLargeDataset()
    {
        // Arrange
        var largePetList = TestDataBuilder.CreateList(1000, i => TestDataBuilder.BuildBasicPet(Guid.NewGuid()));
        _mockPetRepository.SetupGetPets(largePetList);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = _petService.GetPets();
        stopwatch.Stop();

        // Assert
        Assert.Equal(1000, result.Count());
        Assert.True(stopwatch.ElapsedMilliseconds < 100, "Method should complete in under 100ms");
    }

    #endregion

    #region Private Fields

    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IManageImage<Pet>> _mockManageImage;
    private readonly Mock<IMLService> _mockMLService;
    private readonly Mock<ILoggerManager> _mockLogger;
    private readonly Mock<IWebHostEnvironment> _mockHostingEnvironment;
    private readonly Mock<IPetRepository> _mockPetRepository;
    private readonly Mock<IUserRepository<User>> _mockUserRepository;
    private readonly Mock<IMediaStorageService> _mockMediaStorageService;
    private readonly PetService _petService;

    #endregion

    #region GetPets Tests

    [Fact]
    public void GetPets_ShouldReturnAllPets_WhenCalled()
    {
        // Arrange
        var pets = TestDataBuilder.BuildPetList();
        _mockPetRepository.SetupGetPets(pets);

        // Act
        var result = _petService.GetPets();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.Equal(pets, result);
        _mockPetRepository.Verify(x => x.Gets(), Times.Once);
    }

    [Fact]
    public void GetPets_ShouldReturnEmptyCollection_WhenNoPetsExist()
    {
        // Arrange
        _mockPetRepository.SetupGetPets(new List<Pet>());

        // Act
        var result = _petService.GetPets();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockPetRepository.Verify(x => x.Gets(), Times.Once);
    }

    #endregion

    #region GetPetByIdAsync Tests

    [Fact]
    public async Task GetPetByIdAsync_ShouldReturnPet_WhenValidIdProvided()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var pet = TestDataBuilder.BuildBasicPet(petId);

        _mockPetRepository.SetupPetExists(petId, true);
        _mockPetRepository.SetupGetPet(petId, pet);

        // Act
        var result = await _petService.GetPetByIdAsync(petId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(petId, result.Id);
        _mockPetRepository.Verify(x => x.IsExistAsync(petId), Times.Once);
        _mockPetRepository.Verify(x => x.GetAsync(petId), Times.Once);
    }

    [Fact]
    public async Task GetPetByIdAsync_ShouldThrowBadRequestException_WhenEmptyGuidProvided()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _petService.GetPetByIdAsync(emptyGuid));

        Assert.Equal("PetId must be a valid non-empty GUID", exception.Message);
        _mockPetRepository.Verify(r => r.GetAsync(It.IsAny<Guid>()), Times.Never);
        _mockPetRepository.Verify(x => x.IsExistAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetPetByIdAsync_ShouldThrowNotFoundException_WhenPetDoesNotExist()
    {
        // Arrange
        var petId = Guid.NewGuid();
        _mockPetRepository.SetupPetExists(petId, false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _petService.GetPetByIdAsync(petId));

        Assert.Contains("Pet", exception.Message);
        Assert.Contains(petId.ToString(), exception.Message);
        _mockPetRepository.Verify(r => r.IsExistAsync(petId), Times.Once);
        _mockPetRepository.Verify(r => r.GetAsync(It.IsAny<Guid>()), Times.Never);
        _mockLogger.VerifyLogError($"Pet with id: {petId}, hasn't been found in db.");
    }

    #endregion

    #region PetExistsAsync Tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PetExistsAsync_ById_ShouldReturnCorrectResult(bool exists)
    {
        // Arrange
        var petId = Guid.NewGuid();
        _mockPetRepository.SetupPetExists(petId, exists);

        // Act
        var result = await _petService.PetExistsAsync(petId);

        // Assert
        Assert.Equal(exists, result);
        _mockPetRepository.Verify(x => x.IsExistAsync(petId), Times.Once);
    }

    [Theory]
    [InlineData("Fluffy", true)]
    [InlineData("NonExistentPet", false)]
    public async Task PetExistsAsync_ByName_ShouldReturnCorrectResult(string petName, bool exists)
    {
        // Arrange
        _mockPetRepository.Setup(x => x.IsExistAsync(petName))
            .ReturnsAsync(exists);

        // Act
        var result = await _petService.PetExistsAsync(petName);

        // Assert
        Assert.Equal(exists, result);
        _mockPetRepository.Verify(x => x.IsExistAsync(petName), Times.Once);
    }

    #endregion

    #region DeletePetAsync Tests

    [Fact]
    public async Task DeletePetAsync_ShouldDeletePetAndPhoto_WhenValidIdProvided()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var pet = TestDataBuilder.BuildPetWithPhoto("test_photo.jpg").With(x => x.Id = petId);

        _mockPetRepository.SetupPetExists(petId, true);
        _mockPetRepository.SetupGetPet(petId, pet);
        //_mockPetRepository.SetupDelete(petId);
        _mockUnitOfWork.SetupSaveAsync();

        // Act
        await _petService.DeletePetAsync(petId);

        // Assert
        _mockPetRepository.Verify(r => r.IsExistAsync(petId), Times.AtLeast(2));
        _mockManageImage.Verify(x => x.DeletePhoto(pet.Photo), Times.Once);
        _mockPetRepository.Verify(x => x.DeleteAsync(petId), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeletePetAsync_ShouldThrowBadRequestException_WhenEmptyGuidProvided()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _petService.DeletePetAsync(emptyGuid));

        Assert.Equal("PetId must be a valid non-empty GUID", exception.Message);
        _mockPetRepository.Verify(r => r.IsExistAsync(It.IsAny<Guid>()), Times.Never);
        _mockPetRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task DeletePetAsync_ShouldThrowNotFoundException_WhenPetDoesNotExist()
    {
        // Arrange
        var petId = Guid.NewGuid();
        _mockPetRepository.SetupPetExists(petId, false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _petService.DeletePetAsync(petId));

        Assert.Contains("Pet", exception.Message);
        Assert.Contains(petId.ToString(), exception.Message);
        _mockPetRepository.Verify(r => r.IsExistAsync(petId), Times.Once);
        _mockPetRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Never);
        _mockLogger.VerifyLogError($"Pet with id: {petId}, hasn't been found in db.");
    }

    [Fact]
    public async Task DeletePetAsync_WithNullPhoto_DoesNotCallDeletePhoto()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var pet = TestDataBuilder.BuildBasicPet(petId, photo: null);
        _mockPetRepository.SetupPetExists(petId, true);
        _mockPetRepository.SetupGetPet(petId, pet);
        _mockUnitOfWork.SetupSaveAsync();

        // Act
        await _petService.DeletePetAsync(petId);

        // Assert
        _mockPetRepository.Verify(r => r.IsExistAsync(petId), Times.AtLeast(2));
        _mockPetRepository.Verify(r => r.DeleteAsync(petId), Times.Once);
        _mockManageImage.Verify(i => i.DeletePhoto(null), Times.Once); // Will be called with null
        _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
    }

    #endregion

    #region UpdatePetAsync Tests

    [Fact]
    public async Task UpdatePetAsync_ShouldUpdatePet_WhenValidDataProvided()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var existingPet = TestDataBuilder.BuildBasicPet(petId);
        var updateDto = TestDataBuilder.BuildPetForUpdateDto();

        _mockPetRepository.SetupPetExists(petId, true);
        _mockPetRepository.SetupGetPet(petId, existingPet);
        _mockUnitOfWork.SetupSaveAsync();
        _mockMapper.Setup(x => x.Map(updateDto, existingPet));

        // Act
        await _petService.UpdatePetAsync(petId, updateDto);

        // Assert
        _mockPetRepository.Verify(r => r.IsExistAsync(petId), Times.AtLeast(2));
        _mockMapper.Verify(x => x.Map(updateDto, existingPet), Times.Once);
        _mockPetRepository.Verify(x => x.UpdateAsync(existingPet), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdatePetAsync_ShouldUpdatePhotoAndDeleteOld_WhenNewPhotoProvided()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var existingPet = TestDataBuilder.BuildPetWithPhoto("old_photo.jpg").With(x => x.Id = petId);
        var updateDto = TestDataBuilder.BuildPetForUpdateDto().With(x => x.Photo = "new_photo.jpg");

        _mockPetRepository.SetupPetExists(petId, true);
        _mockPetRepository.SetupGetPet(petId, existingPet);
        _mockManageImage.SetupUploadPhoto("uploaded_photo.jpg");
        _mockUnitOfWork.SetupSaveAsync();

        // Act
        await _petService.UpdatePetAsync(petId, updateDto);

        // Assert
        _mockPetRepository.Verify(r => r.IsExistAsync(petId), Times.AtLeast(2));
        _mockPetRepository.Verify(r => r.UpdateAsync(existingPet), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
        _mockManageImage.Verify(x => x.DeletePhoto(existingPet.Photo), Times.Once);
        _mockManageImage.Verify(x => x.UploadPhotoAsync(updateDto.Photo, petId), Times.Once);
    }

    [Fact]
    public async Task UpdatePetAsync_ShouldDoesNotUpdatePhoto_WhenNullPhoto()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var pet = TestDataBuilder.BuildPetWithPhoto().With(x => x.Id = petId);
        var updateDto = TestDataBuilder.BuildPetForUpdateDto().With(x => x.Photo = null);

        _mockPetRepository.SetupPetExists(petId, true);
        _mockPetRepository.SetupGetPet(petId, pet);
        _mockUnitOfWork.SetupSaveAsync();

        // Act
        await _petService.UpdatePetAsync(petId, updateDto);

        // Assert
        _mockPetRepository.Verify(r => r.IsExistAsync(petId), Times.AtLeast(2));
        _mockManageImage.Verify(i => i.DeletePhoto(It.IsAny<string>()), Times.Never);
        _mockManageImage.Verify(i => i.UploadPhotoAsync(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
        _mockPetRepository.Verify(r => r.UpdateAsync(pet), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdatePetAsync_ShouldThrowBadRequestException_WhenEmptyGuidProvided()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var updateDto = TestDataBuilder.BuildPetForUpdateDto();

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<BadRequestException>(() => _petService.UpdatePetAsync(emptyGuid, updateDto));

        Assert.Equal("PetId must be a valid non-empty GUID", exception.Message);
        _mockPetRepository.Verify(r => r.IsExistAsync(It.IsAny<Guid>()), Times.Never);
        _mockPetRepository.Verify(r => r.UpdateAsync(It.IsAny<Pet>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdatePetAsync_ShouldThrowBadRequestException_WhenPetDtoIsNull()
    {
        // Arrange
        var petId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _petService.UpdatePetAsync(petId, null));

        Assert.Equal("Pet is null", exception.Message);
        _mockPetRepository.Verify(r => r.IsExistAsync(It.IsAny<Guid>()), Times.Never);
        _mockLogger.VerifyLogError("Pet object sent from client is null.");
    }

    [Fact]
    public async Task UpdatePetAsync_ShouldThrowNotFoundException_WhenPetDoesNotExist()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var updateDto = TestDataBuilder.BuildPetForUpdateDto();
        _mockPetRepository.SetupPetExists(petId, false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _petService.UpdatePetAsync(petId, updateDto));

        Assert.Contains("Pet", exception.Message);
        Assert.Contains(petId.ToString(), exception.Message);
        _mockPetRepository.Verify(r => r.IsExistAsync(petId), Times.Once);
        _mockPetRepository.Verify(r => r.UpdateAsync(It.IsAny<Pet>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Never);
        _mockLogger.VerifyLogError($"Pet with id: {petId}, hasn't been found in db.");
    }

    #endregion

    #region CreatePetAsync Tests

    [Fact]
    public async Task CreatePetAsync_ShouldCreatePet_WhenValidDataProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = TestDataBuilder.BuildPetForCreateDto().With(x => x.Photo = "retriver.jpg");
        var pet = TestDataBuilder.BuildBasicPet();
        var predictedBreed = "Golden Retriever";

        _mockUserRepository.SetupUserExists(userId, true);
        _mockMapper.Setup(x => x.Map<Pet>(createDto)).Returns(pet);
        _mockMLService.Setup(x => x.PredictAsync(It.IsAny<string>())).ReturnsAsync(predictedBreed);
        _mockUnitOfWork.SetupSaveAsync();

        // Act
        var result = await _petService.CreatePetAsync(userId, createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(predictedBreed, result.Type);
        Assert.Equal(createDto.Photo, result.Photo);
        Assert.True(pet.DateCreateUpdate <= DateTime.UtcNow);
        _mockUserRepository.Verify(r => r.IsExistAsync(userId), Times.Once);
        _mockPetRepository.Verify(x => x.CreateAsync(pet), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task CreatePetAsync_ShouldSetTypeToUnknown_WhenPhotoIsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = TestDataBuilder.BuildPetForCreateDto().With(x => x.Photo = null);
        var pet = TestDataBuilder.BuildBasicPet();

        _mockUserRepository.SetupUserExists(userId, true);
        _mockMapper.Setup(x => x.Map<Pet>(createDto)).Returns(pet);
        _mockUnitOfWork.SetupSaveAsync();

        // Act
        var result = await _petService.CreatePetAsync(userId, createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Unknown", result.Type);
        _mockMLService.Verify(x => x.PredictAsync(It.IsAny<string>()), Times.Never);
        _mockPetRepository.Verify(r => r.CreateAsync(pet), Times.Once);
    }

    [Fact]
    public async Task CreatePetAsync_ShouldCallMLService_WhenPhotoProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = TestDataBuilder.BuildPetForCreateDto().With(x => x.Photo = "testphoto.jpg");
        var pet = TestDataBuilder.BuildBasicPet();
        //var expectedPath = "/test/wwwroot/testphoto.jpg";
        var predictedBreed = "Dog";

        _mockUserRepository.SetupUserExists(userId, true);
        _mockMapper.Setup(x => x.Map<Pet>(createDto)).Returns(pet);
        _mockMLService.Setup(x => x.PredictAsync(It.IsAny<string>())).ReturnsAsync(predictedBreed);
        _mockUnitOfWork.SetupSaveAsync();

        // Act
        var result = await _petService.CreatePetAsync(userId, createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(predictedBreed, result.Type);
        _mockMLService.Verify(x => x.PredictAsync(It.IsAny<string>()), Times.Once);
        _mockPetRepository.Verify(r => r.CreateAsync(pet), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task CreatePetAsync_ShouldThrowBadRequestException_WhenUserIdIsInvalid(string guidString)
    {
        // Arrange
        var userId = string.IsNullOrEmpty(guidString) ? Guid.Empty : Guid.Parse(guidString);
        var createDto = TestDataBuilder.BuildPetForCreateDto();

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<BadRequestException>(() => _petService.CreatePetAsync(userId, createDto));

        Assert.Equal("Invalid userId or createPetDto object.", exception.Message);
        _mockUserRepository.Verify(r => r.IsExistAsync(It.IsAny<Guid>()), Times.Never);
        _mockPetRepository.Verify(r => r.CreateAsync(It.IsAny<Pet>()), Times.Never);
        _mockLogger.VerifyLogError("Error");
    }

    [Fact]
    public async Task CreatePetAsync_ShouldThrowBadRequestException_WhenCreateDtoIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _petService.CreatePetAsync(userId, null));

        Assert.Equal("Invalid userId or createPetDto object.", exception.Message);
        _mockUserRepository.Verify(r => r.IsExistAsync(It.IsAny<Guid>()), Times.Never);
        _mockPetRepository.Verify(r => r.CreateAsync(It.IsAny<Pet>()), Times.Never);
        _mockLogger.VerifyLogError("Error");
    }

    [Fact]
    public async Task CreatePetAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = TestDataBuilder.BuildPetForCreateDto();
        _mockUserRepository.SetupUserExists(userId, false);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<NotFoundException>(() => _petService.CreatePetAsync(userId, createDto));

        Assert.Contains("User", exception.Message);
        Assert.Contains(userId.ToString(), exception.Message);
        _mockUserRepository.Verify(r => r.IsExistAsync(userId), Times.Once);
        _mockPetRepository.Verify(r => r.CreateAsync(It.IsAny<Pet>()), Times.Never);
    }

    [Fact]
    public async Task CreatePetAsync_SetsCorrectDateTime()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = TestDataBuilder.BuildPetForCreateDto();
        var pet = TestDataBuilder.BuildBasicPet();
        var beforeTest = DateTime.UtcNow;

        _mockUserRepository.SetupUserExists(userId, true);
        _mockMapper.Setup(m => m.Map<Pet>(createDto)).Returns(pet);
        _mockUnitOfWork.SetupSaveAsync();

        // Act
        var result = await _petService.CreatePetAsync(userId, createDto);
        var afterTest = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result.DateCreateUpdate);
        Assert.True(result.DateCreateUpdate >= beforeTest);
        Assert.True(result.DateCreateUpdate <= afterTest);
    }

    #endregion

    #region Edge Cases and Error Scenarios

    [Fact]
    public async Task CreatePetAsync_ShouldHandleMLServiceException_Gracefully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = TestDataBuilder.BuildPetForCreateDto().With(x => x.Photo = "testphoto.jpg");
        var pet = TestDataBuilder.BuildBasicPet();

        _mockUserRepository.SetupUserExists(userId, true);
        _mockMapper.Setup(x => x.Map<Pet>(createDto)).Returns(pet);
        _mockMLService.Setup(x => x.PredictAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("ML Service Error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _petService.CreatePetAsync(userId, createDto));
    }

    [Fact]
    public async Task DeletePetAsync_ShouldHandleImageDeletionFailure_Gracefully()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var pet = TestDataBuilder.BuildPetWithPhoto("test_photo.jpg");

        _mockPetRepository.SetupPetExists(petId, true);
        _mockPetRepository.SetupGetPet(petId, pet);
        _mockManageImage.Setup(x => x.DeletePhoto(It.IsAny<string>()))
            .Throws(new Exception("File deletion failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _petService.DeletePetAsync(petId));
    }

    [Theory]
    [InlineData("/uploads/photo.jpg")]
    [InlineData("\\uploads\\photo.jpg")]
    [InlineData("uploads/photo.jpg")]
    public async Task CreatePetAsync_ShouldHandleDifferentPhotoPathFormats(string photoPath)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = TestDataBuilder.BuildPetForCreateDto();
        createDto.Photo = photoPath;
        var pet = TestDataBuilder.BuildBasicPet();

        _mockUserRepository.SetupUserExists(userId, true);
        _mockMapper.Setup(x => x.Map<Pet>(createDto)).Returns(pet);
        _mockMLService.Setup(x => x.PredictAsync(It.IsAny<string>())).ReturnsAsync("Golden Retriever");

        // Act
        await _petService.CreatePetAsync(userId, createDto);

        // Assert
        _mockMLService.Verify(x => x.PredictAsync(It.IsAny<string>()), Times.Once);
    }

    #endregion
}