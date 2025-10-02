using FindPet.Domain.DTOs.EntitiesDTOs.UserDTO;
using FindPet.Domain.Entities;

namespace FindPet.Tests.TestHelpers;

public static class TestDataBuilder
{
    // User building methods
    public static User BuildBasicUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            DateCreateUpdate = DateTime.UtcNow,
            PhoneNumber = "+1234567890"
        };
    }

    public static User BuildUserWithPhoto()
    {
        var user = BuildBasicUser();
        user.Photo = "user_photo.jpg";
        return user;
    }

    public static User BuildUserWithId(Guid id)
    {
        var user = BuildBasicUser();
        user.Id = id;
        return user;
    }

    public static List<User> BuildUserList(int count = 3)
    {
        var users = new List<User>();
        for (int i = 0; i < count; i++)
        {
            users.Add(new User
            {
                Id = Guid.NewGuid(),
                Name = $"Test User {i}",
                Email = $"user{i}@example.com",
                DateCreateUpdate = DateTime.UtcNow,
                PhoneNumber = $"+123456789{i}"
            });
        }
        return users;
    }

    // User DTO building methods
    public static UserForCreateDto BuildUserForCreateDto()
    {
        return new UserForCreateDto
        {
            Name = "New Test User",
            Email = "newuser@example.com",
            PhoneNumber = "+0987654321"
        };
    }

    public static UserForCreateDto BuildUserForCreateDtoWithPhoto()
    {
        var dto = BuildUserForCreateDto();
        dto.Photo = "new_user_photo.jpg";
        return dto;
    }

    public static UserForUpdateDto BuildUserForUpdateDto()
    {
        return new UserForUpdateDto
        {
            Name = "Updated Test User",
            Email = "updated@example.com",
            PhoneNumber = "+1122334455"
        };
    }

    public static UserForUpdateDto BuildUserForUpdateDtoWithPhoto()
    {
        var dto = BuildUserForUpdateDto();
        dto.Photo = "updated_photo.jpg";
        return dto;
    }

    // Pet building methods (example - extend as needed)
    public static Pet BuildBasicPet()
    {
        return new Pet
        {
            Id = Guid.NewGuid(),
            Nickname = "Test Pet",
            Description = "A test pet",
            DateCreateUpdate = DateTime.UtcNow
        };
    }

    public static Pet BuildPetWithOwner(Guid ownerId)
    {
        var pet = BuildBasicPet();
        pet.UserId = ownerId;
        return pet;
    }

    // Ad building methods (example - extend as needed)
    public static Ad BuildBasicAd()
    {
        return new Ad
        {
            Id = Guid.NewGuid(),
            Description = "A test advertisement",
            DateCreateUpdate = DateTime.UtcNow
        };
    }

    // Custom builder methods for specific test scenarios
    public static User BuildInvalidUser()
    {
        return new User
        {
            Id = Guid.Empty,
            Name = null,
            Email = "invalid-email"
        };
    }

    public static UserForCreateDto BuildInvalidUserForCreateDto()
    {
        return new UserForCreateDto
        {
            Name = null,
            Email = "not-an-email"
        };
    }

    // ----------------------------------------------------

    public static User CreateValidUser(Guid? userId = null)
    {
        return new User
        {
            Id = userId ?? Guid.NewGuid(),
            Name = "John",
            Email = "john.doe@example.com",
            PhoneNumber = "+1234567890",
            Photo = "path/to/photo.jpg",
            DateCreateUpdate = DateTime.UtcNow,
        };
    }

    public static UserForCreateDto CreateValidUserForCreateDto()
    {
        return new UserForCreateDto
        {
            Name = "Jane",
            Email = "jane.smith@example.com",
            PhoneNumber = "+1987654321",
            Photo = "path/to/new-photo.jpg",
        };
    }

    public static UserForUpdateDto CreateValidUserForUpdateDto()
    {
        return new UserForUpdateDto
        {
            Name = "Updated",
            Email = "updated@example.com",
            PhoneNumber = "+1111111111",
            Photo = "path/to/updated-photo.jpg"
        };
    }

    public static IEnumerable<User> CreateUserList(int count = 3)
    {
        var users = new List<User>();
        for (int i = 0; i < count; i++)
        {
            users.Add(new User
            {
                Id = Guid.NewGuid(),
                Name = $"User{i}",
                Email = $"user{i}@test.com",
                PhoneNumber = $"+123456789{i}",
                DateCreateUpdate = DateTime.UtcNow.AddDays(-i)
            });
        }
        return users;
    }

    public static class InvalidData
    {
        public static readonly Guid EmptyGuid = Guid.Empty;
        public static readonly string EmptyString = string.Empty;
        public static readonly string WhiteSpaceString = "   ";
        public static readonly string NullString = null;
    }
}