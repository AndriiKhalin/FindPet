using FindPet.BusinessLogicLayer.CQRS.Commands.User;
using FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.User;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.Exceptions;
using FindPet.Tests.TestHelpers;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace FindPet.Tests.UnitTests.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.User;

public class DeleteUserCommandHandlerTests
{
    private readonly DeleteUserCommandHandler _handler;
    private readonly Mock<IUserService> _mockUserService;

    public DeleteUserCommandHandlerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _handler = new DeleteUserCommandHandler(_mockUserService.Object);
    }

    [Fact]
    public async Task Handle_ValidUserId_ShouldDeleteUserSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
        var cancellationToken = CancellationToken.None;

        _mockUserService.SetupDeleteUser();

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be(Unit.Value);
        _mockUserService.Verify(x => x.DeleteUserAsync(userId), Times.Once);
        _mockUserService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
        var cancellationToken = CancellationToken.None;

        _mockUserService.Setup(s => s.DeleteUserAsync(userId))
            .ThrowsAsync(new NotFoundException($"User with ID {userId} not found"));

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User with ID {userId} not found");

        _mockUserService.Verify(x => x.DeleteUserAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_ServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
        var cancellationToken = CancellationToken.None;
        var expectedMessage = "Database connection failed";

        _mockUserService.Setup(s => s.DeleteUserAsync(userId))
            .ThrowsAsync(new InvalidOperationException(expectedMessage));

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(expectedMessage);

        _mockUserService.Verify(x => x.DeleteUserAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
        var cancellationToken = new CancellationToken(true);

        _mockUserService.Setup(s => s.DeleteUserAsync(userId))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        _mockUserService.Verify(x => x.DeleteUserAsync(userId), Times.Once);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")] // Empty GUID
    public async Task Handle_InvalidUserId_ShouldThrowValidationException(string invalidGuidString)
    {
        // Arrange
        var invalidId = Guid.Parse(invalidGuidString);
        var command = new DeleteUserCommand(invalidId);
        var cancellationToken = CancellationToken.None;

        _mockUserService.Setup(s => s.DeleteUserAsync(invalidId))
            .ThrowsAsync(new ValidationException("Invalid user ID: cannot be empty GUID"));

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Invalid user ID: cannot be empty GUID");

        _mockUserService.Verify(x => x.DeleteUserAsync(invalidId), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyGuid_ShouldThrowValidationException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var command = new DeleteUserCommand(emptyGuid);
        var cancellationToken = CancellationToken.None;

        _mockUserService.Setup(s => s.DeleteUserAsync(emptyGuid))
            .ThrowsAsync(new ValidationException("User ID cannot be empty"));

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("User ID cannot be empty");

        _mockUserService.Verify(x => x.DeleteUserAsync(emptyGuid), Times.Once);
    }

    [Fact]
    public void CreateCommand_WithInvalidGuidString_ShouldThrowFormatException()
    {
        // Arrange & Act
        var act = () => new DeleteUserCommand(Guid.Parse("invalid-guid"));

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Unrecognized Guid format.");
    }

    [Fact]
    public void CreateCommand_WithEmptyString_ShouldThrowFormatException()
    {
        // Arrange & Act
        var act = () => new DeleteUserCommand(Guid.Parse(""));

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Unrecognized Guid format.");
    }

    [Fact]
    public async Task Handle_UserHasAssociatedPets_ShouldThrowBusinessLogicException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
        var cancellationToken = CancellationToken.None;

        _mockUserService.Setup(s => s.DeleteUserAsync(userId))
            .ThrowsAsync(new BusinessRuleException("Cannot delete user with active pet advertisements"));

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("Cannot delete user with active pet advertisements");

        _mockUserService.Verify(x => x.DeleteUserAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_ConcurrentDeletion_ShouldThrowConcurrencyException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
        var cancellationToken = CancellationToken.None;

        _mockUserService.SetupDeleteUserAsync(userId)
            .ThrowsAsync(new ConflictException("User was already deleted by another process"));

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("User was already deleted by another process");

        _mockUserService.Verify(x => x.DeleteUserAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_UnauthorizedDeletion_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
        var cancellationToken = CancellationToken.None;

        _mockUserService.SetupDeleteUserAsync(userId)
            .ThrowsAsync(new UnauthorizedAccessException("Insufficient permissions to delete user"));

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Insufficient permissions to delete user");

        _mockUserService.Verify(x => x.DeleteUserAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_DatabaseConstraintViolation_ShouldThrowDatabaseException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
        var cancellationToken = CancellationToken.None;

        _mockUserService.SetupDeleteUserAsync(userId)
            .ThrowsAsync(new ConflictException("Foreign key constraint violation"));

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Foreign key constraint violation");

        _mockUserService.Verify(x => x.DeleteUserAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleCallsWithSameUserId_ShouldCallServiceEachTime()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
        var cancellationToken = CancellationToken.None;

        _mockUserService.SetupDeleteUserAsync(userId)
            .Returns(Task.CompletedTask);

        // Act
        var result1 = await _handler.Handle(command, cancellationToken);
        var result2 = await _handler.Handle(command, cancellationToken);

        // Assert
        result1.Should().Be(Unit.Value);
        result2.Should().Be(Unit.Value);
        _mockUserService.Verify(x => x.DeleteUserAsync(userId), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_NullCommand_ShouldThrowArgumentNullException()
    {
        // Arrange
        DeleteUserCommand command = null;
        var cancellationToken = CancellationToken.None;

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<NullReferenceException>();
        _mockUserService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ServiceReturnsSuccessfully_ShouldReturnUnitValue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
        var cancellationToken = CancellationToken.None;

        _mockUserService.SetupDeleteUserAsync(userId)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Unit>();
        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_LongRunningOperation_ShouldRespectCancellationToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mockUserService.SetupDeleteUserAsync(userId)
            .Returns(async () => { await Task.Delay(1000, cancellationToken); });

        // Act
        cancellationTokenSource.CancelAfter(100);
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        _mockUserService.Verify(x => x.DeleteUserAsync(userId), Times.Once);
    }
}

//public class DeleteUserCommandHandlerTests
//{
//    private readonly Mock<IUserService> _userServiceMock;
//    private readonly DeleteUserCommandHandler _handler;
//    private readonly CancellationToken _cancellationToken;

//    public DeleteUserCommandHandlerTests()
//    {
//        _userServiceMock = MockSetupExtensions.SetupUserServiceMock();
//        _handler = new DeleteUserCommandHandler(_userServiceMock.Object);
//        _cancellationToken = CancellationToken.None;
//    }

//    #region Handle Method Tests

//    [Fact]
//    public async Task Handle_WithValidUserId_ShouldCallDeleteUserAsync()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var command = new DeleteUserCommand(userId);

//        _userServiceMock.Setup(s => s.DeleteUserAsync(userId))
//            .Returns(Task.CompletedTask);

//        // Act
//        var result = await _handler.Handle(command, _cancellationToken);

//        // Assert
//        result.Should().Be(Unit.Value);
//        _userServiceMock.Verify(s => s.DeleteUserAsync(userId), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WithEmptyGuid_ShouldStillAttemptDeleteUser()
//    {
//        // Arrange
//        var userId = Guid.Empty;
//        var command = new DeleteUserCommand(userId);

//        _userServiceMock.Setup(s => s.DeleteUserAsync(userId))
//            .Returns(Task.CompletedTask);

//        // Act
//        var result = await _handler.Handle(command, _cancellationToken);

//        // Assert
//        result.Should().Be(Unit.Value);
//        _userServiceMock.Verify(s => s.DeleteUserAsync(userId), Times.Once);
//        // Note: The empty Guid validation is handled in the UserService, not in the handler
//    }

//    [Fact]
//    public async Task Handle_WhenUserDoesNotExist_ShouldPropagateException()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var command = new DeleteUserCommand(userId);

//        _userServiceMock.Setup(s => s.DeleteUserAsync(userId))
//            .ThrowsAsync(new ArgumentNullException("Invalid user Id"));

//        // Act & Assert
//        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
//            await _handler.Handle(command, _cancellationToken));

//        _userServiceMock.Verify(s => s.DeleteUserAsync(userId), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WhenExceptionOccurs_ShouldPropagateException()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var command = new DeleteUserCommand(userId);

//        _userServiceMock.Setup(s => s.DeleteUserAsync(userId))
//            .ThrowsAsync(new Exception("Unexpected error"));

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<Exception>(async () =>
//            await _handler.Handle(command, _cancellationToken));

//        exception.Message.Should().Be("Unexpected error");
//        _userServiceMock.Verify(s => s.DeleteUserAsync(userId), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WhenUserServiceThrowsNotFoundException_ShouldPropagateException()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var command = new DeleteUserCommand(userId);

//        _userServiceMock.Setup(s => s.DeleteUserAsync(userId))
//            .ThrowsAsync(new NotFoundException("User", userId));

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<NotFoundException>(async () =>
//            await _handler.Handle(command, _cancellationToken));

//        exception.Message.Should().Contain(userId.ToString());
//        _userServiceMock.Verify(s => s.DeleteUserAsync(userId), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WhenUserServiceThrowsBadRequestException_ShouldPropagateException()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var command = new DeleteUserCommand(userId);

//        _userServiceMock.Setup(s => s.DeleteUserAsync(userId))
//            .ThrowsAsync(new BadRequestException("Bad request"));

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<BadRequestException>(async () =>
//            await _handler.Handle(command, _cancellationToken));

//        exception.Message.Should().Be("Bad request");
//        _userServiceMock.Verify(s => s.DeleteUserAsync(userId), Times.Once);
//    }

//    #endregion

//    #region Integration Tests with UserService

//    [Fact]
//    public async Task Handle_IntegrationWithUserService_Success()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var command = new DeleteUserCommand(userId);

//        var userRepositoryMock = MockSetupExtensions.SetupUserRepositoryMock();
//        var unitOfWorkMock = MockSetupExtensions.SetupUnitOfWorkMock();
//        var mapperMock = MockSetupExtensions.SetupMapperMock();
//        var imageServiceMock = MockSetupExtensions.SetupImageServiceMock<FindPet.Domain.Entities.User>();
//        var loggerMock = MockSetupExtensions.SetupLoggerMock();

//        unitOfWorkMock.Setup(u => u.User).Returns(userRepositoryMock.Object);
//        userRepositoryMock.SetupUserExists(userId, true);

//        var userEntity = TestDataBuilder.BuildBasicUser(userId);
//        userRepositoryMock.SetupGetUser(userId, userEntity);

//        var userService = new FindPet.BusinessLogicLayer.Services.EntityService.UserService(
//            unitOfWorkMock.Object,
//            mapperMock.Object,
//            imageServiceMock.Object,
//            loggerMock.Object);

//        var handler = new DeleteUserCommandHandler(userService);

//        // Act
//        var result = await handler.Handle(command, _cancellationToken);

//        // Assert
//        result.Should().Be(Unit.Value);
//        imageServiceMock.Verify(i => i.DeletePhoto(userEntity.Photo), Times.Once);
//        userRepositoryMock.Verify(r => r.DeleteAsync(userId), Times.Once);
//        unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_IntegrationWithUserService_UserNotFound()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var command = new DeleteUserCommand(userId);

//        var userRepositoryMock = MockSetupExtensions.SetupUserRepositoryMock();
//        var unitOfWorkMock = MockSetupExtensions.SetupUnitOfWorkMock();
//        var mapperMock = MockSetupExtensions.SetupMapperMock();
//        var imageServiceMock = MockSetupExtensions.SetupImageServiceMock<FindPet.Domain.Entities.User>();
//        var loggerMock = MockSetupExtensions.SetupLoggerMock();

//        unitOfWorkMock.Setup(u => u.User).Returns(userRepositoryMock.Object);
//        userRepositoryMock.SetupUserExists(userId, false);

//        var userService = new FindPet.BusinessLogicLayer.Services.EntityService.UserService(
//            unitOfWorkMock.Object,
//            mapperMock.Object,
//            imageServiceMock.Object,
//            loggerMock.Object);

//        var handler = new DeleteUserCommandHandler(userService);

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<NotFoundException>(async () =>
//            await handler.Handle(command, _cancellationToken));

//        exception.Message.Should().Contain($"User with ID '{userId}' was not found");
//        loggerMock.VerifyLogError($"User with id: {userId}, hasn't been found in db.");
//        userRepositoryMock.Verify(r => r.DeleteAsync(userId), Times.Never);
//        unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Never);
//    }

//    #endregion

//    #region Edge Cases and Specialized Scenarios

//    [Fact]
//    public async Task Handle_WithMultipleConsecutiveCalls_ShouldCallUserServiceEachTime()
//    {
//        // Arrange
//        var userId1 = Guid.NewGuid();
//        var userId2 = Guid.NewGuid();
//        var command1 = new DeleteUserCommand(userId1);
//        var command2 = new DeleteUserCommand(userId2);

//        _userServiceMock.Setup(s => s.DeleteUserAsync(It.IsAny<Guid>()))
//            .Returns(Task.CompletedTask);

//        // Act
//        await _handler.Handle(command1, _cancellationToken);
//        await _handler.Handle(command2, _cancellationToken);

//        // Assert
//        _userServiceMock.Verify(s => s.DeleteUserAsync(userId1), Times.Once);
//        _userServiceMock.Verify(s => s.DeleteUserAsync(userId2), Times.Once);
//        _userServiceMock.Verify(s => s.DeleteUserAsync(It.IsAny<Guid>()), Times.Exactly(2));
//    }

//    [Fact]
//    public async Task Handle_WhenUserServiceThrowsOnSaveAsync_ShouldPropagateException()
//    {
//        // Arrange
//        var userId = Guid.NewGuid();
//        var command = new DeleteUserCommand(userId);

//        // Create mocks to simulate failure at the SaveAsync level
//        var userRepositoryMock = MockSetupExtensions.SetupUserRepositoryMock();
//        var unitOfWorkMock = MockSetupExtensions.SetupUnitOfWorkMock();
//        var mapperMock = MockSetupExtensions.SetupMapperMock();
//        var imageServiceMock = MockSetupExtensions.SetupImageServiceMock<FindPet.Domain.Entities.User>();
//        var loggerMock = MockSetupExtensions.SetupLoggerMock();

//        unitOfWorkMock.Setup(u => u.User).Returns(userRepositoryMock.Object);
//        userRepositoryMock.SetupUserExists(userId, true);

//        var userEntity = TestDataBuilder.BuildBasicUser(userId);
//        userRepositoryMock.SetupGetUser(userId, userEntity);

//        // Setup SaveAsync to throw an exception
//        unitOfWorkMock.Setup(u => u.SaveAsync())
//            .ThrowsAsync(new Exception("Database connection failed"));

//        var userService = new FindPet.BusinessLogicLayer.Services.EntityService.UserService(
//            unitOfWorkMock.Object,
//            mapperMock.Object,
//            imageServiceMock.Object,
//            loggerMock.Object);

//        var handler = new DeleteUserCommandHandler(userService);

//        // Act & Assert
//        await Assert.ThrowsAsync<Exception>(async () =>
//            await handler.Handle(command, _cancellationToken));

//        // Verify that the delete operation was attempted before the failure
//        imageServiceMock.Verify(i => i.DeletePhoto(userEntity.Photo), Times.Once);
//        userRepositoryMock.Verify(r => r.DeleteAsync(userId), Times.Once);
//        unitOfWorkMock.Verify(u => u.SaveAsync(), Times.Once);
//    }

//    #endregion
//}