using System.Linq.Expressions;
using System.Security.Claims;
using AutoMapper;
using FindPet.BusinessLogicLayer.Interfaces.IEntityService;
using FindPet.BusinessLogicLayer.Interfaces.IImageService;
using FindPet.BusinessLogicLayer.Interfaces.IMLService;
using FindPet.BusinessLogicLayer.Services.EntityService;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Entities;
using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Media.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Language.Flow;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace FindPet.Tests.TestHelpers;

public static class MockSetupExtensions
{
    #region Test Service Factory Methods

    public static UserService CreateUserServiceForTesting(
        IEnumerable<User> users = null,
        User singleUser = null,
        bool userExists = true)
    {
        var mockUow = new Mock<IUnitOfWork>();
        var mockMapper = new Mock<IMapper>();
        var mockLogger = new Mock<ILoggerManager>();
        var mockMediaStorageServer = new Mock<IMediaStorageService>();

        var mockUserRepo = new Mock<IUserRepository<User>>();

        if (users != null) mockUserRepo.Setup(r => r.Gets()).Returns(users);

        if (singleUser != null)
        {
            mockUserRepo.Setup(r => r.GetAsync(singleUser.Id)).ReturnsAsync(singleUser);
            mockUserRepo.Setup(r => r.GetUserAsync(singleUser.Name)).ReturnsAsync(singleUser);
        }

        mockUserRepo.Setup(r => r.IsExistAsync(It.IsAny<Guid>())).ReturnsAsync(userExists);
        mockUserRepo.Setup(r => r.IsExistAsync(It.IsAny<string>())).ReturnsAsync(userExists);

        mockUow.Setup(u => u.User).Returns(mockUserRepo.Object);
        mockUow.Setup(u => u.SaveAsync()).Returns(Task.CompletedTask);

        if (singleUser != null) mockMapper.Setup(m => m.Map<User>(It.IsAny<UserForCreateDto>())).Returns(singleUser);

        return new UserService(mockUow.Object, mockMapper.Object, mockLogger.Object, mockMediaStorageServer.Object);
    }

    // Similar factory methods could be created for PetService, AdService, etc.

    #endregion

    public static Mock<IManageImage<User>> SetupImageServiceMock()
    {
        return new Mock<IManageImage<User>>();
    }

    //public static void SetupGetUsers(this Mock<IUserRepository<User>> userRepoMock, IEnumerable<User> users)
    //{
    //    userRepoMock.Setup(x => x.Gets())
    //               .Returns(users);
    //}

    public static void VerifyImageUpload(this Mock<IManageImage<User>> imageServiceMock, string photo, Guid userId)
    {
        imageServiceMock.Verify(x => x.UploadPhotoAsync(photo, userId), Times.Once);
    }

    public static void VerifyFIleDelete(this Mock<IMediaStorageService> mediaStorageService, string photo)
    {
        mediaStorageService.Verify(x => x.DeleteFileAsync(photo), Times.Once);
    }

    //---------------------------------------------------------


    // Generic repository setup extensions

    public static Mock<IBaseRepository<T>> SetupExistsByPredicate<T>(this Mock<IBaseRepository<T>> mockRepo,
        bool exists) where T : class
    {
        mockRepo.Setup(repo => repo.IsExistAsync(It.IsAny<Expression<Func<T, bool>>>()))
            .ReturnsAsync(exists);
        return mockRepo;
    }

    // UnitOfWork setup extensions
    public static Mock<IUnitOfWork> SetupUserRepository(this Mock<IUnitOfWork> mockUow,
        Mock<IUserRepository<User>> mockRepo)
    {
        mockUow.Setup(uow => uow.User).Returns(mockRepo.Object);
        return mockUow;
    }

    public static Mock<IUnitOfWork> SetupSaveAsync(this Mock<IUnitOfWork> mockUow)
    {
        mockUow.Setup(uow => uow.SaveAsync()).Returns(Task.CompletedTask);
        return mockUow;
    }

    #region Controller Testing Mocks

    /// <summary>
    ///     Controller-specific mock setups
    /// </summary>
    public static class ControllerMocks
    {
        public static void SetupControllerContext<T>(T controller, ClaimsPrincipal user = null)
            where T : ControllerBase
        {
            var httpContext = InfrastructureMocks.SetupHttpContext(user).Object;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        public static ClaimsPrincipal CreateTestUserPrincipal(Guid? userId = null, string role = "User")
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, (userId ?? Guid.NewGuid()).ToString()),
                new Claim(ClaimTypes.Email, TestDataBuilder.TestConstants.DEFAULT_EMAIL),
                new Claim(ClaimTypes.Name, TestDataBuilder.TestConstants.DEFAULT_USERNAME),
                new Claim(ClaimTypes.Role, role)
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }
    }

    #endregion


    #region Scenario-Based Mock Builders

    /// <summary>
    ///     Pre-configured mock scenarios for common testing situations
    /// </summary>
    public static class ScenarioMocks
    {
        // Success Scenarios
        public static class Success
        {
            public static Mock<IUnitOfWork> CreateUserFlow()
            {
                var users = TestDataBuilder.BuildUserList();
                var unitOfWork = SetupUnitOfWorkMock();
                var userRepo = UserRepositoryMocks.SetupUserRepository(users);

                unitOfWork.Setup(x => x.User).Returns(userRepo.Object);
                return unitOfWork;
            }

            public static Mock<IUnitOfWork> CreatePetFlow()
            {
                var pets = TestDataBuilder.BuildPetList();
                var unitOfWork = SetupUnitOfWorkMock();
                var petRepo = PetRepositoryMocks.SetupPetRepository(pets);

                unitOfWork.Setup(x => x.Pet).Returns(petRepo.Object);
                return unitOfWork;
            }

            public static Mock<IUnitOfWork> CreateAdFlow()
            {
                var ads = TestDataBuilder.BuildAdList();
                var unitOfWork = SetupUnitOfWorkMock();
                var adRepo = AdRepositoryMocks.SetupAdRepository(ads);

                unitOfWork.Setup(x => x.Ad).Returns(adRepo.Object);
                return unitOfWork;
            }
        }

        // Failure Scenarios
        public static class Failure
        {
            public static Mock<IUnitOfWork> CreateUserNotFound()
            {
                var unitOfWork = SetupUnitOfWorkMock();
                var userRepo = RepositoryMocks.SetupForFailure<User>();

                unitOfWork.Setup(x => x.User).Returns(userRepo.As<IUserRepository<User>>().Object);
                return unitOfWork;
            }

            public static Mock<IUnitOfWork> CreateDatabaseError()
            {
                var unitOfWork = new Mock<IUnitOfWork>();
                unitOfWork.Setup(x => x.SaveAsync()).ThrowsAsync(new Exception("Database connection failed"));
                return unitOfWork;
            }

            public static Mock<IUserService> CreateEmailAlreadyExists()
            {
                var mock = ServiceMocks.SetupUserService();
                mock.Setup(x => x.IsEmailRegisteredAsync(It.IsAny<string>())).ReturnsAsync(true);
                mock.Setup(x => x.CreateUserAsync(It.IsAny<UserForCreateDto>()))
                    .ThrowsAsync(new InvalidOperationException("Email already exists"));
                return mock;
            }
        }

        // Complex Integration Scenarios
        public static class Integration
        {
            public static (Mock<IUnitOfWork>, Mock<IMLService>, Mock<IManageImage<Pet>>) CreatePetWithMLPrediction()
            {
                var unitOfWork = Success.CreatePetFlow();
                var mlService = ServiceMocks.SetupMLService();
                var imageService = ServiceMocks.SetupImageService<Pet>();

                return (unitOfWork, mlService, imageService);
            }

            public static Mock<IUnitOfWork> CreateUserWithPetsAndAds()
            {
                var user = TestDataBuilder.BuildBasicUser();
                var pets = TestDataBuilder.BuildPetList(3, user.Id);
                var ads = TestDataBuilder.BuildAdList(5, user.Id);

                var unitOfWork = SetupUnitOfWorkMock();
                var userRepo = UserRepositoryMocks.SetupForUserWithRelations(user, pets, ads);

                unitOfWork.Setup(x => x.User).Returns(userRepo.Object);
                return unitOfWork;
            }
        }
    }

    #endregion

    #region Generic Mock Factory

    /// <summary>
    ///     Creates a basic mock with common setup
    /// </summary>
    public static Mock<T> CreateMock<T>() where T : class
    {
        return new Mock<T>();
    }

    /// <summary>
    ///     Creates a mock and applies configuration
    /// </summary>
    public static Mock<T> CreateMockWith<T>(Action<Mock<T>> configure) where T : class
    {
        var mock = new Mock<T>();
        configure(mock);
        return mock;
    }

    #endregion

    #region Unit of Work Mocks

    /// <summary>
    ///     Sets up a complete UnitOfWork mock with all repositories
    /// </summary>
    public static Mock<IUnitOfWork> SetupUnitOfWorkMock()
    {
        var mock = new Mock<IUnitOfWork>();

        mock.Setup(x => x.User).Returns(CreateMock<IUserRepository<User>>().Object);
        mock.Setup(x => x.Pet).Returns(CreateMock<IPetRepository>().Object);
        mock.Setup(x => x.Ad).Returns(CreateMock<IAdRepository>().Object);

        mock.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        return mock;
    }

    /// <summary>
    ///     Sets up UnitOfWork with specific repository
    /// </summary>
    public static Mock<IUnitOfWork> SetupUnitOfWorkWith<TRepo>(Mock<TRepo> repository,
        Expression<Func<IUnitOfWork, TRepo>> propertySelector) where TRepo : class
    {
        var mock = new Mock<IUnitOfWork>();
        mock.Setup(propertySelector).Returns(repository.Object);
        mock.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);
        return mock;
    }

    #endregion


    #region Repository Mock Extensions

    // Generic repository setup extensions
    public static Mock<IBaseRepository<T>> SetupGets<T>(this Mock<IBaseRepository<T>> mockRepo, List<T> entities)
        where T : class
    {
        mockRepo.Setup(repo => repo.Gets()).Returns(entities);
        return mockRepo;
    }

    public static Mock<IBaseRepository<T>> SetupGet<T>(this Mock<IBaseRepository<T>> mockRepo, Guid id, T entity)
        where T : class
    {
        mockRepo.Setup(repo => repo.GetAsync(id)).ReturnsAsync(entity);
        return mockRepo;
    }

    public static Mock<IBaseRepository<T>> SetupExists<T>(this Mock<IBaseRepository<T>> mockRepo, Guid id, bool exists)
        where T : class
    {
        mockRepo.Setup(repo => repo.IsExistAsync(id)).ReturnsAsync(exists);
        return mockRepo;
    }

    public static Mock<IBaseRepository<T>> SetupExistsByPredicate<T>(
        this Mock<IBaseRepository<T>> mockRepo,
        Expression<Func<T, bool>> predicate,
        bool exists) where T : class
    {
        mockRepo.Setup(repo => repo.IsExistAsync(It.IsAny<Expression<Func<T, bool>>>()))
            .ReturnsAsync(exists);
        return mockRepo;
    }

    public static Mock<IBaseRepository<T>> SetupCreate<T>(this Mock<IBaseRepository<T>> mockRepo) where T : class
    {
        mockRepo.Setup(repo => repo.CreateAsync(It.IsAny<T>())).Returns(Task.CompletedTask);
        return mockRepo;
    }

    public static Mock<IBaseRepository<T>> SetupUpdate<T>(this Mock<IBaseRepository<T>> mockRepo) where T : class
    {
        mockRepo.Setup(repo => repo.UpdateAsync(It.IsAny<T>())).Returns(Task.CompletedTask);
        return mockRepo;
    }

    public static Mock<IBaseRepository<T>> SetupDelete<T>(this Mock<IBaseRepository<T>> mockRepo, Guid id)
        where T : class
    {
        mockRepo.Setup(repo => repo.DeleteAsync(id)).Returns(Task.CompletedTask);
        return mockRepo;
    }

    public static void VerifyCreate<T>(this Mock<IBaseRepository<T>> mockRepo, Times times) where T : class
    {
        mockRepo.Verify(repo => repo.CreateAsync(It.IsAny<T>()), times);
    }

    public static void VerifyUpdate<T>(this Mock<IBaseRepository<T>> mockRepo, Times times) where T : class
    {
        mockRepo.Verify(repo => repo.UpdateAsync(It.IsAny<T>()), times);
    }

    public static void VerifyDelete<T>(this Mock<IBaseRepository<T>> mockRepo, Guid id, Times times) where T : class
    {
        mockRepo.Verify(repo => repo.DeleteAsync(id), times);
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

    /// <summary>
    ///     Generic repository setup for any entity
    /// </summary>
    public static class RepositoryMocks
    {
        /// <summary>
        ///     Sets up basic CRUD operations for any repository
        /// </summary>
        public static Mock<IBaseRepository<T>> SetupBasicCrud<T>(
            List<T> data = null,
            T getResult = null,
            bool existsResult = true) where T : class
        {
            var mock = new Mock<IBaseRepository<T>>();

            // Gets collection
            mock.Setup(x => x.Gets()).Returns(data ?? new List<T>());

            // Get single
            mock.Setup(x => x.GetAsync(It.IsAny<Guid>())).ReturnsAsync(getResult);

            // Exists checks
            mock.Setup(x => x.IsExistAsync(It.IsAny<Guid>())).ReturnsAsync(existsResult);
            mock.Setup(x => x.IsExistAsync(It.IsAny<Expression<Func<T, bool>>>())).ReturnsAsync(existsResult);

            // Create/Update/Delete
            mock.Setup(x => x.CreateAsync(It.IsAny<T>())).Returns(Task.CompletedTask);
            mock.Setup(x => x.UpdateAsync(It.IsAny<T>())).Returns(Task.CompletedTask);
            mock.Setup(x => x.DeleteAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

            return mock;
        }

        /// <summary>
        ///     Sets up repository with predefined data
        /// </summary>
        public static Mock<IBaseRepository<T>> SetupWithData<T>(List<T> data) where T : class
        {
            return SetupBasicCrud(data, data?.FirstOrDefault());
        }

        /// <summary>
        ///     Sets up repository for failure scenarios
        /// </summary>
        public static Mock<IBaseRepository<T>> SetupForFailure<T>() where T : class
        {
            var mock = new Mock<IBaseRepository<T>>();

            mock.Setup(x => x.GetAsync(It.IsAny<Guid>())).ReturnsAsync((T)null);
            mock.Setup(x => x.IsExistAsync(It.IsAny<Guid>())).ReturnsAsync(false);
            mock.Setup(x => x.CreateAsync(It.IsAny<T>())).ThrowsAsync(new InvalidOperationException("Database error"));

            return mock;
        }
    }

    #endregion

    #region User Repository Mocks

    public static Mock<IUserRepository<User>> SetupUserRepositoryMock()
    {
        return new Mock<IUserRepository<User>>();
    }

    public static Mock<IUserRepository<User>> SetupGetUser(this Mock<IUserRepository<User>> mockRepo, Guid userId,
        User user)
    {
        mockRepo.Setup(repo => repo.GetAsync(userId)).ReturnsAsync(user);
        return mockRepo;
    }

    public static Mock<IUserRepository<User>> SetupGetUserByName(this Mock<IUserRepository<User>> mockRepo,
        string userName, User user)
    {
        mockRepo.Setup(repo => repo.GetUserAsync(userName)).ReturnsAsync(user);
        return mockRepo;
    }

    public static Mock<IUserRepository<User>> SetupGetUsers(this Mock<IUserRepository<User>> mockRepo,
        IEnumerable<User> users)
    {
        mockRepo.Setup(repo => repo.Gets()).Returns(users);
        return mockRepo;
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

    public static Mock<IUserRepository<User>> SetupUserExistsByPredicate(this Mock<IUserRepository<User>> mockRepo,
        bool exists)
    {
        mockRepo.Setup(repo => repo.IsExistAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(exists);
        return mockRepo;
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

    /// <summary>
    ///     Comprehensive User repository mock setup
    /// </summary>
    public static class UserRepositoryMocks
    {
        public static Mock<IUserRepository<User>> SetupUserRepository(List<User> users = null)
        {
            users ??= TestDataBuilder.BuildUserList();
            var mock = RepositoryMocks.SetupBasicCrud(users, users.FirstOrDefault()).As<IUserRepository<User>>();

            // User-specific methods
            mock.Setup(x => x.GetUserAsync(It.IsAny<string>()))
                .ReturnsAsync((string username) => users.FirstOrDefault(u => u.Name == username));

            mock.Setup(x => x.GetUserAsync(It.IsAny<string>()))
                .ReturnsAsync((string email) => users.FirstOrDefault(u => u.Email == email));

            mock.Setup(x => x.IsExistAsync(It.IsAny<string>()))
                .ReturnsAsync((string username) => users.Any(u => u.Name == username));

            // Email/Phone validation
            mock.Setup(x => x.IsExistAsync(It.IsAny<string>()))
                .ReturnsAsync((string email) => users.Any(u => u.Email == email));

            mock.Setup(x => x.IsExistAsync(It.IsAny<string>()))
                .ReturnsAsync((string phone) => users.Any(u => u.PhoneNumber == phone));

            return mock;
        }

        //public static Mock<IUserRepository<User>> SetupForEmailExists(string email, bool exists = true)
        //{
        //    var mock = new Mock<IUserRepository<User>>();
        //    mock.Setup(x => x.IsEmailExistsAsync(email)).ReturnsAsync(exists);
        //    return mock;
        //}

        //public static Mock<IUserRepository<User>> SetupForPhoneExists(string phone, bool exists = true)
        //{
        //    var mock = new Mock<IUserRepository<User>>();
        //    mock.Setup(x => x.IsPhoneExistsAsync(phone)).ReturnsAsync(exists);
        //    return mock;
        //}

        public static Mock<IUserRepository<User>> SetupForUserWithRelations(User user, List<Pet> pets = null,
            List<Ad> ads = null)
        {
            var mock = new Mock<IUserRepository<User>>();

            mock.Setup(x => x.GetAsync(user.Id)).ReturnsAsync(user);
            //mock.Setup(x => x.GetUserWithPetsAsync(user.Id)).ReturnsAsync(user);
            //mock.Setup(x => x.GetUserWithAdsAsync(user.Id)).ReturnsAsync(user);

            //if (pets != null)
            //{
            //    mock.Setup(x => x.GetUserPetsAsync(user.Id)).ReturnsAsync(pets);
            //}

            //if (ads != null)
            //{
            //    mock.Setup(x => x.GetUserAdsAsync(user.Id)).ReturnsAsync(ads);
            //}

            return mock;
        }
    }

    #endregion


    #region Pet Repository Mocks

    public static Mock<IPetRepository> SetupPetRepositoryMock()
    {
        return new Mock<IPetRepository>();
    }

    public static Mock<IPetRepository> SetupGetPet(this Mock<IPetRepository> mockRepo, Guid petId, Pet pet)
    {
        mockRepo.Setup(repo => repo.GetAsync(petId)).ReturnsAsync(pet);
        return mockRepo;
    }

    public static Mock<IPetRepository> SetupGetPets(this Mock<IPetRepository> mockRepo, IEnumerable<Pet> pets)
    {
        mockRepo.Setup(repo => repo.Gets()).Returns(pets);
        return mockRepo;
    }

    public static Mock<IPetRepository> SetupPetExists(this Mock<IPetRepository> mockRepo, Guid petId, bool exists)
    {
        mockRepo.Setup(repo => repo.IsExistAsync(petId)).ReturnsAsync(exists);
        return mockRepo;
    }

    public static Mock<IPetRepository> SetupPetExistsByName(this Mock<IPetRepository> mockRepo, string petName,
        bool exists)
    {
        mockRepo.Setup(repo => repo.IsExistAsync(petName)).ReturnsAsync(exists);
        return mockRepo;
    }

    /// <summary>
    ///     Comprehensive Pet repository mock setup
    /// </summary>
    public static class PetRepositoryMocks
    {
        public static Mock<IPetRepository> SetupPetRepository(List<Pet> pets = null)
        {
            pets ??= TestDataBuilder.BuildPetList();
            var mock = RepositoryMocks.SetupBasicCrud(pets, pets.FirstOrDefault()).As<IPetRepository>();

            // Pet-specific methods
            //mock.Setup(x => x.GetPetsByOwnerAsync(It.IsAny<Guid>()))
            //    .ReturnsAsync((Guid ownerId) => pets.Where(p => p.OwnerId == ownerId).ToList());

            //mock.Setup(x => x.GetPetsByTypeAsync(It.IsAny<PetType>()))
            //    .ReturnsAsync((PetType type) => pets.Where(p => p.PetType == type).ToList());

            //mock.Setup(x => x.GetLostPetsAsync())
            //    .ReturnsAsync(pets.Where(p => p.Status == PetStatus.Lost).ToList());

            //mock.Setup(x => x.GetFoundPetsAsync())
            //    .ReturnsAsync(pets.Where(p => p.Status == PetStatus.Found).ToList());

            //mock.Setup(x => x.SearchPetsAsync(It.IsAny<string>(), It.IsAny<PetType?>(), It.IsAny<string>()))
            //    .ReturnsAsync((string query, PetType? type, string location) =>
            //        pets.Where(p =>
            //            (string.IsNullOrEmpty(query) || p.Name.Contains(query)) &&
            //            (!type.HasValue || p.PetType == type) &&
            //            (string.IsNullOrEmpty(location) || p.LastSeenLocation.Contains(location))
            //        ).ToList());

            mock.Setup(x => x.IsExistAsync(It.IsAny<string>()))
                .ReturnsAsync((string petName) => pets.Any(p => p.Nickname == petName));

            return mock;
        }

        public static Mock<IPetRepository> SetupForMLPrediction()
        {
            //mock.Setup(x => x.GetSimilarPetsAsync(It.IsAny<string>(), It.IsAny<PetType>()))
            //    .ReturnsAsync(TestDataBuilder.BuildPetList(3));

            var mock = new Mock<IPetRepository>();
            var pets = TestDataBuilder.BuildPetList();
            mock.Setup(x => x.Gets()).Returns(pets);
            return mock;
        }

        public static Mock<IPetRepository> SetupWithOwnerValidation(Guid ownerId, bool ownerExists = true)
        {
            var mock = new Mock<IPetRepository>();
            mock.Setup(x => x.IsExistAsync(It.IsAny<Expression<Func<Pet, bool>>>()))
                .ReturnsAsync(ownerExists);

            //mock.Setup(x => x.ValidateOwnershipAsync(It.IsAny<Guid>(), ownerId))
            //    .ReturnsAsync(ownerExists);

            return mock;
        }
    }

    #endregion

    #region Ad Repository Mocks

    public static Mock<IAdRepository> SetupAdRepositoryMock()
    {
        return new Mock<IAdRepository>();
    }

    public static Mock<IAdRepository> SetupGetAd(this Mock<IAdRepository> mockRepo, Guid adId, Ad ad)
    {
        mockRepo.Setup(repo => repo.GetAsync(adId)).ReturnsAsync(ad);
        return mockRepo;
    }

    public static Mock<IAdRepository> SetupGetAds(this Mock<IAdRepository> mockRepo, IEnumerable<Ad> ads)
    {
        mockRepo.Setup(repo => repo.Gets()).Returns(ads);
        return mockRepo;
    }

    public static Mock<IAdRepository> SetupAdExists(this Mock<IAdRepository> mockRepo, Guid adId, bool exists)
    {
        mockRepo.Setup(repo => repo.IsExistAsync(adId)).ReturnsAsync(exists);
        return mockRepo;
    }

    /// <summary>
    ///     Comprehensive Ad repository mock setup
    /// </summary>
    public static class AdRepositoryMocks
    {
        public static Mock<IAdRepository> SetupAdRepository(List<Ad> ads = null)
        {
            ads ??= TestDataBuilder.BuildAdList();
            var mock = RepositoryMocks.SetupBasicCrud(ads, ads.FirstOrDefault()).As<IAdRepository>();

            // Ad-specific methods
            //mock.Setup(x => x.GetAdsByUserAsync(It.IsAny<Guid>()))
            //    .ReturnsAsync((Guid userId) => ads.Where(a => a.UserId == userId).ToList());

            //mock.Setup(x => x.GetAdsByPetAsync(It.IsAny<Guid>()))
            //    .ReturnsAsync((Guid petId) => ads.Where(a => a.PetId == petId).ToList());

            //mock.Setup(x => x.GetAdsByTypeAsync(It.IsAny<AdType>()))
            //    .ReturnsAsync((AdType type) => ads.Where(a => a.AdType == type).ToList());

            //mock.Setup(x => x.GetActiveAdsAsync())
            //    .ReturnsAsync(ads.Where(a => a.Status == AdStatus.Active).ToList());

            //mock.Setup(x => x.SearchAdsAsync(It.IsAny<string>(), It.IsAny<AdType?>(), It.IsAny<string>()))
            //    .ReturnsAsync((string query, AdType? type, string location) =>
            //        ads.Where(a =>
            //            (string.IsNullOrEmpty(query) || a.Title.Contains(query)) &&
            //            (!type.HasValue || a.AdType == type) &&
            //            (string.IsNullOrEmpty(location) || a.Location.Contains(location))
            //        ).ToList());

            //mock.Setup(x => x.IncrementViewCountAsync(It.IsAny<Guid>()))
            //    .Returns(Task.CompletedTask);

            mock.Setup(x => x.IsExistAsync(It.IsAny<Expression<Func<Ad, bool>>>()))
                .ReturnsAsync(true);

            return mock;
        }

        public static Mock<IAdRepository> SetupForLocationSearch(string location, List<Ad> locationAds)
        {
            var mock = new Mock<IAdRepository>();

            //mock.Setup(x => x.GetAdsByLocationAsync(location))
            //    .ReturnsAsync(locationAds);

            //mock.Setup(x => x.GetAdsInRadiusAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            //    .ReturnsAsync(locationAds);

            mock.Setup(x =>
                    x.IsExistAsync(It.Is<Expression<Func<Ad, bool>>>(expr => expr.ToString().Contains("Location"))))
                .ReturnsAsync(locationAds.Any());

            return mock;
        }
    }

    #endregion


    #region Service Layer Mocks

    // User Service Mocks
    public static Mock<IUserService> SetupUserServiceMock()
    {
        return new Mock<IUserService>();
    }

    public static Mock<IUserService> SetupGetUserById(this Mock<IUserService> mockService, Guid userId, User user)
    {
        mockService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(user);
        return mockService;
    }

    public static Mock<IUserService> SetupGetUserByName(this Mock<IUserService> mockService, string userName, User user)
    {
        mockService.Setup(s => s.GetUserByNameAsync(userName)).ReturnsAsync(user);
        return mockService;
    }

    public static Mock<IUserService> SetupGetUsers(this Mock<IUserService> mockService, IEnumerable<User> users)
    {
        mockService.Setup(s => s.GetUsers()).Returns(users);
        return mockService;
    }

    public static Mock<IUserService> SetupCreateUser(this Mock<IUserService> mockService, UserForCreateDto dto,
        User createdUser)
    {
        mockService.Setup(s => s.CreateUserAsync(It.Is<UserForCreateDto>(x =>
            x.Name == dto.Name &&
            x.Email == dto.Email))).ReturnsAsync(createdUser);
        return mockService;
    }

    public static Mock<IUserService> SetupUpdateUser(this Mock<IUserService> mockService)
    {
        mockService.Setup(s => s.UpdateUserAsync(It.IsAny<Guid>(), It.IsAny<UserForUpdateDto>()))
            .Returns(Task.CompletedTask);
        return mockService;
    }

    public static ISetup<IUserService, Task> SetupDeleteUserAsync(
        this Mock<IUserService> mock, Guid userId)
    {
        return mock.Setup(x => x.DeleteUserAsync(userId));
    }

    public static Mock<IUserService> SetupDeleteUser(this Mock<IUserService> mockService)
    {
        mockService.Setup(s => s.DeleteUserAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        return mockService;
    }

    // Pet Service Mocks
    public static Mock<IPetService> SetupPetServiceMock()
    {
        return new Mock<IPetService>();
    }

    public static Mock<IPetService> SetupGetPetById(this Mock<IPetService> mockService, Guid petId, Pet pet)
    {
        mockService.Setup(s => s.GetPetByIdAsync(petId)).ReturnsAsync(pet);
        return mockService;
    }

    public static Mock<IPetService> SetupGetPets(this Mock<IPetService> mockService, IEnumerable<Pet> pets)
    {
        mockService.Setup(s => s.GetPets()).Returns(pets);
        return mockService;
    }

    public static Mock<IPetService> SetupCreatePet(this Mock<IPetService> mockService, Guid userId, PetForCreateDto dto,
        Pet createdPet)
    {
        mockService.Setup(s => s.CreatePetAsync(userId, It.Is<PetForCreateDto>(x => x.Nickname == dto.Nickname)))
            .ReturnsAsync(createdPet);
        return mockService;
    }

    public static Mock<IPetService> SetupUpdatePet(this Mock<IPetService> mockService)
    {
        mockService.Setup(s => s.UpdatePetAsync(It.IsAny<Guid>(), It.IsAny<PetForUpdateDto>()))
            .Returns(Task.CompletedTask);
        return mockService;
    }

    public static Mock<IPetService> SetupDeletePet(this Mock<IPetService> mockService)
    {
        mockService.Setup(s => s.DeletePetAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        return mockService;
    }

    // Ad Service Mocks
    public static Mock<IAdService> SetupAdServiceMock()
    {
        return new Mock<IAdService>();
    }

    public static Mock<IAdService> SetupGetAdById(this Mock<IAdService> mockService, Guid adId, Ad ad)
    {
        mockService.Setup(s => s.GetAdAsync(adId)).ReturnsAsync(ad);
        return mockService;
    }

    public static Mock<IAdService> SetupGetAds(this Mock<IAdService> mockService, IEnumerable<Ad> ads)
    {
        mockService.Setup(s => s.GetAds()).Returns(ads);
        return mockService;
    }

    public static Mock<IAdService> SetupCreateAd(this Mock<IAdService> mockService, Guid petId, Guid userId,
        AdForCreateDto dto, Ad createdAd)
    {
        mockService.Setup(s =>
                s.CreateAdAsync(petId, userId, It.Is<AdForCreateDto>(x => x.Description == dto.Description)))
            .ReturnsAsync(createdAd);
        return mockService;
    }

    public static Mock<IAdService> SetupUpdateAd(this Mock<IAdService> mockService)
    {
        mockService.Setup(s => s.UpdateAdAsync(It.IsAny<Guid>(), It.IsAny<AdForUpdateDto>()))
            .Returns(Task.CompletedTask);
        return mockService;
    }

    public static Mock<IAdService> SetupDeleteAd(this Mock<IAdService> mockService)
    {
        mockService.Setup(s => s.DeleteAdAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        return mockService;
    }

    /// <summary>
    ///     Service layer mock setups
    /// </summary>
    public static class ServiceMocks
    {
        // User Service Mocks
        public static Mock<IUserService> SetupUserService(List<User> users = null)
        {
            users ??= TestDataBuilder.BuildUserList();
            var mock = new Mock<IUserService>();

            mock.Setup(x => x.GetUsers()).Returns(users);
            mock.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid id) => users.FirstOrDefault(u => u.Id == id));

            mock.Setup(x => x.IsEmailRegisteredAsync(It.IsAny<string>()))
                .ReturnsAsync((string email) => users.Any(u => u.Email == email));

            mock.Setup(x => x.CreateUserAsync(It.IsAny<UserForCreateDto>()))
                .ReturnsAsync((UserForCreateDto dto) => TestDataBuilder.BuildBasicUser());

            mock.Setup(x => x.UpdateUserAsync(It.IsAny<Guid>(), It.IsAny<UserForUpdateDto>()))
                .Returns(Task.CompletedTask);

            mock.Setup(x => x.DeleteUserAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            //mock.Setup(x => x.IsEmailExistsAsync(It.IsAny<string>()))
            //    .ReturnsAsync((string email) => users.Any(u => u.Email == email));

            return mock;
        }

        // Pet Service Mocks
        public static Mock<IPetService> SetupPetService(List<Pet> pets = null)
        {
            pets ??= TestDataBuilder.BuildPetList();
            var mock = new Mock<IPetService>();

            mock.Setup(x => x.GetPets()).Returns(pets);
            mock.Setup(x => x.GetPetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid id) => pets.FirstOrDefault(p => p.Id == id));

            mock.Setup(x => x.CreatePetAsync(It.IsAny<Guid>(), It.IsAny<PetForCreateDto>()))
                .ReturnsAsync((Guid userId, PetForCreateDto dto) => TestDataBuilder.BuildBasicPet(userId: userId));

            mock.Setup(x => x.UpdatePetAsync(It.IsAny<Guid>(), It.IsAny<PetForUpdateDto>()))
                .Returns(Task.CompletedTask);

            return mock;
        }

        // Ad Service Mocks
        public static Mock<IAdService> SetupAdService(List<Ad> ads = null)
        {
            ads ??= TestDataBuilder.BuildAdList();
            var mock = new Mock<IAdService>();

            mock.Setup(x => x.GetAds()).Returns(ads);
            mock.Setup(x => x.GetAdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid id) => ads.FirstOrDefault(a => a.Id == id));

            mock.Setup(x => x.CreateAdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<AdForCreateDto>()))
                .ReturnsAsync((Guid petId, Guid userId, AdForCreateDto dto) => TestDataBuilder.BuildAd(userId: userId));

            return mock;
        }

        // ML Service Mocks
        public static Mock<IMLService> SetupMLService()
        {
            var mock = new Mock<IMLService>();

            mock.Setup(x => x.PredictAsync(It.IsAny<string>()))
                .ReturnsAsync("Unknown");
            //mock.Setup(x => x.PredictBreedAsync(It.IsAny<byte[]>()))
            //    .ReturnsAsync(TestDataBuilder.MLTestData.CreatePredictionResults());

            //mock.Setup(x => x.PredictBreedAsync(It.IsAny<IFormFile>()))
            //    .ReturnsAsync(TestDataBuilder.MLTestData.CreatePredictionResults());

            //mock.Setup(x => x.IsModelTrainedAsync())
            //    .ReturnsAsync(true);

            return mock;
        }

        // Image Service Mocks
        public static Mock<IManageImage<T>> SetupImageService<T>() where T : class
        {
            var mock = new Mock<IManageImage<T>>();

            mock.Setup(x => x.UploadPhotoAsync(It.IsAny<string>(), It.IsAny<Guid>()))
                .ReturnsAsync("uploaded-photo-path.jpg");

            mock.Setup(x => x.DeletePhoto(It.IsAny<string>()));

            //mock.Setup(x => x.ValidateImageAsync(It.IsAny<IFormFile>()))
            //    .ReturnsAsync(true);

            return mock;
        }

        // Logger Service Mocks
        public static Mock<ILoggerManager> SetupLoggerService()
        {
            var mock = new Mock<ILoggerManager>();

            // Setup all logging methods
            mock.Setup(x => x.LogInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            mock.Setup(x => x.LogWarn(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            mock.Setup(x => x.LogDebug(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            mock.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));

            return mock;
        }
    }

    #endregion


    #region Infrastructure Mocks

    public static Mock<IMapper> SetupMapperMock()
    {
        return new Mock<IMapper>();
    }

    public static void SetupMap<TSource, TDestination>(this Mock<IMapper> mockMapper, TSource source,
        TDestination destination)
    {
        mockMapper.Setup(m => m.Map<TDestination>(source)).Returns(destination);
    }

    public static Mock<IMapper> SetupMapToExisting<TSource, TDestination>(this Mock<IMapper> mockMapper)
        where TDestination : class
    {
        mockMapper.Setup(m => m.Map(It.IsAny<TSource>(), It.IsAny<TDestination>()));
        return mockMapper;
    }

    public static Mock<ILoggerManager> SetupLoggerMock()
    {
        return new Mock<ILoggerManager>();
    }

    public static Mock<ILoggerManager> SetupLogError(this Mock<ILoggerManager> mockLogger)
    {
        mockLogger.Setup(l => l.LogError(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()));
        return mockLogger;
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

    public static Mock<IManageImage<T>> SetupImageServiceMock<T>() where T : class
    {
        return new Mock<IManageImage<T>>();
    }

    public static Mock<IManageImage<T>> SetupUploadPhoto<T>(this Mock<IManageImage<T>> mockImageService,
        string photoPath)
        where T : class
    {
        mockImageService.Setup(s => s.UploadPhotoAsync(It.IsAny<IFormFile>(), It.IsAny<Guid?>()))
            .ReturnsAsync(photoPath);
        mockImageService.Setup(s => s.UploadPhotoAsync(It.IsAny<string>(), It.IsAny<Guid?>()))
            .ReturnsAsync(photoPath);
        return mockImageService;
    }

    public static Mock<IMediaStorageService> SetupPredictTypePet(
        this Mock<IMediaStorageService> mockMediaStorageService, string photoPath)
    {
        mockMediaStorageService.Setup(x => x.FileExistsAsync(photoPath)).ReturnsAsync(true);

        var mockImageStream = new MemoryStream(new byte[] { 0x00, 0x01, 0x02 });
        mockMediaStorageService.Setup(x => x.GetFileAsync(photoPath))
            .ReturnsAsync(mockImageStream);

        return mockMediaStorageService;
    }

    /// <summary>
    ///     Infrastructure layer mock setups
    /// </summary>
    public static class InfrastructureMocks
    {
        // AutoMapper Mocks
        public static Mock<IMapper> SetupMapper()
        {
            var mock = new Mock<IMapper>();

            // User mappings
            mock.Setup(x => x.Map<User>(It.IsAny<UserForCreateDto>()))
                .Returns((UserForCreateDto dto) => TestDataBuilder.BuildBasicUser());

            mock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
                .Returns((User user) => new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email
                });

            // Pet mappings
            mock.Setup(x => x.Map<Pet>(It.IsAny<PetForCreateDto>()))
                .Returns((PetForCreateDto dto) => TestDataBuilder.BuildBasicPet());

            mock.Setup(x => x.Map<PetDto>(It.IsAny<Pet>()))
                .Returns((Pet pet) => new PetDto
                {
                    Id = pet.Id,
                    Nickname = pet.Nickname,
                    Breed = pet.Breed
                });

            // Ad mappings
            mock.Setup(x => x.Map<Ad>(It.IsAny<AdForCreateDto>()))
                .Returns((AdForCreateDto dto) => TestDataBuilder.BuildAd());

            mock.Setup(x => x.Map<AdDto>(It.IsAny<Ad>()))
                .Returns((Ad ad) => new AdDto
                {
                    Id = ad.Id,
                    Description = ad.Description,
                    Location = ad.Location
                });

            // Generic mapping setup
            mock.Setup(x => x.Map(It.IsAny<object>(), It.IsAny<object>()));

            return mock;
        }

        // HttpContext Mocks
        public static Mock<HttpContext> SetupHttpContext(ClaimsPrincipal user = null)
        {
            var mock = new Mock<HttpContext>();

            mock.Setup(x => x.User).Returns(user ?? CreateTestUser());
            mock.Setup(x => x.Request.Headers).Returns(new HeaderDictionary());
            mock.Setup(x => x.Response.Headers).Returns(new HeaderDictionary());

            return mock;
        }

        // Identity Mocks
        public static Mock<UserManager<User>> SetupUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            var mock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            mock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            mock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((string email) => TestDataBuilder.BuildBasicUser().With(u => u.Email = email));

            mock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            return mock;
        }

        // SignInManager Mocks
        public static Mock<SignInManager<User>> SetupSignInManager(Mock<UserManager<User>> userManager = null)
        {
            userManager ??= SetupUserManager();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();

            var mock = new Mock<SignInManager<User>>(
                userManager.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null, null, null, null);

            mock.Setup(x =>
                    x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Success);

            return mock;
        }

        private static ClaimsPrincipal CreateTestUser()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, TestDataBuilder.TestConstants.DEFAULT_EMAIL),
                new Claim(ClaimTypes.Name, TestDataBuilder.TestConstants.DEFAULT_USERNAME)
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }
    }

    #endregion


    #region MediatR Mocks

    public static Mock<IMediator> SetupMediatorMock()
    {
        return new Mock<IMediator>();
    }

    public static Mock<IMediator> SetupSend<TRequest, TResponse>(this Mock<IMediator> mediatorMock, TResponse response)
        where TRequest : IRequest<TResponse>
    {
        mediatorMock.Setup(m => m.Send(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        return mediatorMock;
    }

    public static Mock<IMediator> SetupSend<TRequest, TResponse>(this Mock<IMediator> mediatorMock,
        Func<TRequest, bool> match, TResponse response)
        where TRequest : IRequest<TResponse>
    {
        mediatorMock.Setup(m => m.Send(It.Is<TRequest>(r => match(r)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        return mediatorMock;
    }

    #endregion

    #region HTTP Context Mocks

    public static Mock<HttpContext> SetupHttpContextWithUser(string userId = null, string[] roles = null)
    {
        var user = new ClaimsPrincipal();

        if (userId != null || roles != null)
        {
            var claims = new List<Claim>();

            if (userId != null) claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));

            if (roles != null)
                foreach (var role in roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));

            var identity = new ClaimsIdentity(claims, "TestAuth");
            user = new ClaimsPrincipal(identity);
        }

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.User).Returns(user);

        return httpContextMock;
    }

    public static Mock<IFormFile> SetupMockFormFile(string fileName, string contentType, long length)
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.Length).Returns(length);

        return fileMock;
    }

    #endregion

    #region Exception Testing Mocks

    public static Mock<IUserRepository<User>> SetupUserRepositoryWithException<TException>(string methodName)
        where TException : Exception, new()
    {
        var mock = new Mock<IUserRepository<User>>();

        switch (methodName.ToLower())
        {
            case "getasync":
                mock.Setup(r => r.GetAsync(It.IsAny<Guid>())).ThrowsAsync(new TException());
                break;
            case "getuserasync":
                mock.Setup(r => r.GetUserAsync(It.IsAny<string>())).ThrowsAsync(new TException());
                break;
            case "gets":
                mock.Setup(r => r.Gets()).Throws(new TException());
                break;
            case "createasync":
                mock.Setup(r => r.CreateAsync(It.IsAny<User>())).ThrowsAsync(new TException());
                break;
            case "updateasync":
                mock.Setup(r => r.UpdateAsync(It.IsAny<User>())).ThrowsAsync(new TException());
                break;
            case "deleteasync":
                mock.Setup(r => r.DeleteAsync(It.IsAny<Guid>())).ThrowsAsync(new TException());
                break;
            default:
                throw new ArgumentException($"Method name '{methodName}' not supported for exception setup.");
        }

        return mock;
    }

    public static Mock<IPetRepository> SetupPetRepositoryWithException<TException>(string methodName)
        where TException : Exception, new()
    {
        var mock = new Mock<IPetRepository>();

        switch (methodName.ToLower())
        {
            case "getasync":
                mock.Setup(r => r.GetAsync(It.IsAny<Guid>())).ThrowsAsync(new TException());
                break;
            case "gets":
                mock.Setup(r => r.Gets()).Throws(new TException());
                break;
            case "createasync":
                mock.Setup(r => r.CreateAsync(It.IsAny<Pet>())).ThrowsAsync(new TException());
                break;
            case "updateasync":
                mock.Setup(r => r.UpdateAsync(It.IsAny<Pet>())).ThrowsAsync(new TException());
                break;
            case "deleteasync":
                mock.Setup(r => r.DeleteAsync(It.IsAny<Guid>())).ThrowsAsync(new TException());
                break;
            default:
                throw new ArgumentException($"Method name '{methodName}' not supported for exception setup.");
        }

        return mock;
    }

    #endregion
}

#region Verification Extensions

/// <summary>
///     Mock verification helpers
/// </summary>
public static class VerificationExtensions
{
    // Repository Verifications
    public static void VerifyRepositoryCall<T>(this Mock<IBaseRepository<T>> mock,
        Expression<Action<IBaseRepository<T>>> expression,
        Times times) where T : class
    {
        mock.Verify(expression, times);
    }

    // Service Verifications
    public static void VerifyServiceCall<T>(this Mock<T> mock,
        Expression<Action<T>> expression,
        Times times) where T : class
    {
        mock.Verify(expression, times);
    }

    // Logger Verifications
    public static void VerifyLogCall(this Mock<ILoggerManager> mock,
        string expectedMessage,
        Times times)
    {
        mock.Verify(x => x.LogInfo(
                It.Is<string>(s => s.Contains(expectedMessage)),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()),
            times);
    }

    public static void VerifyErrorLogCall(this Mock<ILoggerManager> mock,
        string expectedMessage,
        Times times)
    {
        mock.Verify(x => x.LogError(
                It.Is<string>(s => s.Contains(expectedMessage)),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()),
            times);
    }

    // Image Service Verifications
    public static void VerifyImageUpload<T>(this Mock<IManageImage<T>> mock,
        Times times) where T : class
    {
        mock.Verify(x => x.UploadPhotoAsync(It.IsAny<string>(), It.IsAny<Guid>()), times);
    }

    public static void VerifyImageDelete<T>(this Mock<IManageImage<T>> mock,
        Times times) where T : class
    {
        mock.Verify(x => x.DeletePhoto(It.IsAny<string>()), times);
    }
}

#endregion