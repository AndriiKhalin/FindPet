using System.Text.Json;
using FindPet.BusinessLogicLayer.Interfaces.ICacheService;
using StackExchange.Redis;

namespace FindPet.BusinessLogicLayer.Services.CacheService;

public class RedisCacheService(IConnectionMultiplexer redis) : IRedisCacheService
{
    IDatabase GetDatabase() => redis.GetDatabase();
    //IServer? GetServer() => redis.GetServers().LastOrDefault();
    public async Task SetValueAsync<T>(string key, T value, TimeSpan expiration)
    {
        var db = GetDatabase();
        var json = JsonSerializer.Serialize(value);
        await db.StringSetAsync(key, json, expiration);
    }

    public async Task<T> GetValueAsync<T>(string key)
    {
        var db = GetDatabase();
        if (db == null)
        {
            return default;
        }
        var data = await db.StringGetAsync(key);
        return data.HasValue && !data.IsNullOrEmpty ? JsonSerializer.Deserialize<T>(data) : default;
    }

    public async Task<T> GetValueOrInitializeAsync<T>(string key, Func<Task<T>> functionToObtain, TimeSpan duration)
    {
        var value = await GetValueAsync<T>(key);

        if (value == null)
        {
            value = await functionToObtain.Invoke();
            if (value != null)
                await SetValueAsync(key, value, duration);
        }

        return value;
    }

    public async Task RemoveAsync(string key)
    {
        await GetDatabase().KeyDeleteAsync(key);
    }

    public Task<bool> ExistsAsync(string key) => GetDatabase().KeyExistsAsync(new RedisKey(key)) ?? Task.FromResult(false);
}