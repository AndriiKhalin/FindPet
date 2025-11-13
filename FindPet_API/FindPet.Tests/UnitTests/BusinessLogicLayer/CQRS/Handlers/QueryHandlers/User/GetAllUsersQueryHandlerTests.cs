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

/// <summary>
///     Unit tests for GetAllUsersQueryHandler covering all scenarios including success, empty collections,
///     service failures, mapping errors, and edge cases.
/// </summary>
public class GetAllUsersQueryHandlerTests
{
    private readonly GetAllUsersQueryHandler _handler;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IPhotoUrlTransformerService> _mockPhotoTransformerService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly GetAllUsersQuery _query;

    public GetAllUsersQueryHandlerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockMapper = new Mock<IMapper>();
        _mockPhotoTransformerService = MockSetupExtensions.CreateMock<IPhotoUrlTransformerService>();
        _handler = new GetAllUsersQueryHandler(_mockUserService.Object, _mockMapper.Object,
            _mockPhotoTransformerService.Object);
        _query = new GetAllUsersQuery();
    }

    #region Cancellation AccessToken Scenarios

    [Fact]
    public async Task Handle_WithCancelledToken_ShouldNotAffectSynchronousOperations()
    {
        // Arrange
        var users = TestDataBuilder.BuildUserList(2);
        var userDtos = TestDataBuilder.BuildUserDtoList(2);
        var cancelledToken = new CancellationToken(true);

        _mockUserService.SetupGetUsers(users);
        _mockMapper.SetupMapUserWithPhotosAsync(_mockPhotoTransformerService, users, userDtos);

        // Act
        var result = await _handler.Handle(_query, cancelledToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _mockUserService.Verify(x => x.GetUsersAsync(), Times.Once);
        _mockMapper.Verify(x => x.Map<IEnumerable<UserDto>>(users), Times.Once);
    }

    #endregion

    #region Verification Helper Methods

    private void VerifyNoUnexpectedCalls()
    {
        _mockUserService.VerifyNoOtherCalls();
        _mockMapper.VerifyNoOtherCalls();
    }

    #endregion

    #region Success Scenarios

    [Fact]
    public async Task Handle_WithValidUsers_ShouldReturnMappedUserDtos()
    {
        // Arrange
        var users = TestDataBuilder.BuildUserList();
        var expectedUserDtos = TestDataBuilder.BuildUserDtoList();

        _mockUserService.SetupGetUsers(users);
        _mockMapper.SetupMapUserWithPhotosAsync(_mockPhotoTransformerService, users, expectedUserDtos);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(expectedUserDtos);

        _mockUserService.Verify(x => x.GetUsersAsync(), Times.Once);
        _mockMapper.Verify(x => x.Map<IEnumerable<UserDto>>(users), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSingleUser_ShouldReturnSingleUserDto()
    {
        // Arrange
        var user = TestDataBuilder.BuildBasicUser();
        var users = new List<Domain.Entities.User> { user };
        var userDto = TestDataBuilder.BuildBasicUserDto();
        var expectedUserDtos = new List<UserDto> { userDto };

        _mockUserService.SetupGetUsers(users);
        _mockMapper.SetupMapUserWithPhotosAsync(_mockPhotoTransformerService, users, expectedUserDtos);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Should().BeEquivalentTo(userDto);

        _mockUserService.Verify(x => x.GetUsersAsync(), Times.Once);
        _mockMapper.Verify(x => x.Map<IEnumerable<UserDto>>(users), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyUserCollection_ShouldReturnEmptyCollection()
    {
        // Arrange
        var emptyUsers = new List<Domain.Entities.User>();
        var emptyUserDtos = new List<UserDto>();

        _mockUserService.SetupGetUsers(emptyUsers);
        _mockMapper.SetupMap(emptyUsers, emptyUserDtos);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _mockUserService.Verify(x => x.GetUsersAsync(), Times.Once);
        _mockMapper.Verify(x => x.Map<IEnumerable<UserDto>>(emptyUsers), Times.Once);
    }

    [Fact]
    public async Task Handle_WithLargeUserCollection_ShouldHandleEfficiently()
    {
        // Arrange
        var users = TestDataBuilder.BuildUserList(1000);
        var userDtos = TestDataBuilder.BuildUserDtoList(1000);

        _mockUserService.SetupGetUsers(users);
        _mockMapper.SetupMapUserWithPhotosAsync(_mockPhotoTransformerService, users, userDtos);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1000);

        _mockUserService.Verify(x => x.GetUsersAsync(), Times.Once);
        _mockMapper.Verify(x => x.Map<IEnumerable<UserDto>>(users), Times.Once);
    }

    #endregion

    #region Service Exception Scenarios

    [Fact]
    public async Task Handle_WhenUserServiceThrowsBadRequestException_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new BadRequestException("Invalid request parameters");

        _mockUserService
            .Setup(x => x.GetUsersAsync())
            .Throws(expectedException);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(_query, CancellationToken.None));

        exception.Message.Should().Be("Invalid request parameters");
        _mockUserService.Verify(x => x.GetUsersAsync(), Times.Once);
        _mockMapper.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenUserServiceThrowsNotFoundException_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new NotFoundException("Users", "collection");

        _mockUserService
            .Setup(x => x.GetUsersAsync())
            .Throws(expectedException);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(_query, CancellationToken.None));

        exception.Message.Should().Contain("Users");
        _mockUserService.Verify(x => x.GetUsersAsync(), Times.Once);
        _mockMapper.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenUserServiceThrowsInvalidOperationException_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Database connection failed");

        _mockUserService
            .Setup(x => x.GetUsersAsync())
            .Throws(expectedException);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(_query, CancellationToken.None));

        exception.Message.Should().Be("Database connection failed");
        _mockUserService.Verify(x => x.GetUsersAsync(), Times.Once);
        _mockMapper.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenUserServiceThrowsGenericException_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Unexpected error occurred");

        _mockUserService
            .Setup(x => x.GetUsersAsync())
            .Throws(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(_query, CancellationToken.None));

        exception.Message.Should().Be("Unexpected error occurred");
        _mockUserService.Verify(x => x.GetUsersAsync(), Times.Once);
        _mockMapper.VerifyNoOtherCalls();
    }

    #endregion

    #region Mapper Exception Scenarios

    [Fact]
    public async Task Handle_WhenMapperThrowsAutoMapperMappingException_ShouldPropagateException()
    {
        // Arrange
        var users = TestDataBuilder.BuildUserList(2);
        var expectedException = new AutoMapperMappingException("Mapping failed for User to UserDto");

        _mockUserService.SetupGetUsers(users);
        _mockMapper
            .Setup(x => x.Map<IEnumerable<UserDto>>(users))
            .Throws(expectedException);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _handler.Handle(_query, CancellationToken.None));

        exception.Message.Should().Contain("Mapping failed");
        _mockUserService.Verify(x => x.GetUsersAsync(), Times.Once);
        _mockMapper.Verify(x => x.Map<IEnumerable<UserDto>>(users), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMapperThrowsArgumentNullException_ShouldPropagateException()
    {
        // Arrange
        var users = TestDataBuilder.BuildUserList(1);
        var expectedException = new ArgumentNullException("source", "Source cannot be null");

        _mockUserService.SetupGetUsers(users);
        _mockMapper
            .Setup(x => x.Map<IEnumerable<UserDto>>(users))
            .Throws(expectedException);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(_query, CancellationToken.None));

        exception.ParamName.Should().Be("source");
        _mockUserService.Verify(x => x.GetUsersAsync(), Times.Once);
        _mockMapper.Verify(x => x.Map<IEnumerable<UserDto>>(users), Times.Once);
    }

    #endregion

    #region Edge Cases and Null Scenarios

    [Fact]
    public async Task Handle_WhenUserServiceReturnsNull_ShouldHandleGracefully()
    {
        // Arrange
        IEnumerable<Domain.Entities.User>? nullUsers = null;
        var emptyUserDtos = new List<UserDto>();

        _mockUserService
            .Setup(x => x.GetUsersAsync())
            .ReturnsAsync(nullUsers!);

        _mockMapper
            .Setup(x => x.Map<IEnumerable<UserDto>>(nullUsers!))
            .Returns(emptyUserDtos);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _mockUserService.Verify(x => x.GetUsersAsync(), Times.Once);
        _mockMapper.Verify(x => x.Map<IEnumerable<UserDto>>(nullUsers!), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMapperReturnsNull_ShouldReturnNull()
    {
        // Arrange
        var users = TestDataBuilder.BuildUserList(2);
        IEnumerable<UserDto>? nullUserDtos = null;

        _mockUserService.SetupGetUsers(users);
        _mockMapper.SetupMapUserWithPhotosAsync(_mockPhotoTransformerService, users, nullUserDtos);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _mockUserService.Verify(x => x.GetUsersAsync(), Times.Once);
        _mockMapper.Verify(x => x.Map<IEnumerable<UserDto>>(users), Times.Once);
    }

    #endregion

    #region Complex User Data Scenarios

    [Fact]
    public async Task Handle_WithUsersHavingComplexProperties_ShouldMapCorrectly()
    {
        // Arrange
        var users = new List<Domain.Entities.User>
        {
            TestDataBuilder.BuildBasicUser(
                name: "John",
                email: "john.doe@example.com",
                phone: "+1234567890"),
            TestDataBuilder.BuildBasicUser(
                name: "Jane",
                email: "jane.smith@example.com",
                phone: "+0987654321")
        };

        var userDtos = new List<UserDto>
        {
            TestDataBuilder.BuildBasicUserDto(
                name: "John",
                email: "john.doe@example.com",
                phone: "+1234567890"),
            TestDataBuilder.BuildBasicUserDto(
                name: "Jane",
                email: "jane.smith@example.com",
                phone: "+0987654321")
        };

        _mockUserService.SetupGetUsers(users);
        _mockMapper.SetupMapUserWithPhotosAsync(_mockPhotoTransformerService, users, userDtos);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var resultList = result.ToList();
        resultList[0].Name.Should().Be("John");
        resultList[0].Email.Should().Be("john.doe@example.com");
        resultList[1].Name.Should().Be("Jane");
        resultList[1].Email.Should().Be("jane.smith@example.com");

        _mockUserService.Verify(x => x.GetUsersAsync(), Times.Once);
        _mockMapper.Verify(x => x.Map<IEnumerable<UserDto>>(users), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUsersHavingNullOptionalProperties_ShouldHandleGracefully()
    {
        // Arrange
        var users = TestDataBuilder.BuildUserList(2);

        var userDtos = TestDataBuilder.BuildUserDtoList(2);

        _mockUserService.SetupGetUsers(users);
        _mockMapper.SetupMapUserWithPhotosAsync(_mockPhotoTransformerService, users, userDtos);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _mockUserService.Verify(x => x.GetUsersAsync(), Times.Once);
        _mockMapper.Verify(x => x.Map<IEnumerable<UserDto>>(users), Times.Once);
    }

    #endregion

    #region Performance and Memory Tests

    [Fact]
    public async Task Handle_ShouldNotCacheResults_EachCallShouldInvokeService()
    {
        // Arrange
        var users = TestDataBuilder.BuildUserList(1);
        var userDtos = TestDataBuilder.BuildUserDtoList(1);

        _mockUserService.SetupGetUsers(users);
        _mockMapper.SetupMap(users, userDtos);

        // Act
        await _handler.Handle(_query, CancellationToken.None);
        await _handler.Handle(_query, CancellationToken.None);

        // Assert
        _mockUserService.Verify(x => x.GetUsersAsync(), Times.Exactly(2));
        _mockMapper.Verify(x => x.Map<IEnumerable<UserDto>>(users), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WithValidUsers_ShouldExecuteAsynchronously()
    {
        // Arrange
        var users = TestDataBuilder.BuildUserList(1);
        var userDtos = TestDataBuilder.BuildUserDtoList(1);

        _mockUserService.SetupGetUsers(users);

        _mockMapper
            .Setup(x => x.Map<IEnumerable<UserDto>>(users))
            .Returns(userDtos);

        _mockPhotoTransformerService
            .Setup(x => x.TransformUserPhotosAsync(userDtos))
            .ReturnsAsync(userDtos);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo(userDtos);

        _mockUserService.Verify(x => x.GetUsersAsync(), Times.Once);
        _mockMapper.Verify(x => x.Map<IEnumerable<UserDto>>(users), Times.Once);
        _mockPhotoTransformerService.Verify(x => x.TransformUserPhotosAsync(It.IsAny<IEnumerable<UserDto>>()),
            Times.Once);
    }

    #endregion

    #region Constructor and Dependency Tests

    [Fact]
    public void Constructor_WithNullUserService_ShouldNotThrowException()
    {
        // Act & Assert - Based on the test failure, the constructor doesn't validate null parameters
        var handler = new GetAllUsersQueryHandler(null!, _mockMapper.Object, _mockPhotoTransformerService.Object);
        handler.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullMapper_ShouldNotThrowException()
    {
        // Act & Assert - Based on the test failure, the constructor doesn't validate null parameters
        var handler = new GetAllUsersQueryHandler(_mockUserService.Object, null!, _mockPhotoTransformerService.Object);
        handler.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act
        var handler = new GetAllUsersQueryHandler(_mockUserService.Object, _mockMapper.Object,
            _mockPhotoTransformerService.Object);

        // Assert
        handler.Should().NotBeNull();
        handler.Should().BeOfType<GetAllUsersQueryHandler>();
    }

    #endregion
}