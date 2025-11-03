namespace FindPet.Domain.Constants;

public static class CacheKeys
{
    // Cache durations
    public static class Duration
    {
        public static readonly TimeSpan Short = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan Medium = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan Long = TimeSpan.FromHours(1);
        public static readonly TimeSpan VeryLong = TimeSpan.FromHours(24);
    }

    // Pet-related keys
    public const string AllPets = "pets:all";
    public const string PetById = "pet:{0}";
    public const string PetsByUser = "pets:user:{0}";
    public const string RecentPets = "pets:recent";
    public const string PopularPets = "pets:popular";

    // Ad-related keys
    public const string AllAds = "ads:all";
    public const string AdById = "ad:{0}";
    public const string AdsByUser = "ads:user:{0}";
    public const string RecentAds = "ads:recent";

    // User-related keys (cache sparingly - contains sensitive data)
    public const string UserById = "user:{0}";

    // Auth-related keys
    public const string AllRoles = "roles:all";

    // Helper methods
    public static string GetPetByIdKey(Guid petId) => string.Format(PetById, petId);
    public static string GetPetsByUserKey(Guid userId) => string.Format(PetsByUser, userId);
    public static string GetAdByIdKey(Guid adId) => string.Format(AdById, adId);
    public static string GetAdsByUserKey(Guid userId) => string.Format(AdsByUser, userId);
    public static string GetUserByIdKey(Guid userId) => string.Format(UserById, userId);
}