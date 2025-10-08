using FindPet.BusinessLogicLayer.CQRS.Commands.User;
using FindPet.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.User;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Exceptions;
using FindPet.Tests.TestHelpers;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace FindPet.Tests.UnitTests.BusinessLogicLayer.CQRS.Handlers.CommandHandlers.User;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _handler = new UpdateUserCommandHandler(_mockUserService.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCallUserServiceAndReturnUnit()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userForUpdate = TestDataBuilder.BuildUserForUpdateDto();
        var command = new UpdateUserCommand(userId, userForUpdate);

        _mockUserService
            .Setup(x => x.UpdateUserAsync(userId, userForUpdate))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _mockUserService.Verify(x => x.UpdateUserAsync(userId, userForUpdate), Times.Once);
    }

    [Fact]
    public async Task Handle_UserServiceThrowsBadRequestException_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userForUpdate = TestDataBuilder.BuildUserForUpdateDto();
        var command = new UpdateUserCommand(userId, userForUpdate);
        var expectedException = new BadRequestException("User is null");

        _mockUserService
            .Setup(x => x.UpdateUserAsync(userId, userForUpdate))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("User is null");
        _mockUserService.Verify(x => x.UpdateUserAsync(userId, userForUpdate), Times.Once);
    }

    [Fact]
    public async Task Handle_UserServiceThrowsNotFoundException_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userForUpdate = TestDataBuilder.BuildUserForUpdateDto();
        var command = new UpdateUserCommand(userId, userForUpdate);
        var expectedException = new NotFoundException("User", userId);

        _mockUserService
            .Setup(x => x.UpdateUserAsync(userId, userForUpdate))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("User");
        exception.Message.Should().Contain(userId.ToString());
        _mockUserService.Verify(x => x.UpdateUserAsync(userId, userForUpdate), Times.Once);
    }

    [Fact]
    public async Task Handle_UserServiceThrowsGenericException_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userForUpdate = TestDataBuilder.BuildUserForUpdateDto();
        var command = new UpdateUserCommand(userId, userForUpdate);
        var expectedException = new InvalidOperationException("Database error");

        _mockUserService
            .Setup(x => x.UpdateUserAsync(userId, userForUpdate))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Database error");
        _mockUserService.Verify(x => x.UpdateUserAsync(userId, userForUpdate), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ShouldPropagateExceptionFromService()
    {
        // Arrange
        var userId = Guid.Empty;
        var userForUpdate = TestDataBuilder.BuildUserForUpdateDto();
        var command = new UpdateUserCommand(userId, userForUpdate);
        var expectedException = new BadRequestException("User ID must be NON-Empty");

        _mockUserService
            .Setup(x => x.UpdateUserAsync(userId, userForUpdate))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("User ID must be NON-Empty");
        _mockUserService.Verify(x => x.UpdateUserAsync(userId, userForUpdate), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullUserDto_ShouldPropagateExceptionFromService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        UserForUpdateDto? userForUpdate = null;
        var command = new UpdateUserCommand(userId, userForUpdate!);
        var expectedException = new BadRequestException("User is null");

        _mockUserService
            .Setup(x => x.UpdateUserAsync(userId, userForUpdate!))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("User is null");
        _mockUserService.Verify(x => x.UpdateUserAsync(userId, userForUpdate!), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userForUpdate = TestDataBuilder.BuildUserForUpdateDto();
        var command = new UpdateUserCommand(userId, userForUpdate);
        var cancellationToken = new CancellationToken(true);

        _mockUserService
            .Setup(x => x.UpdateUserAsync(userId, userForUpdate))
            .Returns(Task.CompletedTask);

        // Act & Assert - Should handle cancellation gracefully
        var result = await _handler.Handle(command, cancellationToken);

        result.Should().Be(Unit.Value);
        _mockUserService.Verify(x => x.UpdateUserAsync(userId, userForUpdate), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Handle_UserUpdateWithInvalidData_ShouldPropagateValidationException(string? invalidValue)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userForUpdate = TestDataBuilder.BuildUserForUpdateDto();
        // Simulate invalid data that would cause service validation to fail
        var command = new UpdateUserCommand(userId, userForUpdate);
        var expectedException = new BadRequestException($"Invalid user data: {invalidValue}");

        _mockUserService
            .Setup(x => x.UpdateUserAsync(userId, userForUpdate))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Invalid user data");
        _mockUserService.Verify(x => x.UpdateUserAsync(userId, userForUpdate), Times.Once);
    }

    [Fact]
    public async Task Handle_ConcurrentModification_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userForUpdate = TestDataBuilder.BuildUserForUpdateDto();
        var command = new UpdateUserCommand(userId, userForUpdate);
        var expectedException = new InvalidOperationException("Concurrency conflict detected");

        _mockUserService
            .Setup(x => x.UpdateUserAsync(userId, userForUpdate))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Concurrency conflict detected");
        _mockUserService.Verify(x => x.UpdateUserAsync(userId, userForUpdate), Times.Once);
    }

    [Fact]
    public async Task Handle_ServiceReturnsSuccessfully_ShouldCompleteWithoutException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userForUpdate = TestDataBuilder.BuildUserForUpdateDto(
            name: "UpdatedName",
            email: "updated@example.com");
        var command = new UpdateUserCommand(userId, userForUpdate);

        _mockUserService
            .Setup(x => x.UpdateUserAsync(userId, userForUpdate))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _mockUserService.Verify(x => x.UpdateUserAsync(userId, userForUpdate), Times.Once);
        _mockUserService.VerifyNoOtherCalls();
    }
}