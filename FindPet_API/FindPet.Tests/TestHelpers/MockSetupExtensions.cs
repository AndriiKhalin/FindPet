using AutoMapper;
using FindPet.BusinessLogicLayer.Interfaces.IImageService;
using FindPet.BusinessLogicLayer.Interfaces.ILoggerService;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq.Expressions;

namespace FindPet.Tests.TestHelpers;

public static class MockSetupExtensions
{
    public static Mock<IUnitOfWork> SetupUnitOfWorkMock()
    {
        var mock = new Mock<IUnitOfWork>();
        mock.Setup(x => x.User).Returns(new Mock<IUserRepository<User>>().Object);
        return mock;
    }

    public static Mock<IUserRepository<User>> SetupUserRepositoryMock()
    {
        return new Mock<IUserRepository<User>>();
    }

    public static Mock<IMapper> SetupMapperMock()
    {
        return new Mock<IMapper>();
    }

    public static Mock<IManageImage<User>> SetupImageServiceMock()
    {
        return new Mock<IManageImage<User>>();
    }

    public static Mock<ILoggerManager> SetupLoggerMock()
    {
        return new Mock<ILoggerManager>();
    }

    public static void SetupUserExists(this Mock<IUserRepository<User>> userRepoMock, Guid userId, bool exists)
    {
        userRepoMock.Setup(x => x.IsExistAsync(userId))
                   .ReturnsAsync(exists);
    }

    public static void SetupUserExists(this Mock<IUserRepository<User>> userRepoMock, string userName, bool exists)
    {
        userRepoMock.Setup(x => x.IsExistAsync(userName))
                   .ReturnsAsync(exists);
    }

    public static void SetupEmailExists(this Mock<IUserRepository<User>> userRepoMock, string email, bool exists)
    {
        userRepoMock.Setup(x => x.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>()))
                   .ReturnsAsync(exists);
    }

    public static void SetupPhoneExists(this Mock<IUserRepository<User>> userRepoMock, string phone, bool exists)
    {
        userRepoMock.Setup(x => x.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>()))
                   .ReturnsAsync(exists);
    }

    public static void SetupGetUsers(this Mock<IUserRepository<User>> userRepoMock, IEnumerable<User> users)
    {
        userRepoMock.Setup(x => x.Gets())
                   .Returns(users);
    }

    public static void VerifyLogError(this Mock<ILoggerManager> loggerMock, string message)
    {
        loggerMock.Verify(x => x.LogError(
                It.Is<string>(s => s.Contains(message)),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Once);
    }

    public static void VerifyImageUpload(this Mock<IManageImage<User>> imageServiceMock, string photo, Guid userId)
    {
        imageServiceMock.Verify(x => x.UploadPhotoAsync(photo, userId), Times.Once);
    }

    public static void VerifyImageDelete(this Mock<IManageImage<User>> imageServiceMock, string photo)
    {
        imageServiceMock.Verify(x => x.DeletePhoto(photo), Times.Once);
    }

    //---------------------------------------------------------

    public static Mock<IUserRepository<User>> SetupGetsUsers(this Mock<IUserRepository<User>> mockRepo, List<User> users)
    {
        mockRepo.Setup(repo => repo.Gets()).Returns(users);
        return mockRepo;
    }

    public static Mock<IUserRepository<User>> SetupGetUser(this Mock<IUserRepository<User>> mockRepo, Guid userId, User user)
    {
        mockRepo.Setup(repo => repo.GetAsync(userId)).ReturnsAsync(user);
        return mockRepo;
    }

    public static Mock<IUserRepository<User>> SetupGetUserByName(this Mock<IUserRepository<User>> mockRepo, string userName, User user)
    {
        mockRepo.Setup(repo => repo.GetUserAsync(userName)).ReturnsAsync(user);
        return mockRepo;
    }

    public static Mock<IUserRepository<User>> SetupUserExistsById(this Mock<IUserRepository<User>> mockRepo, Guid userId, bool exists)
    {
        mockRepo.Setup(repo => repo.IsExistAsync(userId)).ReturnsAsync(exists);
        return mockRepo;
    }

    public static Mock<IUserRepository<User>> SetupUserExistsByName(this Mock<IUserRepository<User>> mockRepo, string userName, bool exists)
    {
        mockRepo.Setup(repo => repo.IsExistAsync(userName)).ReturnsAsync(exists);
        return mockRepo;
    }

    public static Mock<IUserRepository<User>> SetupUserExistsByPredicate(this Mock<IUserRepository<User>> mockRepo, bool exists)
    {
        mockRepo.Setup(repo => repo.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(exists);
        return mockRepo;
    }

    // Generic repository setup extensions
    public static Mock<IBaseRepository<T>> SetupGets<T>(this Mock<IBaseRepository<T>> mockRepo, List<T> entities) where T : class
    {
        mockRepo.Setup(repo => repo.Gets()).Returns(entities);
        return mockRepo;
    }

    public static Mock<IBaseRepository<T>> SetupGet<T>(this Mock<IBaseRepository<T>> mockRepo, Guid id, T entity) where T : class
    {
        mockRepo.Setup(repo => repo.GetAsync(id)).ReturnsAsync(entity);
        return mockRepo;
    }

    public static Mock<IBaseRepository<T>> SetupExists<T>(this Mock<IBaseRepository<T>> mockRepo, Guid id, bool exists) where T : class
    {
        mockRepo.Setup(repo => repo.IsExistAsync(id)).ReturnsAsync(exists);
        return mockRepo;
    }

    public static Mock<IBaseRepository<T>> SetupExistsByPredicate<T>(this Mock<IBaseRepository<T>> mockRepo, bool exists) where T : class
    {
        mockRepo.Setup(repo => repo.IsExistAsync(It.IsAny<Expression<Func<T, bool>>>()))
            .ReturnsAsync(exists);
        return mockRepo;
    }

    // UnitOfWork setup extensions
    public static Mock<IUnitOfWork> SetupUserRepository(this Mock<IUnitOfWork> mockUow, Mock<IUserRepository<User>> mockRepo)
    {
        mockUow.Setup(uow => uow.User).Returns(mockRepo.Object);
        return mockUow;
    }

    public static Mock<IUnitOfWork> SetupSaveAsync(this Mock<IUnitOfWork> mockUow)
    {
        mockUow.Setup(uow => uow.SaveAsync()).Returns(Task.CompletedTask);
        return mockUow;
    }

    // Logger setup extensions
    public static Mock<ILoggerManager> SetupLogError(this Mock<ILoggerManager> mockLogger)
    {
        mockLogger.Setup(l => l.LogError(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()));
        return mockLogger;
    }

    // AutoMapper setup extensions
    public static Mock<IMapper> SetupMap<TSource, TDestination>(this Mock<IMapper> mockMapper, TSource source, TDestination destination)
    {
        mockMapper.Setup(m => m.Map<TDestination>(source)).Returns(destination);
        return mockMapper;
    }

    public static Mock<IMapper> SetupMapToExisting<TSource, TDestination>(this Mock<IMapper> mockMapper) where TDestination : class
    {
        mockMapper.Setup(m => m.Map(It.IsAny<TSource>(), It.IsAny<TDestination>()));
        return mockMapper;
    }

    // EF Core related mocks
    public static Mock<DbSet<T>> SetupDbSet<T>(this IEnumerable<T> data) where T : class
    {
        var queryableData = data.AsQueryable();
        var mockDbSet = new Mock<DbSet<T>>();

        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());

        return mockDbSet;
    }
}