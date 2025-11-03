using FindPet.BusinessLogicLayer.Interfaces.ICacheService;
using FindPet.Domain.Interfaces.ILoggerService;
using StackExchange.Redis;
using System.Text.Json;

namespace FindPet.BusinessLogicLayer.Services.CacheService;

public class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILoggerManager _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(IConnectionMultiplexer redis, ILoggerManager logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }
    private IDatabase GetDatabase()
    {
        try
        {
            return _redis.GetDatabase();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to get Redis database: {ex.Message}");
            throw new InvalidOperationException("Redis connection unavailable", ex);
        }
    }
    //IServer? GetServer() => redis.GetServers().LastOrDefault();
    public async Task SetValueAsync<T>(string key, T value, TimeSpan expiration)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

        if (value == null)
        {
            _logger.LogWarn($"Attempted to cache null value for key: {key}");
            return;
        }

        try
        {
            var db = GetDatabase();
            var json = JsonSerializer.Serialize(value, _jsonOptions);

            var success = await db.StringSetAsync(key, json, expiration);

            if (success)
            {
                _logger.LogDebug($"Successfully cached data for key: {key} with expiration: {expiration.TotalMinutes} minutes");
            }
            else
            {
                _logger.LogWarn($"Failed to cache data for key: {key}");
            }
        }
        catch (RedisException ex)
        {
            _logger.LogError($"Redis error while setting value for key '{key}': {ex.Message}");
        }
        catch (JsonException ex)
        {
            _logger.LogError($"JSON serialization error for key '{key}': {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error while caching key '{key}': {ex.Message}");
        }
    }

    public async Task<T> GetValueAsync<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

        try
        {
            var db = GetDatabase();
            var data = await db.StringGetAsync(key);

            if (!data.HasValue || data.IsNullOrEmpty)
            {
                _logger.LogDebug($"Cache miss for key: {key}");
                return default;
            }

            var result = JsonSerializer.Deserialize<T>(data!, _jsonOptions);
            _logger.LogDebug($"Cache hit for key: {key}");

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError($"JSON deserialization error for key '{key}': {ex.Message}");
            await RemoveAsync(key);
            return default;
        }
        catch (RedisException ex)
        {
            _logger.LogError($"Redis error while getting cache key '{key}': {ex.Message}");
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error while getting cache key '{key}': {ex.Message}");
            return default;
        }
    }

    public async Task<T> GetValueOrInitializeAsync<T>(string key, Func<Task<T>> functionToObtain, TimeSpan duration)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

        if (functionToObtain == null)
            throw new ArgumentNullException(nameof(functionToObtain));

        try
        {
            var cachedValue = await GetValueAsync<T>(key);

            if (cachedValue != null && !EqualityComparer<T>.Default.Equals(cachedValue, default))
            {
                return cachedValue;
            }

            // Cache miss - execute function
            _logger.LogDebug($"Cache miss for key: {key}, fetching from source");
            var value = await functionToObtain.Invoke();

            // Cache the result if not null
            if (value != null && !EqualityComparer<T>.Default.Equals(value, default))
            {
                await SetValueAsync(key, value, duration);
            }

            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in GetValueOrInitializeAsync for key '{key}': {ex.Message}");
            // If caching fails, still return the fresh value
            return await functionToObtain.Invoke();
        }
    }

    public async Task RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

        try
        {
            var db = GetDatabase();
            var removed = await db.KeyDeleteAsync(key);

            if (removed)
            {
                _logger.LogDebug($"Successfully removed cache key: {key}");
            }
            else
            {
                _logger.LogDebug($"Cache key not found for removal: {key}");
            }
        }
        catch (RedisException ex)
        {
            _logger.LogError($"Redis error while removing cache key '{key}': {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error while removing cache key '{key}': {ex.Message}");
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

        try
        {
            var db = GetDatabase();
            return await db.KeyExistsAsync(key);
        }
        catch (RedisException ex)
        {
            _logger.LogError($"Redis error while checking existence of key '{key}': {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error while checking key '{key}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets Redis connection health status
    /// </summary>
    public bool IsConnected()
    {
        try
        {
            return _redis.IsConnected;
        }
        catch
        {
            return false;
        }
    }
}