using FindPet.Domain.DTOs.AuthDTOs;
using FindPet.Domain.DTOs.EntitiesDTOs.AdDTO;
using FindPet.Domain.DTOs.EntitiesDTOs.PetDTO;
using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Net;

namespace FindPet.Tests.TestHelpers;

public static class TestDataBuilder
{
    #region Constants for Test Data

    public static class TestConstants
    {
        public const string DEFAULT_EMAIL = "test@findpet.com";
        public const string DEFAULT_PASSWORD = "TestPassword123!";
        public const string DEFAULT_PHONE = "+380501234567";
        public const string DEFAULT_USERNAME = "testuser";
        public const string DEFAULT_FIRST_NAME = "John";
        public const string DEFAULT_LAST_NAME = "Doe";
        public const string DEFAULT_PHOTO_URL = "https://example.com/photo.jpg";

        public const string DEFAULT_PET_NAME = "Fluffy";
        public const string DEFAULT_PET_DESCRIPTION = "A beautiful and friendly pet";
        public const string DEFAULT_BREED = "Golden Retriever";

        public const string DEFAULT_AD_TITLE = "Lost Pet - Please Help";
        public const string DEFAULT_AD_DESCRIPTION = "Lost my beloved pet, please contact if found";
        public const string DEFAULT_LOCATION = "Kyiv, Ukraine";

        public const string INVALID_EMAIL = "invalid-email";
        public const string EMPTY_STRING = "";
        public const string WHITESPACE_STRING = "   ";
        public static readonly string LONG_STRING = new('A', 1000);
    }

    #endregion

    #region ML Model Test Data

    /// <summary>
    ///     Creates test data for ML model predictions
    /// </summary>
    public static class MLTestData
    {
        public static byte[] CreateValidImageBytes()
        {
            // Create a simple valid image byte array for testing
            //return Encoding.UTF8.GetBytes("fake-image-data-for-testing");
            return Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");
            //return new byte[] {
            //    // PNG signature
            //    0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,

            //    // IHDR chunk (13 bytes data + 12 bytes header/crc)
            //    0x00, 0x00, 0x00, 0x0D, // Length: 13 bytes
            //    0x49, 0x48, 0x44, 0x52, // Type: IHDR
            //    0x00, 0x00, 0x00, 0x01, // Width: 1
            //    0x00, 0x00, 0x00, 0x01, // Height: 1
            //    0x08,                   // Bit depth: 8
            //    0x02,                   // Color type: 2 (RGB)
            //    0x00,                   // Compression: 0
            //    0x00,                   // Filter: 0
            //    0x00,                   // Interlace: 0
            //    0x90, 0x77, 0x53, 0xDE, // CRC

            //    // IDAT chunk (12 bytes data + 12 bytes header/crc)
            //    0x00, 0x00, 0x00, 0x0C, // Length: 12 bytes
            //    0x49, 0x44, 0x41, 0x54, // Type: IDAT
            //    0x78, 0x9C,             // Zlib header
            //    0x63, 0xF8, 0x0F, 0x00, // Compressed data (RGB: 255,255,255)
            //    0x00, 0x01, 0x00, 0x01,
            //    0x35, 0x5C, 0xC5, 0x9A, // CRC

            //    // IEND chunk
            //    0x00, 0x00, 0x00, 0x00, // Length: 0
            //    0x49, 0x45, 0x4E, 0x44, // Type: IEND
            //    0xAE, 0x42, 0x60, 0x82  // CRC
            //};
        }

        public static IFormFile CreateValidImageFile(string fileName = "test-pet.jpg")
        {
            // Generate image bytes (e.g., from a test image or pattern)
            var imageBytes = CreateValidImageBytes(); // or your actual image bytes

            // Buffer the bytes
            var buffer = imageBytes.ToArray();

            // Always return a new MemoryStream for each OpenReadStream call
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            fileMock.Setup(f => f.Length).Returns(buffer.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(buffer));
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns<Stream, CancellationToken>(async (s, ct) =>
                {
                    using var ms = new MemoryStream(buffer);
                    ms.Position = 0;
                    await ms.CopyToAsync(s, ct);
                });
            return fileMock.Object;
            //var content = CreateValidImageBytes();
            //using var stream = new MemoryStream(content);

            //var file = new FormFile(stream, 0, content.Length, "file", fileName)
            //{
            //    Headers = new HeaderDictionary(),
            //    ContentType = "image/jpeg"
            //};

            //return file;
        }

        public static string[] GetTestBreeds()
        {
            return new[]
            {
                "Golden Retriever", "German Shepherd", "Labrador", "Bulldog",
                "Poodle", "Beagle", "Rottweiler", "Siberian Husky"
            };
        }

        public static Dictionary<string, float> CreatePredictionResults()
        {
            return new Dictionary<string, float>
            {
                { "Golden Retriever", 0.95f },
                { "Labrador", 0.85f },
                { "German Shepherd", 0.75f },
                { "Beagle", 0.65f }
            };
        }
    }

    #endregion

    #region Authentication Test Data

    /// <summary>
    ///     Creates test data for authentication scenarios
    /// </summary>
    public static class AuthTestData
    {
        public static string CreateValidJwtToken()
        {
            return
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        }

        public static string CreateExpiredJwtToken()
        {
            return "expired.jwt.token";
        }

        public static Dictionary<string, object> CreateJwtClaims(Guid userId)
        {
            return new Dictionary<string, object>
            {
                { "sub", userId.ToString() },
                { "email", TestConstants.DEFAULT_EMAIL },
                { "name", TestConstants.DEFAULT_USERNAME },
                { "role", "User" },
                { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds() }
            };
        }
    }

    #endregion

    #region HTTP Test Data

    /// <summary>
    ///     Creates test data for HTTP requests and responses
    /// </summary>
    public static class HttpTestData
    {
        public static HttpRequestMessage CreateValidRequest(HttpMethod method, string uri)
        {
            return new HttpRequestMessage(method, uri);
        }

        public static HttpResponseMessage CreateSuccessResponse<T>(T data)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            // Add JSON content if needed
            return response;
        }

        public static HttpResponseMessage CreateErrorResponse(HttpStatusCode statusCode, string message)
        {
            var response = new HttpResponseMessage(statusCode);
            // Add error content if needed
            return response;
        }
    }

    #endregion

    #region Database Test Data

    /// <summary>
    ///     Creates test data for database scenarios
    /// </summary>
    public static class DatabaseTestData
    {
        public static string GetTestConnectionString()
        {
            return
                "Server=(localdb)\\mssqllocaldb;Database=FindPetTestDb;Trusted_Connection=true;MultipleActiveResultSets=true";
        }

        public static void SeedDatabase(DbContext context)
        {
            if (!context.Set<User>().Any())
            {
                var users = BuildUserList(5);
                context.Set<User>().AddRange(users);

                var pets = BuildPetList(10, users.First().Id);
                context.Set<Pet>().AddRange(pets);

                var ads = BuildAdList(15, users.First().Id);
                context.Set<Ad>().AddRange(ads);

                context.SaveChanges();
            }
        }
    }

    #endregion

    #region Performance Test Data

    /// <summary>
    ///     Creates test data for performance testing
    /// </summary>
    public static class PerformanceTestData
    {
        public static List<User> CreateLargeUserDataset(int count = 10000)
        {
            return BuildUserList(count);
        }

        public static List<Pet> CreateLargePetDataset(int count = 50000)
        {
            return BuildPetList(count);
        }

        public static List<Ad> CreateLargeAdDataset(int count = 100000)
        {
            return BuildAdList(count);
        }
    }

    #endregion

    #region Validation Test Data

    /// <summary>
    ///     Creates test data for validation scenarios
    /// </summary>
    public static class ValidationTestData
    {
        public static IEnumerable<object[]> GetInvalidEmailData()
        {
            yield return new object[] { "" };
            yield return new object[] { "   " };
            yield return new object[] { "invalid-email" };
            yield return new object[] { "@invalid.com" };
            yield return new object[] { "test@" };
            yield return new object[] { "test@.com" };
        }

        public static IEnumerable<object[]> GetInvalidPhoneData()
        {
            yield return new object[] { "" };
            yield return new object[] { "123" };
            yield return new object[] { "invalid-phone" };
            yield return new object[] { "+123" };
            yield return new object[] { "1234567890123456" };
        }

        public static IEnumerable<object[]> GetInvalidPasswordData()
        {
            yield return new object[] { "" };
            yield return new object[] { "123" };
            yield return new object[] { "password" };
            yield return new object[] { "12345678" };
            yield return new object[] { "PASSWORD123" };
        }
    }

    #endregion

    // ----------------------------------------------------

    #region Utility Methods

    public static class InvalidData
    {
        public static readonly Guid EmptyGuid = Guid.Empty;
        public static readonly string EmptyString = string.Empty;
        public static readonly string WhiteSpaceString = "   ";
        public static readonly string NullString = null;
        public static readonly DateTime FutureDate = DateTime.UtcNow.AddYears(10);
        public static readonly DateTime DistantPastDate = DateTime.UtcNow.AddYears(-100);
    }

    #endregion

    #region Generic Builders

    /// <summary>
    ///     Creates a list of entities with specified count
    /// </summary>
    public static List<T> CreateList<T>(int count, Func<int, T> factory)
    {
        return Enumerable.Range(1, count)
            .Select(factory)
            .ToList();
    }

    /// <summary>
    ///     Creates an entity with custom properties
    /// </summary>
    public static T With<T>(this T entity, Action<T> customize) where T : class
    {
        customize(entity);
        return entity;
    }

    #endregion

    #region User Entity

    // User building methods
    public static User BuildBasicUser(
        Guid? id = null,
        string name = "Test User",
        string email = "test@example.com",
        string phone = "+1234567890",
        string password = "Password123",
        string photo = null,
        DateTime? birthDate = null,
        DateTime? createDate = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Email = email,
            PhoneNumber = phone,
            Password = password,
            Photo = photo,
            BirthDate = birthDate,
            DateCreateUpdate = createDate ?? DateTime.UtcNow
        };
    }

    public static User BuildUserWithPhoto(string photoPath = "user_photo.jpg")
    {
        var user = BuildBasicUser();
        user.Photo = photoPath;
        return user;
    }

    public static User BuildUserWithId(Guid id)
    {
        var user = BuildBasicUser();
        user.Id = id;
        return user;
    }

    public static User BuildUserWithRole(string role, Guid id)
    {
        var user = BuildBasicUser();
        user.Id = id;

        return user;
    }

    public static User BuildInvalidUser(string invalidField = "email")
    {
        var user = BuildBasicUser();

        return invalidField.ToLower() switch
        {
            "id" => With(user, u => u.Id = Guid.Empty),
            "email" => With(user, u => u.Email = "invalid-email"),
            "name" => With(user, u => u.Name = null),
            "phone" => With(user, u => u.PhoneNumber = "12345"),
            _ => With(user, u => u.Email = "invalid-email")
        };
    }

    public static List<User> BuildUserList(int count = 3)
    {
        return CreateList(count, i =>
        {
            var user = BuildBasicUser();
            user.Name = $"Test User {i}";
            user.Email = $"user{i}@findpet.com";
            user.PhoneNumber = $"+123456789{i}";
            return user;
        });
    }

    #endregion

    #region User DTO Builders

    // User DTO building methods
    public static UserForCreateDto BuildUserForCreateDto(
        string name = "New Test User",
        string email = "newuser@example.com",
        string phone = "+0987654321",
        string password = "NewPass123",
        string photo = null,
        DateTime? birthDate = null)
    {
        return new UserForCreateDto
        {
            Name = name,
            Email = email,
            PhoneNumber = phone,
            Password = password,
            Photo = photo,
            BirthDate = birthDate
        };
    }

    public static UserForCreateDto BuildUserForCreateDtoWithPhoto()
    {
        var dto = BuildUserForCreateDto();
        dto.Photo = "new_user_photo.jpg";
        return dto;
    }

    public static UserForUpdateDto BuildUserForUpdateDto(
        string name = "Updated Test User",
        string email = "updated@example.com",
        string phone = "+1122334455",
        string password = "UpdatedPass123",
        string photo = null,
        DateTime? birthDate = null)
    {
        return new UserForUpdateDto
        {
            Name = name,
            Email = email,
            PhoneNumber = phone,
            Password = password,
            Photo = photo,
            BirthDate = birthDate
        };
    }

    public static UserForUpdateDto BuildUserForUpdateDtoWithPhoto()
    {
        var dto = BuildUserForUpdateDto();
        dto.Photo = "updated_photo.jpg";
        return dto;
    }

    public static LoginDto BuildUserForLoginDto(
        string email = "testuser@example.com",
        string password = "Password123!")
    {
        return new LoginDto
        {
            Email = email,
            Password = password
        };
    }

    public static UserForCreateDto BuildInvalidUserForCreateDto(string invalidField = "email")
    {
        var dto = BuildUserForCreateDto();

        return invalidField.ToLower() switch
        {
            "email" => With(dto, d => d.Email = "not-an-email"),
            "name" => With(dto, d => d.Name = null),
            "phone" => With(dto, d => d.PhoneNumber = "123"), // Invalid format
            _ => With(dto, d => d.Email = "not-an-email")
        };
    }

    public static UserForUpdateDto BuildInvalidUserForUpdateDto(string invalidField = "email")
    {
        var dto = BuildUserForUpdateDto();

        return invalidField.ToLower() switch
        {
            "email" => With(dto, d => d.Email = "not-an-email"),
            "name" => With(dto, d => d.Name = null),
            "phone" => With(dto, d => d.PhoneNumber = "123"), // Invalid format
            _ => With(dto, d => d.Email = "not-an-email")
        };
    }

    public static UserDto BuildBasicUserDto(
        Guid? id = null,
        string name = "Test User",
        string email = "test@example.com",
        string phone = "+1234567890",
        string password = "Password123",
        string photo = null,
        DateTime? birthDate = null,
        DateTime? createDate = null)
    {
        return new UserDto
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Email = email,
            PhoneNumber = phone,
            Password = password,
            Photo = photo,
            BirthDate = birthDate,
            DateCreateUpdate = createDate ?? DateTime.UtcNow
        };
    }

    public static List<UserDto> BuildUserDtoList(int count = 3)
    {
        return CreateList(count, i =>
        {
            var user = BuildBasicUserDto();
            user.Name = $"Test User {i}";
            user.Email = $"user{i}@findpet.com";
            user.PhoneNumber = $"+123456789{i}";
            return user;
        });
    }

    #endregion

    #region Pet Entity Builders

    // Pet building methods (example - extend as needed)
    public static Pet BuildBasicPet(
        Guid? id = null,
        string type = "Dog",
        string breed = "Labrador",
        string nickname = "Rex",
        string gender = "Male",
        string color = "Black",
        string size = "Medium",
        string marks = "White spot on chest",
        string photo = null,
        string description = "A friendly dog",
        Guid? userId = null,
        string status = "Missing")
    {
        return new Pet
        {
            Id = id ?? Guid.NewGuid(),
            Type = type,
            Breed = breed,
            Nickname = nickname,
            Gender = gender,
            Color = color,
            Size = size,
            SpecialMarks = marks,
            Photo = photo,
            Description = description,
            DateCreateUpdate = DateTime.UtcNow,
            LostDate = DateTime.UtcNow.AddDays(-5),
            LostLocation = "Main Street, 123",
            FoundDate = null,
            FoundLocation = null,
            Status = status,
            UserId = userId
        };
    }

    public static Pet BuildPetWithOwner(Guid ownerId)
    {
        var pet = BuildBasicPet();
        pet.UserId = ownerId;
        return pet;
    }

    public static Pet BuildPetWithPhoto(string photoPath = "pet_photo.jpg")
    {
        var pet = BuildBasicPet();
        pet.Photo = photoPath;
        return pet;
    }

    public static Pet BuildFoundPet(Guid? id = null, Guid? userId = null)
    {
        var pet = BuildBasicPet(id, userId: userId, status: "Found");
        pet.FoundDate = DateTime.UtcNow.AddDays(-1);
        pet.FoundLocation = "Park Avenue, 45";
        return pet;
    }

    public static List<Pet> BuildPetList(int count = 3, Guid? ownerId = null)
    {
        var pets = new List<Pet>();
        string[] types = { "Dog", "Cat", "Rabbit" };
        string[] breeds = { "Labrador", "Siamese", "Netherland Dwarf" };
        string[] colors = { "Black", "White", "Brown" };

        for (var i = 0; i < count; i++)
        {
            var typeIndex = i % types.Length;
            pets.Add(BuildBasicPet(
                type: types[typeIndex],
                breed: breeds[typeIndex],
                nickname: $"Pet {i}",
                color: colors[i % colors.Length],
                userId: ownerId
            ));
        }

        return pets;
    }

    public static Pet BuildInvalidPet(string invalidField = "type")
    {
        var pet = BuildBasicPet();
        return invalidField.ToLower() switch
        {
            "name" => With(pet, p => p.Nickname = null),
            "type" => With(pet, p => p.Type = null),
            "nickname" => With(pet, p => p.Nickname = null),
            "status" => With(pet, p => p.Status = "UnknownStatus"),
            _ => With(pet, p => p.Type = null)
        };
    }

    #endregion

    #region Pet DTO Builders

    public static PetForCreateDto BuildPetForCreateDto(
        string type = "Dog",
        string breed = "Labrador",
        string nickname = "Rex",
        string gender = "Male",
        string color = "Black",
        string size = "Medium",
        string marks = "White spot on chest",
        string photo = null,
        string description = "A friendly dog",
        string status = "Missing")
    {
        return new PetForCreateDto
        {
            Breed = breed,
            Nickname = nickname,
            Gender = gender,
            Color = color,
            Size = size,
            SpecialMarks = marks,
            Photo = photo,
            Description = description,
            LostDate = DateTime.UtcNow.AddDays(-5)
        };
    }

    public static PetForUpdateDto BuildPetForUpdateDto(
        string type = "Dog",
        string breed = "Updated Breed",
        string nickname = "Updated Name",
        string gender = "Male",
        string color = "Black",
        string size = "Medium",
        string marks = "Updated marks",
        string photo = null,
        string description = "Updated description",
        string status = "Missing")
    {
        return new PetForUpdateDto
        {
            Breed = breed,
            Nickname = nickname,
            Gender = gender,
            Color = color,
            Size = size,
            SpecialMarks = marks,
            Photo = photo,
            Description = description,
            LostDate = DateTime.UtcNow.AddDays(-5)
        };
    }

    public static PetForCreateDto BuildInvalidPetForCreateDto(string invalidField = "breed")
    {
        var dto = BuildPetForCreateDto();

        return invalidField.ToLower() switch
        {
            "breed" => With(dto, d => d.Breed = null),
            "nickname" => With(dto, d => d.Nickname = null),
            "gender" => With(dto, d => d.Gender = null),
            "color" => With(dto, d => d.Color = null),
            "size" => With(dto, d => d.Size = null),
            "specialmarks" => With(dto, d => d.SpecialMarks = null),
            "photo" => With(dto, d => d.Photo = null),
            "description" => With(dto, d => d.Description = null),
            _ => With(dto, d => d.Breed = null)
        };
    }

    #endregion


    #region Ad Entity Builders

    // Ad building methods (example - extend as needed)
    public static Ad BuildAd(
        Guid? id = null,
        string description = "A test advertisement",
        string location = "Test Location",
        string photo = null,
        Guid? petId = null,
        Guid? userId = null)
    {
        return new Ad
        {
            Id = id ?? Guid.NewGuid(),
            Description = description,
            Location = location,
            Photo = photo,
            DateCreateUpdate = DateTime.UtcNow,
            PetId = petId,
            UserId = userId
        };
    }

    public static List<Ad> BuildAdList(int count = 3, Guid? userId = null, Guid? petId = null)
    {
        var ads = new List<Ad>();
        for (var i = 0; i < count; i++)
            ads.Add(BuildAd(
                description: $"Advertisement {i}",
                location: $"Location {i}",
                userId: userId,
                petId: petId
            ));
        return ads;
    }

    public static Ad BuildInvalidAd(string invalidField = "description")
    {
        var ad = BuildAd();

        return invalidField.ToLower() switch
        {
            "description" => With(ad, a => a.Description = null),
            "location" => With(ad, a => a.Location = null),
            _ => With(ad, a => a.Description = null)
        };
    }

    #endregion

    #region Ad DTO Builders

    public static AdForCreateDto BuildAdForCreateDto(
        string description = "New advertisement",
        string location = "New location",
        string photo = null)
    {
        return new AdForCreateDto
        {
            Description = description,
            Location = location,
            Photo = photo
        };
    }

    public static AdForUpdateDto BuildAdForUpdateDto(
        string description = "Updated advertisement",
        string location = "Updated location",
        string photo = null)
    {
        return new AdForUpdateDto
        {
            Description = description,
            Location = location,
            Photo = photo
        };
    }

    public static AdForCreateDto BuildInvalidAdForCreateDto(string invalidField = "description")
    {
        var dto = BuildAdForCreateDto();

        return invalidField.ToLower() switch
        {
            "description" => With(dto, d => d.Description = null),
            "location" => With(dto, d => d.Location = null),
            _ => With(dto, d => d.Description = null)
        };
    }

    #endregion

    #region Specialized Builders for Different Testing Layers

    /// <summary>
    ///     Creates test data specifically for service layer testing
    /// </summary>
    public static class ServiceTestData
    {
        public static User CreateUserForServiceTest(bool withRelatedData = false)
        {
            var user = BuildBasicUser();

            if (withRelatedData)
            {
                user.Pets = BuildPetList(3, user.Id);
                user.Ads = BuildAdList(5, user.Id);
            }

            return user;
        }

        public static Pet CreatePetForServiceTest(bool withOwner = false, bool withAds = false)
        {
            var pet = BuildBasicPet();

            if (withOwner) pet.User = BuildBasicUser(pet.UserId);

            if (withAds) pet.Ads = BuildAdList(2, petId: pet.Id);

            return pet;
        }
    }

    /// <summary>
    ///     Creates test data specifically for controller layer testing
    /// </summary>
    public static class ControllerTestData
    {
        public static User CreateUserForControllerTest()
        {
            return BuildBasicUser();
        }

        public static UserForCreateDto CreateUserForCreateControllerTest()
        {
            return BuildUserForCreateDto();
        }

        public static Dictionary<string, string> CreateControllerHeaders()
        {
            return new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {AuthTestData.CreateValidJwtToken()}" },
                { "Content-Type", "application/json" },
                { "Accept", "application/json" }
            };
        }
    }

    /// <summary>
    ///     Creates test data specifically for repository layer testing
    /// </summary>
    public static class RepositoryTestData
    {
        public static User CreateUserForRepositoryTest()
        {
            return BuildBasicUser();
        }

        //public static Expression<Func<User, bool>> CreateUserFilterExpression()
        //{
        //    return u => u. && u.EmailConfirmed;
        //}

        //public static Expression<Func<Pet, bool>> CreatePetFilterExpression()
        //{
        //    return p => p.IsActive && p.PetType == PetType.Dog;
        //}
    }

    #endregion

    #region Entity Relationships

    public static User BuildUserWithPets(int petCount = 2)
    {
        var user = BuildBasicUser();
        user.Pets = BuildPetList(petCount, user.Id);
        return user;
    }

    public static User BuildUserWithAds(int adCount = 2)
    {
        var user = BuildBasicUser();
        user.Ads = BuildAdList(adCount, user.Id);
        return user;
    }

    public static Pet BuildPetWithAds(int adCount = 2)
    {
        var pet = BuildBasicPet();
        pet.Ads = BuildAdList(adCount, petId: pet.Id);
        return pet;
    }

    public static User BuildCompleteUser(int petCount = 2, int adCount = 2)
    {
        var user = BuildBasicUser();
        user.Pets = BuildPetList(petCount, user.Id);
        user.Ads = BuildAdList(adCount, user.Id);
        return user;
    }

    #endregion
}