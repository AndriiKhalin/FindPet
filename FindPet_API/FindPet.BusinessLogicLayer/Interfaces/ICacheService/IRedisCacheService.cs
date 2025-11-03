namespace FindPet.BusinessLogicLayer.Interfaces.ICacheService;

public interface IRedisCacheService
{
    Task SetValueAsync<T>(string key, T value, TimeSpan expiration);
    Task<T> GetValueAsync<T>(string key);

    Task<T> GetValueOrInitializeAsync<T>(string key, Func<Task<T>> functionToObtain, TimeSpan duration);
    Task RemoveAsync(string key);

    Task<bool> ExistsAsync(string key);
}