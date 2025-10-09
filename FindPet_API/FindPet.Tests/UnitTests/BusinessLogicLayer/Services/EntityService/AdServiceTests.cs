using AutoMapper;
using FindPet.BusinessLogicLayer.Interfaces.IImageService;
using FindPet.BusinessLogicLayer.Interfaces.ILoggerService;
using FindPet.BusinessLogicLayer.Services.EntityService;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;
using FindPet.Domain.Entities;
using FindPet.Domain.Exceptions;
using FindPet.Media.Interfaces;
using FindPet.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace FindPet.Tests.UnitTests.BusinessLogicLayer.Services.EntityService;

public class AdServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IManageImage<Ad>> _mockImageService;
    private readonly Mock<ILoggerManager> _mockLogger;
    private readonly AdService _adService;
    private readonly IMediaStorageService _mediaStorageService;


    // Repository mocks
    private readonly Mock<IAdRepository> _mockAdRepository;
    private readonly Mock<IPetRepository> _mockPetRepository;
    private readonly Mock<IUserRepository<User>> _mockUserRepository;

    public AdServiceTests()
    {
        _mockUnitOfWork = MockSetupExtensions.SetupUnitOfWorkMock();
        _mockMapper = MockSetupExtensions.SetupMapperMock();
        _mockImageService = MockSetupExtensions.SetupImageServiceMock<Ad>();
        _mockLogger = MockSetupExtensions.SetupLoggerMock();
        _mediaStorageService = MockSetupExtensions.CreateMock<IMediaStorageService>().Object;

        // Setup repository mocks
        _mockAdRepository = new Mock<IAdRepository>();
        _mockPetRepository = new Mock<IPetRepository>();
        _mockUserRepository = new Mock<IUserRepository<User>>();

        // Setup UnitOfWork properties
        _mockUnitOfWork.Setup(x => x.Ad).Returns(_mockAdRepository.Object);
        _mockUnitOfWork.Setup(x => x.Pet).Returns(_mockPetRepository.Object);
        _mockUnitOfWork.Setup(x => x.User).Returns(_mockUserRepository.Object);

        _adService = new AdService(_mockUnitOfWork.Object, _mockMapper.Object, _mockImageService.Object, _mockLogger.Object, _mediaStorageService);
    }

    #region GetAds Tests

    [Fact]
    public void GetAds_ShouldReturnAllAds()
    {
        // Arrange
        var expectedAds = TestDataBuilder.BuildAdList(3);
        _mockAdRepository.Setup(x => x.Gets()).Returns(expectedAds);

        // Act
        var result = _adService.GetAds();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAds.Count, result.Count());
        Assert.Equal(expectedAds, result);
        _mockAdRepository.Verify(x => x.Gets(), Times.Once);
    }

    [Fact]
    public void GetAds_ShouldReturnEmptyList_WhenNoAdsExist()
    {
        // Arrange
        var emptyAdList = new List<Ad>();
        _mockAdRepository.Setup(x => x.Gets()).Returns(emptyAdList);

        // Act
        var result = _adService.GetAds();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockAdRepository.Verify(x => x.Gets(), Times.Once);
    }

    #endregion

    #region GetAdAsync Tests

    [Fact]
    public async Task GetAdAsync_WithValidId_ShouldReturnAd()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var expectedAd = TestDataBuilder.BuildAd(adId);

        _mockAdRepository.Setup(x => x.IsExistAsync(adId)).ReturnsAsync(true);
        _mockAdRepository.Setup(x => x.GetAsync(adId)).ReturnsAsync(expectedAd);

        // Act
        var result = await _adService.GetAdAsync(adId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAd, result);
        Assert.Equal(adId, result.Id);
        _mockAdRepository.Verify(x => x.IsExistAsync(adId), Times.Once);
        _mockAdRepository.Verify(x => x.GetAsync(adId), Times.Once);
    }

    [Fact]
    public async Task GetAdAsync_WithEmptyGuid_ShouldThrowBadRequestException()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _adService.GetAdAsync(emptyId));

        Assert.Equal("AdId must be a valid non-empty GUID", exception.Message);
        _mockAdRepository.Verify(x => x.IsExistAsync(It.IsAny<Guid>()), Times.Never);
        _mockAdRepository.Verify(x => x.GetAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetAdAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var adId = Guid.NewGuid();
        _mockAdRepository.Setup(x => x.IsExistAsync(adId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _adService.GetAdAsync(adId));
        //Assert.Equal("Ad", exception.Name);
        //Assert.Equal(adId, exception.Key);

        Assert.Equal($"Ad with ID '{adId}' was not found", exception.Message);
        _mockLogger.VerifyLogError($"Ad with id: {adId}, hasn't been found in db.");
        _mockAdRepository.Verify(x => x.IsExistAsync(adId), Times.Once);
        _mockAdRepository.Verify(x => x.GetAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region AdExistsAsync Tests

    [Fact]
    public async Task AdExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var adId = Guid.NewGuid();
        _mockAdRepository.Setup(x => x.IsExistAsync(adId)).ReturnsAsync(true);

        // Act
        var result = await _adService.AdExistsAsync(adId);

        // Assert
        Assert.True(result);
        _mockAdRepository.Verify(x => x.IsExistAsync(adId), Times.Once);
    }

    [Fact]
    public async Task AdExistsAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        var adId = Guid.NewGuid();
        _mockAdRepository.Setup(x => x.IsExistAsync(adId)).ReturnsAsync(false);

        // Act
        var result = await _adService.AdExistsAsync(adId);

        // Assert
        Assert.False(result);
        _mockAdRepository.Verify(x => x.IsExistAsync(adId), Times.Once);
    }

    #endregion

    #region DeleteAdAsync Tests

    [Fact]
    public async Task DeleteAdAsync_WithValidId_ShouldDeleteAdAndPhoto()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var existingAd = TestDataBuilder.BuildAd(adId, photo: "test-photo.jpg");

        _mockAdRepository.Setup(x => x.IsExistAsync(adId)).ReturnsAsync(true);
        _mockAdRepository.Setup(x => x.GetAsync(adId)).ReturnsAsync(existingAd);
        _mockAdRepository.Setup(x => x.DeleteAsync(adId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        await _adService.DeleteAdAsync(adId);

        // Assert
        _mockAdRepository.Verify(x => x.IsExistAsync(adId), Times.AtLeast(2));
        _mockAdRepository.Verify(x => x.GetAsync(adId), Times.Once);
        _mockImageService.Verify(x => x.DeletePhoto(existingAd.Photo), Times.Once);
        _mockAdRepository.Verify(x => x.DeleteAsync(adId), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAdAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var adId = Guid.NewGuid();
        _mockAdRepository.Setup(x => x.IsExistAsync(adId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _adService.DeleteAdAsync(adId));

        Assert.Equal($"Ad with ID '{adId}' was not found", exception.Message);
        _mockLogger.VerifyLogError($"Ad with id: {adId}, hasn't been found in db.");
        _mockAdRepository.Verify(x => x.IsExistAsync(adId), Times.Once);
        _mockAdRepository.Verify(x => x.GetAsync(adId), Times.Never);
        _mockAdRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Never);
    }

    #endregion

    #region UpdateAdAsync Tests

    [Fact]
    public async Task UpdateAdAsync_WithValidData_ShouldUpdateAd()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var existingAd = TestDataBuilder.BuildAd(adId, photo: "old-photo.jpg");
        var updateDto = TestDataBuilder.BuildAdForUpdateDto();
        var mockFile = TestDataBuilder.MLTestData.CreateValidImageFile();
        updateDto.Photo = mockFile;

        _mockAdRepository.Setup(x => x.IsExistAsync(adId)).ReturnsAsync(true);
        _mockAdRepository.Setup(x => x.GetAsync(adId)).ReturnsAsync(existingAd);
        _mockImageService.Setup(x => x.UploadPhotoAsync(updateDto.Photo, adId)).ReturnsAsync("new-photo.jpg");
        _mockMapper.Setup(x => x.Map(updateDto, existingAd));
        //_mockAdRepository.SetupUpdate();
        _mockAdRepository.Setup(x => x.UpdateAsync(existingAd)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        await _adService.UpdateAdAsync(adId, updateDto);

        // Assert
        _mockAdRepository.Verify(x => x.IsExistAsync(adId), Times.AtLeast(2));
        _mockAdRepository.Verify(x => x.GetAsync(adId), Times.Once);
        _mockImageService.Verify(x => x.DeletePhoto("old-photo.jpg"), Times.Once);
        _mockImageService.Verify(x => x.UploadPhotoAsync(updateDto.Photo, adId), Times.Once);
        _mockMapper.Verify(x => x.Map(updateDto, existingAd), Times.Once);
        _mockAdRepository.Verify(x => x.UpdateAsync(existingAd), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAdAsync_WithNullAd_ShouldThrowBadRequestException()
    {
        // Arrange
        var adId = Guid.NewGuid();
        AdForUpdateDto nullAd = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _adService.UpdateAdAsync(adId, nullAd));

        Assert.Equal("Ad is null", exception.Message);
        _mockLogger.VerifyLogError("Ad object sent from client is null.");
        _mockAdRepository.Verify(x => x.IsExistAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAdAsync_WithNonExistentAd_ShouldThrowNotFoundException()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var updateDto = TestDataBuilder.BuildAdForUpdateDto();
        _mockAdRepository.Setup(x => x.IsExistAsync(adId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _adService.UpdateAdAsync(adId, updateDto));

        Assert.Equal($"Ad with ID '{adId}' was not found", exception.Message);
        _mockLogger.VerifyLogError($"Ad with id: {adId}, hasn't been found in db."); ;
        _mockAdRepository.Verify(x => x.IsExistAsync(adId), Times.Once);
        _mockAdRepository.Verify(x => x.GetAsync(adId), Times.Never);
    }

    [Fact]
    public async Task UpdateAdAsync_WithEmptyId_ShouldThrowBadRequestException()
    {
        // Arrange
        var adId = Guid.Empty;
        var updateDto = TestDataBuilder.BuildAdForUpdateDto();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _adService.UpdateAdAsync(adId, updateDto));

        Assert.Equal("AdId must be a valid non-empty GUID", exception.Message);
        _mockLogger.VerifyLogError("AdId is empty.");
    }

    #endregion

    #region CreateAdAsync Tests

    [Fact]
    public async Task CreateAdAsync_WithValidData_ShouldCreateAd()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var createDto = TestDataBuilder.BuildAdForCreateDto().With(x => x.Photo = TestDataBuilder.MLTestData.CreateValidImageFile());
        var mappedAd = TestDataBuilder.BuildAd();


        var pet = TestDataBuilder.BuildBasicPet(petId);
        var user = TestDataBuilder.BuildBasicUser(userId);
        var newAdId = Guid.NewGuid();
        var photoPath = "uploaded-photo.jpg";

        _mockPetRepository.Setup(x => x.GetAsync(petId)).ReturnsAsync(pet);
        _mockUserRepository.Setup(x => x.GetAsync(userId)).ReturnsAsync(user);
        _mockMapper.Setup(x => x.Map<Ad>(createDto)).Returns(mappedAd);
        _mockImageService.Setup(x => x.UploadPhotoAsync(createDto.Photo, mappedAd.Id)).ReturnsAsync(photoPath);
        _mockAdRepository.Setup(x => x.CreateAsync(mappedAd)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _adService.CreateAdAsync(petId, userId, createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(petId, result.PetId);
        Assert.Equal(photoPath, result.Photo);
        Assert.True(result.DateCreateUpdate > DateTime.MinValue);

        _mockPetRepository.Verify(x => x.GetAsync(petId), Times.Once);
        _mockUserRepository.Verify(x => x.GetAsync(userId), Times.Once);
        _mockMapper.Verify(x => x.Map<Ad>(createDto), Times.Once);
        _mockImageService.Verify(x => x.UploadPhotoAsync(createDto.Photo, mappedAd.Id), Times.Once);
        _mockAdRepository.Verify(x => x.CreateAsync(mappedAd), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Theory]
    [InlineData(true, true, false)]   // petId valid, userId valid, dto null
    [InlineData(true, false, true)]   // petId valid, userId empty, dto valid
    [InlineData(false, true, true)]   // petId empty, userId valid, dto valid
    public async Task CreateAdAsync_WithInvalidInput_ShouldThrowArgumentNullException(
        bool isPetIdValid, bool isUserIdValid, bool isDtoValid)
    {
        // Arrange
        var petId = isPetIdValid ? Guid.NewGuid() : Guid.Empty;
        var userId = isUserIdValid ? Guid.NewGuid() : Guid.Empty;
        var createDto = isDtoValid ? TestDataBuilder.BuildAdForCreateDto() : null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _adService.CreateAdAsync(petId, userId, createDto));

        Assert.Equal("Invalid petId,userId or ad object.", exception.Message);
        _mockLogger.VerifyLogError("Invalid petId,userId or ad object.");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task CreateAdAsync_ShouldSetCorrectDateTimeUtc()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var createDto = TestDataBuilder.BuildAdForCreateDto();
        var pet = TestDataBuilder.BuildBasicPet(petId);
        var user = TestDataBuilder.BuildBasicUser(userId);
        var beforeCreate = DateTime.UtcNow;

        _mockPetRepository.Setup(x => x.GetAsync(petId)).ReturnsAsync(pet);
        _mockUserRepository.Setup(x => x.GetAsync(userId)).ReturnsAsync(user);
        _mockMapper.Setup(x => x.Map<Ad>(createDto)).Returns(new Ad());
        _mockImageService.Setup(x => x.UploadPhotoAsync(It.IsAny<IFormFile>(), It.IsAny<Guid>())).ReturnsAsync("photo.jpg");
        _mockAdRepository.Setup(x => x.CreateAsync(It.IsAny<Ad>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _adService.CreateAdAsync(petId, userId, createDto);
        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.True(result.DateCreateUpdate >= beforeCreate);
        Assert.True(result.DateCreateUpdate <= afterCreate);
        Assert.Equal(DateTimeKind.Utc, result.DateCreateUpdate.Value.Kind);
    }

    [Fact]
    public async Task UpdateAndDeleteWorkflow_ShouldHandlePhotoCorrectly()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var originalPhoto = "original-photo.jpg";
        var newPhoto = "new-photo.jpg";
        var existingAd = TestDataBuilder.BuildAd(adId, photo: originalPhoto);
        var updateDto = TestDataBuilder.BuildAdForUpdateDto().With(x => x.Photo = TestDataBuilder.MLTestData.CreateValidImageFile());

        // Setup for update
        _mockAdRepository.Setup(x => x.IsExistAsync(adId)).ReturnsAsync(true);
        _mockAdRepository.Setup(x => x.GetAsync(adId)).ReturnsAsync(existingAd);
        _mockImageService.Setup(x => x.UploadPhotoAsync(updateDto.Photo, adId)).ReturnsAsync(newPhoto);
        _mockAdRepository.Setup(x => x.UpdateAsync(existingAd)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        // Act - Update
        await _adService.UpdateAdAsync(adId, updateDto);

        // Setup for delete
        existingAd.Photo = newPhoto; // Simulate photo update
        _mockAdRepository.Setup(x => x.DeleteAsync(adId)).Returns(Task.CompletedTask);

        // Act - Delete
        await _adService.DeleteAdAsync(adId);

        // Assert
        _mockImageService.Verify(x => x.DeletePhoto(originalPhoto), Times.Once, "Original photo should be deleted during update");
        _mockImageService.Verify(x => x.UploadPhotoAsync(updateDto.Photo, adId), Times.Once, "New photo should be uploaded during update");
        _mockImageService.Verify(x => x.DeletePhoto(newPhoto), Times.Once, "New photo should be deleted during ad deletion");
    }

    #endregion

    #region Edge Cases and Error Scenarios

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task GetAdAsync_WithGuidFormats_ShouldHandleCorrectly(string guidString)
    {
        // Arrange
        var adId = Guid.Parse(guidString);

        // Act & Assert
        if (adId == Guid.Empty)
        {
            await Assert.ThrowsAsync<BadRequestException>(() => _adService.GetAdAsync(adId));
        }
    }

    [Fact]
    public async Task AdService_ShouldHandleConcurrentOperations()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var ad = TestDataBuilder.BuildAd(adId);

        _mockAdRepository.Setup(x => x.IsExistAsync(adId)).ReturnsAsync(true);
        _mockAdRepository.Setup(x => x.GetAsync(adId)).ReturnsAsync(ad);

        // Act - Simulate concurrent reads
        var tasks = new List<Task<Ad?>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_adService.GetAdAsync(adId));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, result => Assert.Equal(ad, result));
        _mockAdRepository.Verify(x => x.GetAsync(adId), Times.Exactly(10));
    }

    #endregion
}