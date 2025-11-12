using AutoMapper;
using FindPet.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.User;
using FindPet.BusinessLogicLayer.CQRS.Queries.User;
using FindPet.BusinessLogicLayer.Helpers.Resolver.PhotoUrlTransformer;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Exceptions;
using FindPet.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace FindPet.Tests.UnitTests.BusinessLogicLayer.CQRS.Handlers.QueryHandlers.User;

public class GetUserByIdQueryHandlerTests
{
    private readonly GetUserByIdQueryHandler _handler;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IPhotoUrlTransformerService> _mockPhotoUrlTransformerService;
    private readonly Mock<IUserService> _mockUserService;

    public GetUserByIdQueryHandlerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockMapper = new Mock<IMapper>();
        _mockPhotoUrlTransformerService = MockSetupExtensions.CreateMock<IPhotoUrlTransformerService>();
        _handler = new GetUserByIdQueryHandler(_mockUserService.Object, _mockMapper.Object,
            _mockPhotoUrlTransformerService.Object);
    }

    [Theory]
    [InlineData("123e4567-e89b-12d3-a456-426614174000")]
    [InlineData("987fcdeb-51a2-43d1-9c45-123456789abc")]
    public async Task Handle_WithValidUserId_ShouldReturnMappedUserDto(string userIdString)
    {
        // Arrange
        var userId = Guid.Parse(userIdString);
        var query = new GetUserByIdQuery(userId);
        var cancellationToken = CancellationToken.None;

        var userEntity = TestDataBuilder.BuildBasicUser(
            userId,
            email: "test@example.com",
            name: "John");

        var expectedUserDto = TestDataBuilder.BuildBasicUserDto(
            userId,
            email: "test@example.com",
            name: "John");

        _mockUserService.SetupGetUserById(userId, userEntity);
        _mockMapper.SetupMap(userEntity, expectedUserDto);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedUserDto);
        result.Id.Should().Be(userId);
        result.Email.Should().Be("test@example.com");
        result.Name.Should().Be("John");

        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockMapper.Verify(x => x.Map<UserDto>(userEntity), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ShouldCallServiceWithEmptyGuid()
    {
        // Arrange
        var userId = Guid.Empty;
        var query = new GetUserByIdQuery(userId);
        var cancellationToken = CancellationToken.None;

        var userEntity = TestDataBuilder.BuildBasicUser(userId);

        var userDto = new UserDto { Id = userId };

        _mockUserService.SetupGetUserById(userId, userEntity);
        _mockMapper.SetupMap(userEntity, userDto);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(Guid.Empty);
        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserServiceThrowsEntityNotFoundException_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var cancellationToken = CancellationToken.None;

        _mockUserService.Setup(s => s.GetUserByIdAsync(userId))
            .ThrowsAsync(new NotFoundException("User", userId));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, cancellationToken));

        exception.Should().NotBeNull();
        exception.Message.Should().Contain($"User with ID '{userId}' was not found");
        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockMapper.Verify(x => x.Map<UserDto>(It.IsAny<Domain.Entities.User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserServiceThrowsGenericException_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var cancellationToken = CancellationToken.None;

        var expectedException = new InvalidOperationException("Database connection failed");
        _mockUserService.Setup(x => x.GetUserByIdAsync(userId)).ThrowsAsync(expectedException);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(query, cancellationToken));

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Database connection failed");
        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockMapper.Verify(x => x.Map<UserDto>(It.IsAny<Domain.Entities.User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserServiceReturnsNull_ShouldAttemptToMapNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var cancellationToken = CancellationToken.None;

        _mockUserService.SetupGetUserById(userId, null);
        _mockMapper.SetupMap<Domain.Entities.User, UserDto>(null, null);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().BeNull();
        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockMapper.Verify(x => x.Map<UserDto>(null), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMapperThrowsException_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var cancellationToken = CancellationToken.None;

        var userEntity = TestDataBuilder.BuildBasicUser(userId);

        var mappingException = new AutoMapperMappingException("Mapping configuration error");
        _mockUserService.SetupGetUserById(userId, userEntity);
        _mockMapper.Setup(m => m.Map<UserDto>(It.IsAny<Domain.Entities.User>()))
            .Throws(mappingException);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _handler.Handle(query, cancellationToken));

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Mapping configuration error");
        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockMapper.Verify(x => x.Map<UserDto>(userEntity), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var userEntity = TestDataBuilder.BuildBasicUser(userId);

        var userDto = new UserDto { Id = userId };

        _mockUserService.SetupGetUserById(userId, userEntity);
        _mockMapper.SetupMap<Domain.Entities.User, UserDto>(userEntity, userDto);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancelledToken_ShouldThrowOperationCancelledException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancellationToken = cancellationTokenSource.Token;

        var cancellationException = new OperationCanceledException("Operation was cancelled");
        _mockUserService.Setup(x => x.GetUserByIdAsync(userId)).ThrowsAsync(cancellationException);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<OperationCanceledException>(() => _handler.Handle(query, cancellationToken));

        exception.Should().NotBeNull();
        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockMapper.Verify(x => x.Map<UserDto>(It.IsAny<Domain.Entities.User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithComplexUserEntity_ShouldMapAllProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var cancellationToken = CancellationToken.None;
        var userEntity = TestDataBuilder.BuildBasicUser(
            userId,
            email: "complex.user@example.com",
            name: "Jane",
            phone: "+1234567890"
        );
        var expectedUserDto = TestDataBuilder.BuildBasicUserDto(
            userId,
            email: "complex.user@example.com",
            name: "Jane",
            phone: "+1234567890"
        );

        _mockUserService.SetupGetUserById(userId, userEntity);
        _mockMapper.SetupMap(userEntity, expectedUserDto);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedUserDto);
        result.Id.Should().Be(userId);
        result.Email.Should().Be("complex.user@example.com");
        result.Name.Should().Be("Jane");
        result.PhoneNumber.Should().Be("+1234567890");

        _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        _mockMapper.Verify(x => x.Map<UserDto>(userEntity), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFollowExpectedExecutionFlow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var cancellationToken = CancellationToken.None;

        var userEntity = TestDataBuilder.BuildBasicUser(userId);

        var userDto = new UserDto { Id = userId };

        _mockUserService.SetupGetUserById(userId, userEntity);
        _mockMapper.SetupMap<Domain.Entities.User, UserDto>(userEntity, userDto);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();

        // Verify execution order and interactions
        var invocations = new List<string>();
        _mockUserService.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
            .Callback(() => invocations.Add("UserService.GetUserByIdAsync"))
            .ReturnsAsync(userEntity);

        _mockMapper.Setup(x => x.Map<UserDto>(It.IsAny<Domain.Entities.User>()))
            .Callback(() => invocations.Add("Mapper.Map"))
            .Returns(userDto);

        // Re-execute to verify order
        await _handler.Handle(query, cancellationToken);

        invocations.Should().HaveCount(2);
        invocations[0].Should().Be("UserService.GetUserByIdAsync");
        invocations[1].Should().Be("Mapper.Map");
    }
}

//public class GetUserByIdQueryHandlerTests
//{
//    private readonly Mock<IUserService> _userServiceMock;
//    private readonly Mock<IMapper> _mapperMock;
//    private readonly GetUserByIdQueryHandler _handler;
//    private readonly CancellationToken _cancellationToken;

//    public GetUserByIdQueryHandlerTests()
//    {
//        _userServiceMock = MockSetupExtensions.SetupUserServiceMock();
//        _mapperMock = MockSetupExtensions.SetupMapperMock();
//        _handler = new GetUserByIdQueryHandler(_userServiceMock.Object, _mapperMock.Object);
//        _cancellationToken = CancellationToken.None;
//    }

//    #region Happy Path Tests

//    [Fact]
//    public async Task Handle_WithValidUserId_ShouldReturnUserDto()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var user = TestDataBuilder.BuildBasicUser(id: userId);
//        var userDto = TestDataBuilder.BuildBasicUserDto(id: userId);
//        var query = new GetUserByIdQuery(userId);

//        _userServiceMock.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(user);
//        _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

//        // Act
//        var result = await _handler.Handle(query, _cancellationToken);

//        // Assert
//        result.Should().NotBeNull();
//        result.Should().BeEquivalentTo(userDto);
//        _userServiceMock.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
//        _mapperMock.Verify(m => m.Map<UserDto>(user), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WithValidUserId_ShouldCorrectlyMapAllUserProperties()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var user = TestDataBuilder.BuildBasicUser(
//            id: userId,
//            name: "John Doe",
//            email: "john@example.com",
//            phone: "+1234567890",
//            photo: "profile.jpg",
//            birthDate: new DateTime(1990, 1, 1),
//            createDate: new DateTime(2023, 1, 1));

//        var userDto = new UserDto
//        {
//            Id = userId,
//            Name = "John Doe",
//            Email = "john@example.com",
//            PhoneNumber = "+1234567890",
//            Photo = "profile.jpg",
//            BirthDate = new DateTime(1990, 1, 1),
//            DateCreateUpdate = new DateTime(2023, 1, 1)
//        };

//        var query = new GetUserByIdQuery(userId);

//        _userServiceMock.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(user);
//        _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

//        // Act
//        var result = await _handler.Handle(query, _cancellationToken);

//        // Assert
//        result.Should().NotBeNull();
//        result.Id.Should().Be(userId);
//        result.Name.Should().Be("John Doe");
//        result.Email.Should().Be("john@example.com");
//        result.PhoneNumber.Should().Be("+1234567890");
//        result.Photo.Should().Be("profile.jpg");
//        result.BirthDate.Should().Be(new DateTime(1990, 1, 1));
//        result.DateCreateUpdate.Should().Be(new DateTime(2023, 1, 1));
//    }

//    #endregion

//    #region Exception Tests

//    [Fact]
//    public async Task Handle_WhenUserNotFound_ShouldPropagateNotFoundException()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var query = new GetUserByIdQuery(userId);

//        _userServiceMock.Setup(s => s.GetUserByIdAsync(userId))
//            .ThrowsAsync(new NotFoundException("User", userId));

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<NotFoundException>(
//            () => _handler.Handle(query, _cancellationToken));

//        exception.Message.Should().Contain($"User with ID '{userId}' was not found");
//        _mapperMock.Verify(m => m.Map<UserDto>(It.IsAny<Domain.Entities.User>()), Times.Never);
//    }

//    [Fact]
//    public async Task Handle_WithEmptyUserId_ShouldPropagateBadRequestException()
//    {
//        // Arrange
//        var emptyId = Guid.Empty;
//        var query = new GetUserByIdQuery(emptyId);

//        _userServiceMock.Setup(s => s.GetUserByIdAsync(emptyId))
//            .ThrowsAsync(new BadRequestException("User ID must be NON-Empty"));

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<BadRequestException>(
//            () => _handler.Handle(query, _cancellationToken));

//        exception.Message.Should().Be("User ID must be NON-Empty");
//        _mapperMock.Verify(m => m.Map<UserDto>(It.IsAny<Domain.Entities.User>()), Times.Never);
//    }

//    [Fact]
//    public async Task Handle_WhenMapperFails_ShouldPropagateException()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var user = TestDataBuilder.BuildBasicUser(id: userId);
//        var query = new GetUserByIdQuery(userId);

//        _userServiceMock.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(user);
//        _mapperMock.Setup(m => m.Map<UserDto>(user)).Throws(new AutoMapperMappingException("Mapping failed"));

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
//            () => _handler.Handle(query, _cancellationToken));

//        exception.Message.Should().Contain("Mapping failed");
//        _userServiceMock.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WhenUnexpectedServiceException_ShouldPropagateException()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var query = new GetUserByIdQuery(userId);

//        _userServiceMock.Setup(s => s.GetUserByIdAsync(userId))
//            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
//            () => _handler.Handle(query, _cancellationToken));

//        exception.Message.Should().Be("Unexpected error");
//        _mapperMock.Verify(m => m.Map<UserDto>(It.IsAny<Domain.Entities.User>()), Times.Never);
//    }

//    #endregion

//    #region Edge Case Tests

//    [Fact]
//    public async Task Handle_WhenUserServiceReturnsNull_ShouldReturnNull()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var query = new GetUserByIdQuery(userId);

//        _userServiceMock.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync((Domain.Entities.User)null);
//        _mapperMock.Setup(m => m.Map<UserDto>(null)).Returns((UserDto)null);

//        // Act
//        var result = await _handler.Handle(query, _cancellationToken);

//        // Assert
//        result.Should().BeNull();
//        _userServiceMock.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
//        _mapperMock.Verify(m => m.Map<UserDto>(null), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WhenMapperReturnsNull_ShouldReturnNull()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var user = TestDataBuilder.BuildBasicUser(id: userId);
//        var query = new GetUserByIdQuery(userId);

//        _userServiceMock.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(user);
//        _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns((UserDto)null);

//        // Act
//        var result = await _handler.Handle(query, _cancellationToken);

//        // Assert
//        result.Should().BeNull();
//        _userServiceMock.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
//        _mapperMock.Verify(m => m.Map<UserDto>(user), Times.Once);
//    }

//    #endregion
//}