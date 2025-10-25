using System.Linq.Expressions;
using AutoMapper;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.BusinessLogicLayer.Interfaces.IImageService;
using FindPet.BusinessLogicLayer.Services.EntityService;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.Entities;
using FindPet.Domain.Exceptions;
using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Media.Interfaces;
using FindPet.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace FindPet.Tests.UnitTests.BusinessLogicLayer.Services.EntityService;

public class UserServiceTests
{
    private readonly Mock<IManageImage<User>> _imageServiceMock;
    private readonly Mock<ILoggerManager> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMediaStorageService> _mediaStorageServer;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository<User>> _userRepositoryMock;
    private readonly IUserService _userService;

    public UserServiceTests()
    {
        // Setup runs for each test via constructor in xUnit
        _unitOfWorkMock = MockSetupExtensions.SetupUnitOfWorkMock();
        _userRepositoryMock = MockSetupExtensions.SetupUserRepositoryMock();
        _mapperMock = MockSetupExtensions.SetupMapperMock();
        _imageServiceMock = MockSetupExtensions.SetupImageServiceMock();
        _loggerMock = MockSetupExtensions.SetupLoggerMock();
        _mediaStorageServer = MockSetupExtensions.CreateMock<IMediaStorageService>();

        _unitOfWorkMock.Setup(x => x.User).Returns(_userRepositoryMock.Object);

        _userService = new UserService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _mediaStorageServer.Object);
    }

    #region GetUsers Tests

    [Fact]
    public void GetUsers_WhenCalled_ShouldReturnAllUsers()
    {
        // Arrange
        var expectedUsers = TestDataBuilder.BuildUserList();
        _userRepositoryMock.SetupGetUsers(expectedUsers);

        // Act
        var result = _userService.GetUsers();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(expectedUsers);
        _userRepositoryMock.Verify(x => x.Gets(), Times.Once);
    }

    [Fact]
    public void GetUsers_WhenNoUsers_ShouldReturnEmptyCollection()
    {
        // Arrange
        var emptyUsers = new List<User>();
        _userRepositoryMock.SetupGetUsers(emptyUsers);

        // Act
        var result = _userService.GetUsers();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = TestDataBuilder.BuildBasicUser(userId);

        _userRepositoryMock.SetupUserExists(userId, true);
        _userRepositoryMock.SetupGetUser(userId, expectedUser);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedUser);
        _userRepositoryMock.Verify(x => x.IsExistAsync(userId), Times.Once);
        _userRepositoryMock.Verify(x => x.GetAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithEmptyGuid_ShouldThrowBadRequestException()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _userService.GetUserByIdAsync(emptyId));

        exception.Message.Should().Be("User ID must be NON-Empty");
        _userRepositoryMock.Verify(x => x.IsExistAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.SetupUserExists(userId, false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _userService.GetUserByIdAsync(userId));

        exception.Should().NotBeNull();
        _loggerMock.VerifyLogError($"User with id: {userId}, hasn't been found in db.");
        _userRepositoryMock.Verify(x => x.GetAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region GetUserByNameAsync Tests

    [Fact]
    public async Task GetUserByNameAsync_WithValidName_ShouldReturnUser()
    {
        // Arrange
        var userName = "testuser";
        var expectedUser = TestDataBuilder.BuildBasicUser();
        expectedUser.Name = userName;

        _userRepositoryMock.SetupUserExists(userName, true);
        _userRepositoryMock.SetupGetUserByName(userName, expectedUser);

        // Act
        var result = await _userService.GetUserByNameAsync(userName);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(userName);
        _userRepositoryMock.Verify(x => x.IsExistAsync(userName), Times.Once);
        _userRepositoryMock.Verify(x => x.GetUserAsync(userName), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetUserByNameAsync_WithInvalidName_ShouldThrowBadRequestException(string invalidName)
    {
        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<BadRequestException>(() => _userService.GetUserByNameAsync(invalidName));

        exception.Message.Should().Be("UserName cannot be empty");
        _userRepositoryMock.Verify(x => x.IsExistAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetUserByNameAsync_WithNonExistentName_ShouldThrowNotFoundException()
    {
        // Arrange
        var userName = "nonexistent";
        _userRepositoryMock.SetupUserExists(userName, false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _userService.GetUserByNameAsync(userName));

        exception.Should().NotBeNull();
        _loggerMock.VerifyLogError($"User with name: {userName}, hasn't been found in db.");
        _userRepositoryMock.Verify(x => x.GetUserAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Email and Phone Validation Tests

    [Fact]
    public async Task IsEmailRegisteredAsync_WithExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        _userRepositoryMock.SetupEmailExists(email, true);

        // Act
        var result = await _userService.IsEmailRegisteredAsync(email);

        // Assert
        result.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
    }

    [Fact]
    public async Task IsEmailRegisteredAsync_WithNonExistingEmail_ShouldReturnFalse()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _userRepositoryMock.SetupEmailExists(email, false);

        // Act
        var result = await _userService.IsEmailRegisteredAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsPhoneNumberRegisteredAsync_WithExistingPhone_ShouldReturnTrue()
    {
        // Arrange
        var phone = "+1234567890";
        _userRepositoryMock.SetupPhoneExists(phone, true);

        // Act
        var result = await _userService.IsPhoneNumberRegisteredAsync(phone);

        // Assert
        result.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
    }

    [Fact]
    public async Task IsPhoneNumberRegisteredAsync_WithNonExistingPhone_ShouldReturnFalse()
    {
        // Arrange
        var phone = "+9999999999";
        _userRepositoryMock.SetupPhoneExists(phone, false);

        // Act
        var result = await _userService.IsPhoneNumberRegisteredAsync(phone);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserExists Tests

    [Fact]
    public async Task UserExistsAsync_WithGuid_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.SetupUserExists(userId, true);

        // Act
        var result = await _userService.UserExistsAsync(userId);

        // Assert
        result.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.IsExistAsync(userId), Times.Once);
    }

    [Fact]
    public async Task UserExistsAsync_WithString_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        var userName = "testuser";
        _userRepositoryMock.SetupUserExists(userName, true);

        // Act
        var result = await _userService.UserExistsAsync(userName);

        // Assert
        result.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.IsExistAsync(userName), Times.Once);
    }

    #endregion

    #region DeleteUserAsync Tests

    [Fact]
    public async Task DeleteUserAsync_WithValidId_ShouldDeleteUserAndPhoto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = TestDataBuilder.BuildBasicUser(userId);

        _userRepositoryMock.SetupUserExists(userId, true);
        _userRepositoryMock.SetupGetUser(userId, user);

        // Act
        await _userService.DeleteUserAsync(userId);

        // Assert
        _imageServiceMock.VerifyImageDelete(user.Photo);
        _userRepositoryMock.Verify(x => x.DeleteAsync(userId), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.SetupUserExists(userId, false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _userService.DeleteUserAsync(userId));

        exception.Message.Should().Contain($"User with ID '{userId}' was not found");
        _loggerMock.VerifyLogError($"User with id: {userId}, hasn't been found in db.");
        _userRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    public async Task UpdateUserAsync_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = TestDataBuilder.BuildUserForUpdateDto();
        var existingUser = TestDataBuilder.BuildBasicUser(userId);

        _userRepositoryMock.SetupUserExists(userId, true);
        _userRepositoryMock.SetupGetUser(userId, existingUser);

        // Act
        await _userService.UpdateUserAsync(userId, updateDto);

        // Assert
        _mapperMock.Verify(x => x.Map(updateDto, existingUser), Times.Once);
        _userRepositoryMock.Verify(x => x.UpdateAsync(existingUser), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WithPhoto_ShouldDeleteOldAndUploadNew()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = TestDataBuilder.BuildUserForUpdateDtoWithPhoto();
        var existingUser = TestDataBuilder.BuildUserWithPhoto().With(u => u.Id = userId);

        _userRepositoryMock.SetupUserExists(userId, true);
        _userRepositoryMock.SetupGetUser(userId, existingUser);

        // Act
        await _userService.UpdateUserAsync(userId, updateDto);

        // Assert
        _imageServiceMock.VerifyImageDelete(existingUser.Photo);
        _imageServiceMock.VerifyImageUpload(updateDto.Photo, userId);
    }

    [Fact]
    public async Task UpdateUserAsync_WithNullUser_ShouldThrowBadRequestException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _userService.UpdateUserAsync(userId, null));

        exception.Message.Should().Contain("User is null");
        _loggerMock.Verify(x => x.LogError(
                "User object sent from client is null.",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = TestDataBuilder.BuildUserForUpdateDto();
        _userRepositoryMock.SetupUserExists(userId, false);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<NotFoundException>(() => _userService.UpdateUserAsync(userId, updateDto));

        exception.Message.Should().Contain($"User with ID '{userId}' was not found");
        _loggerMock.VerifyLogError($"User with id: {userId}, hasn't been found in db.");
    }

    #endregion

    #region CreateUserAsync Tests

    [Fact]
    public async Task CreateUserAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var createDto = TestDataBuilder.BuildUserForCreateDto();
        var mappedUser = TestDataBuilder.BuildBasicUser();

        _mapperMock.Setup(x => x.Map<User>(createDto)).Returns(mappedUser);

        // Act
        var result = await _userService.CreateUserAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.DateCreateUpdate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.Photo.Should().Be(createDto.Photo);

        _mapperMock.Verify(x => x.Map<User>(createDto), Times.Once);
        _userRepositoryMock.Verify(x => x.CreateAsync(mappedUser), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithNullUser_ShouldThrowBadRequestException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _userService.CreateUserAsync(null));

        exception.Message.Should().Be("Invalid  user object.");
        _loggerMock.Verify(x => x.LogError(
                "Error",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Once);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task DeleteUserAsync_CompleteFlow_ShouldExecuteAllSteps()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = TestDataBuilder.BuildBasicUser(userId);

        _userRepositoryMock.SetupUserExists(userId, true);
        _userRepositoryMock.SetupGetUser(userId, user);

        // Act
        await _userService.DeleteUserAsync(userId);

        // Assert - Verify complete flow
        _userRepositoryMock.Verify(x => x.IsExistAsync(userId),
            Times.Exactly(2)); // Once in DeleteUserAsync, once in GetUserByIdAsync
        _userRepositoryMock.Verify(x => x.GetAsync(userId), Times.Once);
        _imageServiceMock.Verify(x => x.DeletePhoto(user.Photo), Times.Once);
        _userRepositoryMock.Verify(x => x.DeleteAsync(userId), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WithoutPhoto_ShouldNotManageImages()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = TestDataBuilder.BuildUserForUpdateDto();
        updateDto.Photo = null; // No photo update
        var existingUser = TestDataBuilder.BuildBasicUser(userId);

        _userRepositoryMock.SetupUserExists(userId, true);
        _userRepositoryMock.SetupGetUser(userId, existingUser);

        // Act
        await _userService.UpdateUserAsync(userId, updateDto);

        // Assert
        _imageServiceMock.Verify(x => x.DeletePhoto(It.IsAny<string>()), Times.Never);
        _imageServiceMock.Verify(x => x.UploadPhotoAsync(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
        _mapperMock.Verify(x => x.Map(updateDto, existingUser), Times.Once);
    }

    #endregion
}