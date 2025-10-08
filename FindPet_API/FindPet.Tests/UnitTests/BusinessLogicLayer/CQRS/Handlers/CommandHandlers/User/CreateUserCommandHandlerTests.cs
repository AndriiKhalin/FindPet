using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Commands.User;
using FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.User;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Exceptions;
using FindPet.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace FindPet.Tests.UnitTests.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.User;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _userServiceMock = MockSetupExtensions.SetupUserServiceMock();
        _mapperMock = MockSetupExtensions.SetupMapperMock();

        _handler = new CreateUserCommandHandler(_userServiceMock.Object, _mapperMock.Object);
    }

    #region Successful Creation Tests

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateUserAndReturnDto()
    {
        // Arrange
        var createDto = TestDataBuilder.BuildUserForCreateDto();
        var command = new CreateUserCommand(createDto);
        var createdUser = TestDataBuilder.BuildBasicUser();
        var expectedDto = new UserDto
        {
            Id = createdUser.Id,
            Name = createdUser.Name,
            Email = createdUser.Email
        };

        _userServiceMock.SetupCreateUser(createDto, createdUser);
        _mapperMock.Setup(x => x.Map<UserDto>(createdUser)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDto);
        result.Id.Should().Be(createdUser.Id);
        result.Name.Should().Be(createdUser.Name);
        result.Email.Should().Be(createdUser.Email);

        _userServiceMock.Verify(x => x.CreateUserAsync(createDto), Times.Once);
        _mapperMock.Verify(x => x.Map<UserDto>(createdUser), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUserWithPhoto_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var createDto = TestDataBuilder.BuildUserForCreateDtoWithPhoto();
        var command = new CreateUserCommand(createDto);
        var createdUser = TestDataBuilder.BuildUserWithPhoto();
        var expectedDto = new UserDto { Id = createdUser.Id, Name = createdUser.Name };

        _userServiceMock.SetupCreateUser(createDto, createdUser);
        _mapperMock.Setup(x => x.Map<UserDto>(createdUser)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(createdUser.Id);
        _userServiceMock.Verify(x => x.CreateUserAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMinimalValidData_ShouldCreateUser()
    {
        // Arrange
        var createDto = TestDataBuilder.BuildUserForCreateDto(
            name: "John",
            email: "john@test.com",
            phone: "+1234567890",
            password: "Password123"
        );
        var command = new CreateUserCommand(createDto);
        var createdUser = TestDataBuilder.BuildBasicUser();
        var expectedDto = new UserDto { Id = createdUser.Id };

        _userServiceMock.SetupCreateUser(createDto, createdUser);
        _mapperMock.Setup(x => x.Map<UserDto>(createdUser)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _userServiceMock.Verify(x => x.CreateUserAsync(createDto), Times.Once);
    }

    #endregion

    #region Validation and Business Logic Tests

    [Fact]
    public async Task Handle_WithNullUser_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new CreateUserCommand(null);

        _userServiceMock.Setup(x => x.CreateUserAsync(null))
            .ThrowsAsync(new BadRequestException("Invalid  user object."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Invalid  user object.");
        _userServiceMock.Verify(x => x.CreateUserAsync(null), Times.Once);
        _mapperMock.Verify(x => x.Map<UserDto>(It.IsAny<Domain.Entities.User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidUserData_ShouldThrowValidationException()
    {
        // Arrange
        var invalidDto = TestDataBuilder.BuildInvalidUserForCreateDto("email");
        var command = new CreateUserCommand(invalidDto);

        _userServiceMock.Setup(x => x.CreateUserAsync(invalidDto))
            .ThrowsAsync(new ArgumentException("Invalid user data"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));

        _userServiceMock.Verify(x => x.CreateUserAsync(invalidDto), Times.Once);
        _mapperMock.Verify(x => x.Map<UserDto>(It.IsAny<Domain.Entities.User>()), Times.Never);
    }

    #endregion

    #region Email and Phone Uniqueness Tests

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldThrowException()
    {
        // Arrange
        var createDto = TestDataBuilder.BuildUserForCreateDto(email: "existing@test.com");
        var command = new CreateUserCommand(createDto);

        _userServiceMock.Setup(x => x.CreateUserAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Email already exists"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Email already exists");
        _userServiceMock.Verify(x => x.CreateUserAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingPhoneNumber_ShouldThrowException()
    {
        // Arrange
        var createDto = TestDataBuilder.BuildUserForCreateDto(phone: "+1234567890");
        var command = new CreateUserCommand(createDto);

        _userServiceMock.Setup(x => x.CreateUserAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Phone number already exists"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Phone number already exists");
    }

    #endregion

    #region Database and Infrastructure Tests

    [Fact]
    public async Task Handle_WithDatabaseError_ShouldThrowException()
    {
        // Arrange
        var createDto = TestDataBuilder.BuildUserForCreateDto();
        var command = new CreateUserCommand(createDto);

        _userServiceMock.Setup(x => x.CreateUserAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Database connection failed");
        _mapperMock.Verify(x => x.Map<UserDto>(It.IsAny<Domain.Entities.User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithMappingError_ShouldThrowException()
    {
        // Arrange
        var createDto = TestDataBuilder.BuildUserForCreateDto();
        var command = new CreateUserCommand(createDto);
        var createdUser = TestDataBuilder.BuildBasicUser();

        _userServiceMock.SetupCreateUser(createDto, createdUser);
        _mapperMock.Setup(x => x.Map<UserDto>(createdUser))
            .Throws(new AutoMapperMappingException("Mapping failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Mapping failed");
        _userServiceMock.Verify(x => x.CreateUserAsync(createDto), Times.Once);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToService()
    {
        // Arrange
        var createDto = TestDataBuilder.BuildUserForCreateDto();
        var command = new CreateUserCommand(createDto);
        var cancellationToken = new CancellationToken(true);
        var createdUser = TestDataBuilder.BuildBasicUser();

        _userServiceMock.Setup(x => x.CreateUserAsync(createDto))
            .Callback(() => cancellationToken.ThrowIfCancellationRequested())
            .ReturnsAsync(createdUser);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(command, cancellationToken));
    }

    [Fact]
    public async Task Handle_WithValidCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        var createDto = TestDataBuilder.BuildUserForCreateDto();
        var command = new CreateUserCommand(createDto);
        var cancellationToken = CancellationToken.None;
        var createdUser = TestDataBuilder.BuildBasicUser();
        var expectedDto = new UserDto { Id = createdUser.Id };

        _userServiceMock.SetupCreateUser(createDto, createdUser);
        _mapperMock.Setup(x => x.Map<UserDto>(createdUser)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        _userServiceMock.Verify(x => x.CreateUserAsync(createDto), Times.Once);
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Fact]
    public async Task Handle_WithLongUserName_ShouldCreateSuccessfully()
    {
        // Arrange
        var longName = new string('A', 100); // Maximum allowed length
        var createDto = TestDataBuilder.BuildUserForCreateDto(name: longName);
        var command = new CreateUserCommand(createDto);
        var createdUser = TestDataBuilder.BuildBasicUser().With(u => u.Name = longName);
        var expectedDto = new UserDto { Id = createdUser.Id, Name = longName };

        _userServiceMock.SetupCreateUser(createDto, createdUser);
        _mapperMock.Setup(x => x.Map<UserDto>(createdUser)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(longName);
    }

    [Fact]
    public async Task Handle_WithSpecialCharactersInData_ShouldCreateSuccessfully()
    {
        // Arrange
        var createDto = TestDataBuilder.BuildUserForCreateDto(
            name: "José María O'Connor-Smith",
            email: "jose.maria@test-domain.com"
        );
        var command = new CreateUserCommand(createDto);
        var createdUser = TestDataBuilder.BuildBasicUser();
        var expectedDto = new UserDto { Id = createdUser.Id };

        _userServiceMock.SetupCreateUser(createDto, createdUser);
        _mapperMock.Setup(x => x.Map<UserDto>(createdUser)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _userServiceMock.Verify(x => x.CreateUserAsync(createDto), Times.Once);
    }

    #endregion

    #region Service Integration Tests

    [Fact]
    public async Task Handle_WithCompleteUserData_ShouldCallAllServices()
    {
        // Arrange
        var createDto = TestDataBuilder.BuildUserForCreateDto(
            birthDate: DateTime.UtcNow.AddYears(-25),
            photo: "profile.jpg"
        );
        var command = new CreateUserCommand(createDto);
        var createdUser = TestDataBuilder.BuildBasicUser();
        createdUser.BirthDate = createDto.BirthDate;
        createdUser.Photo = createDto.Photo;

        var expectedDto = new UserDto
        {
            Id = createdUser.Id,
            Name = createdUser.Name,
            Email = createdUser.Email
        };

        _userServiceMock.SetupCreateUser(createDto, createdUser);
        _mapperMock.Setup(x => x.Map<UserDto>(createdUser)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDto);

        // Verify service calls
        _userServiceMock.Verify(x => x.CreateUserAsync(It.Is<UserForCreateDto>(dto =>
            dto.Name == createDto.Name &&
            dto.Email == createDto.Email &&
            dto.PhoneNumber == createDto.PhoneNumber &&
            dto.Password == createDto.Password &&
            dto.BirthDate == createDto.BirthDate &&
            dto.Photo == createDto.Photo
        )), Times.Once);

        _mapperMock.Verify(x => x.Map<UserDto>(It.Is<Domain.Entities.User>(u =>
            u.Id == createdUser.Id &&
            u.Name == createdUser.Name &&
            u.Email == createdUser.Email
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldEnsureCorrectExecutionOrder()
    {
        // Arrange
        var createDto = TestDataBuilder.BuildUserForCreateDto();
        var command = new CreateUserCommand(createDto);
        var createdUser = TestDataBuilder.BuildBasicUser();
        var expectedDto = new UserDto { Id = createdUser.Id };

        var executionOrder = new List<string>();

        _userServiceMock.Setup(x => x.CreateUserAsync(createDto))
            .Callback(() => executionOrder.Add("UserService.CreateUserAsync"))
            .ReturnsAsync(createdUser);

        _mapperMock.Setup(x => x.Map<UserDto>(createdUser))
            .Callback(() => executionOrder.Add("Mapper.Map"))
            .Returns(expectedDto);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        executionOrder.Should().HaveCount(2);
        executionOrder[0].Should().Be("UserService.CreateUserAsync");
        executionOrder[1].Should().Be("Mapper.Map");
    }

    #endregion

    #region Mock Verification Tests

    [Fact]
    public async Task Handle_ShouldNotCallMapperIfServiceFails()
    {
        // Arrange
        var createDto = TestDataBuilder.BuildUserForCreateDto();
        var command = new CreateUserCommand(createDto);

        _userServiceMock.Setup(x => x.CreateUserAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Service failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        _userServiceMock.Verify(x => x.CreateUserAsync(createDto), Times.Once);
        _mapperMock.Verify(x => x.Map<UserDto>(It.IsAny<Domain.Entities.User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallServicesExactlyOnce()
    {
        // Arrange
        var createDto = TestDataBuilder.BuildUserForCreateDto();
        var command = new CreateUserCommand(createDto);
        var createdUser = TestDataBuilder.BuildBasicUser();
        var expectedDto = new UserDto { Id = createdUser.Id };

        _userServiceMock.SetupCreateUser(createDto, createdUser);
        _mapperMock.Setup(x => x.Map<UserDto>(createdUser)).Returns(expectedDto);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Verify exact call counts
        _userServiceMock.Verify(x => x.CreateUserAsync(createDto), Times.Once);
        _mapperMock.Verify(x => x.Map<UserDto>(createdUser), Times.Once);

        // Verify no other methods were called
        _userServiceMock.VerifyNoOtherCalls();
    }

    #endregion
}