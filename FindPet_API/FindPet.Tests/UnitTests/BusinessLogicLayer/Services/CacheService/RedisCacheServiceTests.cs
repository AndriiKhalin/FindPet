using System.Text.Json;
using FindPet.BusinessLogicLayer.Services.CacheService;
using FindPet.Domain.Entities;
using FindPet.Domain.Interfaces.ILoggerService;
using FindPet.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace FindPet.Tests.UnitTests.BusinessLogicLayer.Services.CacheService;

public class RedisCacheServiceTests
{
    private readonly RedisCacheService _cacheService;
    private readonly Mock<IDatabase> _mockDatabase;
    private readonly Mock<ILoggerManager> _mockLogger;
    private readonly Mock<IConnectionMultiplexer> _mockRedis;

    public RedisCacheServiceTests()
    {
        _mockRedis = new Mock<IConnectionMultiplexer>();
        _mockDatabase = new Mock<IDatabase>();
        _mockLogger = MockSetupExtensions.SetupLoggerMock();

        _mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_mockDatabase.Object);
        _mockRedis.Setup(x => x.IsConnected).Returns(true);

        _cacheService = new RedisCacheService(_mockRedis.Object, _mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullRedis_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new RedisCacheService(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("redis");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new RedisCacheService(_mockRedis.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var service = new RedisCacheService(_mockRedis.Object, _mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region SetValueAsync Tests

    [Fact]
    public async Task SetValueAsync_WithValidData_ShouldSerializeAndCacheValue()
    {
        // Arrange
        var key = "test-user-key";
        var user = TestDataBuilder.BuildBasicUser();
        var expiration = TimeSpan.FromMinutes(5);

        _mockDatabase.Setup(x => x.StringSetAsync(
                key,
                It.IsAny<RedisValue>(),
                expiration,
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _cacheService.SetValueAsync(key, user, expiration);

        // Assert
        _mockDatabase.Verify(x => x.StringSetAsync(
            key,
            It.IsAny<RedisValue>(),
            expiration,
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), Times.Once);

        _mockLogger.Verify(x => x.LogDebug(
            It.Is<string>(s => s.Contains($"Successfully cached data for key: {key}")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SetValueAsync_WithInvalidKey_ShouldThrowArgumentException(string invalidKey)
    {
        // Arrange
        var user = TestDataBuilder.BuildBasicUser();
        var expiration = TimeSpan.FromMinutes(5);

        // Act
        var act = async () => await _cacheService.SetValueAsync(invalidKey, user, expiration);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Cache key cannot be null or empty*")
            .WithParameterName("key");
    }

    [Fact]
    public async Task SetValueAsync_WithNullValue_ShouldLogWarningAndReturn()
    {
        // Arrange
        var key = "test-key";
        User nullUser = null;
        var expiration = TimeSpan.FromMinutes(5);

        // Act
        await _cacheService.SetValueAsync(key, nullUser, expiration);

        // Assert
        _mockLogger.Verify(x => x.LogWarn(
            It.Is<string>(s => s.Contains($"Attempted to cache null value for key: {key}")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);

        _mockDatabase.Verify(x => x.StringSetAsync(
            It.Is<RedisKey>(k => k == key),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), Times.Never);
    }

    [Fact]
    public async Task SetValueAsync_WhenRedisNotConnected_ShouldLogDebugAndReturn()
    {
        // Arrange
        _mockRedis.Setup(x => x.IsConnected).Returns(false);
        var key = "test-key";
        var user = TestDataBuilder.BuildBasicUser();
        var expiration = TimeSpan.FromMinutes(5);

        // Act
        await _cacheService.SetValueAsync(key, user, expiration);

        // Assert
        _mockLogger.Verify(x => x.LogDebug(
            It.Is<string>(s => s.Contains("Cache set skipped for key") && s.Contains("Redis not available")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);

        _mockDatabase.Verify(x => x.StringSetAsync(
            It.Is<RedisKey>(k => k == key),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), Times.Never);
    }

    [Fact]
    public async Task SetValueAsync_WhenSetFails_ShouldLogWarning()
    {
        // Arrange
        var key = "test-key";
        var user = TestDataBuilder.BuildBasicUser();
        var expiration = TimeSpan.FromMinutes(5);

        _mockDatabase.Setup(x => x.StringSetAsync(
                key,
                It.IsAny<RedisValue>(),
                expiration,
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        // Act
        await _cacheService.SetValueAsync(key, user, expiration);

        // Assert
        _mockLogger.Verify(x => x.LogWarn(
            It.Is<string>(s => s.Contains($"Failed to cache data for key: {key}")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task SetValueAsync_WhenRedisThrowsException_ShouldLogError()
    {
        // Arrange
        var key = "test-key";
        var user = TestDataBuilder.BuildBasicUser();
        var expiration = TimeSpan.FromMinutes(5);

        _mockDatabase.Setup(x => x.StringSetAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisException("Connection timeout"));

        // Act
        await _cacheService.SetValueAsync(key, user, expiration);

        // Assert
        _mockLogger.Verify(x => x.LogError(
            It.Is<string>(s => s.Contains("Redis error while setting value") && s.Contains(key)),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task SetValueAsync_WithComplexObject_ShouldSerializeCorrectly()
    {
        // Arrange
        var key = "test-pet-list";
        var pets = TestDataBuilder.BuildPetList(5);
        var expiration = TimeSpan.FromMinutes(10);
        RedisValue capturedValue = default;

        _mockDatabase.Setup(x => x.StringSetAsync(
                key,
                It.IsAny<RedisValue>(),
                expiration,
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .Callback<RedisKey, RedisValue, TimeSpan?, bool, When, CommandFlags>((k, v, exp, keepTtl, when, flags) =>
                capturedValue = v)
            .ReturnsAsync(true);

        // Act
        await _cacheService.SetValueAsync(key, pets, expiration);

        // Assert
        capturedValue.HasValue.Should().BeTrue();
        var deserializedPets = JsonSerializer.Deserialize<List<Pet>>(capturedValue!);
        deserializedPets.Should().NotBeNull();
        deserializedPets.Should().HaveCount(5);
    }

    #endregion

    #region GetValueAsync Tests

    [Fact]
    public async Task GetValueAsync_WithExistingKey_ShouldReturnDeserializedValue()
    {
        // Arrange
        var key = "test-user-key";
        var expectedUser = TestDataBuilder.BuildBasicUser();
        var serializedUser = JsonSerializer.Serialize(expectedUser);
        var slidingExpiration = TimeSpan.FromMinutes(5);

        _mockDatabase.Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue(serializedUser));

        _mockDatabase.Setup(x => x.KeyExistsAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockDatabase.Setup(x =>
                x.KeyExpireAsync(key, slidingExpiration, It.IsAny<ExpireWhen>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await _cacheService.GetValueAsync<User>(key, slidingExpiration);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedUser.Id);
        result.Email.Should().Be(expectedUser.Email);
        result.Name.Should().Be(expectedUser.Name);

        _mockLogger.Verify(x => x.LogDebug(
            It.Is<string>(s => s.Contains($"Cache hit for key: {key}")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task GetValueAsync_WithNonExistingKey_ShouldReturnDefault()
    {
        // Arrange
        var key = "non-existing-key";
        var slidingExpiration = TimeSpan.FromMinutes(5);

        _mockDatabase.Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await _cacheService.GetValueAsync<User>(key, slidingExpiration);

        // Assert
        result.Should().BeNull();

        _mockLogger.Verify(x => x.LogDebug(
            It.Is<string>(s => s.Contains($"Cache miss for key: {key}")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetValueAsync_WithInvalidKey_ShouldThrowArgumentException(string invalidKey)
    {
        // Arrange
        var slidingExpiration = TimeSpan.FromMinutes(5);

        // Act
        Func<Task> act = async () => await _cacheService.GetValueAsync<User>(invalidKey, slidingExpiration);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Cache key cannot be null or empty*")
            .WithParameterName("key");
    }

    [Fact]
    public async Task GetValueAsync_WhenRedisNotConnected_ShouldReturnDefault()
    {
        // Arrange
        _mockRedis.Setup(x => x.IsConnected).Returns(false);
        var key = "test-key";
        var slidingExpiration = TimeSpan.FromMinutes(5);

        // Act
        var result = await _cacheService.GetValueAsync<User>(key, slidingExpiration);

        // Assert
        result.Should().BeNull();

        _mockLogger.Verify(x => x.LogDebug(
            It.Is<string>(s => s.Contains("Cache get skipped for key") && s.Contains("Redis not available")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task GetValueAsync_WithInvalidJson_ShouldLogErrorAndRemoveKey()
    {
        // Arrange
        var key = "test-key";
        var invalidJson = "{ invalid json }";
        var slidingExpiration = TimeSpan.FromMinutes(5);

        _mockDatabase.Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue(invalidJson));

        _mockDatabase.Setup(x => x.KeyDeleteAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await _cacheService.GetValueAsync<User>(key, slidingExpiration);

        // Assert
        result.Should().BeNull();

        _mockLogger.Verify(x => x.LogError(
            It.Is<string>(s => s.Contains("JSON deserialization error") && s.Contains(key)),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);

        _mockDatabase.Verify(x => x.KeyDeleteAsync(key, It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task GetValueAsync_ShouldRefreshSlidingExpiration()
    {
        // Arrange
        var key = "test-key";
        var user = TestDataBuilder.BuildBasicUser();
        var serializedUser = JsonSerializer.Serialize(user);
        var slidingExpiration = TimeSpan.FromMinutes(10);

        _mockDatabase.Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue(serializedUser));

        _mockDatabase.Setup(x => x.KeyExistsAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockDatabase.Setup(x =>
                x.KeyExpireAsync(key, slidingExpiration, It.IsAny<ExpireWhen>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await _cacheService.GetValueAsync<User>(key, slidingExpiration);

        // Assert
        result.Should().NotBeNull();

        _mockDatabase.Verify(x => x.KeyExpireAsync(
            key,
            slidingExpiration,
            It.IsAny<ExpireWhen>(),
            It.IsAny<CommandFlags>()), Times.Once);

        _mockLogger.Verify(x => x.LogDebug(
            It.Is<string>(s => s.Contains($"Successfully refreshed expiration for key: {key}")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task GetValueAsync_WhenRedisThrowsException_ShouldLogErrorAndReturnDefault()
    {
        // Arrange
        var key = "test-key";
        var slidingExpiration = TimeSpan.FromMinutes(5);

        _mockDatabase.Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisException("Connection timeout"));

        // Act
        var result = await _cacheService.GetValueAsync<User>(key, slidingExpiration);

        // Assert
        result.Should().BeNull();

        _mockLogger.Verify(x => x.LogError(
            It.Is<string>(s => s.Contains("Redis error while getting cache key") && s.Contains(key)),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);
    }

    #endregion

    #region GetValueOrInitializeAsync Tests

    [Fact]
    public async Task GetValueOrInitializeAsync_WithCacheHit_ShouldReturnCachedValue()
    {
        // Arrange
        var key = "test-key";
        var cachedUser = TestDataBuilder.BuildBasicUser();
        var serializedUser = JsonSerializer.Serialize(cachedUser);
        var duration = TimeSpan.FromMinutes(5);
        var factoryCallCount = 0;

        _mockDatabase.Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue(serializedUser));

        _mockDatabase.Setup(x => x.KeyExistsAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockDatabase.Setup(x => x.KeyExpireAsync(key, duration, It.IsAny<ExpireWhen>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        Func<Task<User>> factory = async () =>
        {
            factoryCallCount++;
            await Task.Delay(10);
            return TestDataBuilder.BuildBasicUser();
        };

        // Act
        var result = await _cacheService.GetValueOrInitializeAsync(key, factory, duration);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(cachedUser.Id);
        factoryCallCount.Should().Be(0, "Factory should not be called on cache hit");

        _mockLogger.Verify(x => x.LogDebug(
            It.Is<string>(s => s.Contains($"Cache hit for key: {key}")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task GetValueOrInitializeAsync_WithCacheMiss_ShouldCallFactoryAndCacheResult()
    {
        // Arrange
        var key = "test-key";
        var newUser = TestDataBuilder.BuildBasicUser();
        var duration = TimeSpan.FromMinutes(5);
        var factoryCallCount = 0;

        _mockDatabase.Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        _mockDatabase.Setup(x => x.StringSetAsync(
                key,
                It.IsAny<RedisValue>(),
                duration,
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        Func<Task<User>> factory = async () =>
        {
            factoryCallCount++;
            await Task.Delay(10);
            return newUser;
        };

        // Act
        var result = await _cacheService.GetValueOrInitializeAsync(key, factory, duration);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(newUser.Id);
        factoryCallCount.Should().Be(1, "Factory should be called exactly once on cache miss");

        _mockLogger.Verify(x => x.LogDebug(
            It.Is<string>(s => s.Contains($"Cache miss for key: {key}, fetching from source")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);

        _mockDatabase.Verify(x => x.StringSetAsync(
            key,
            It.IsAny<RedisValue>(),
            duration,
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetValueOrInitializeAsync_WithInvalidKey_ShouldThrowArgumentException(string invalidKey)
    {
        // Arrange
        var duration = TimeSpan.FromMinutes(5);
        var factory = () => Task.FromResult(TestDataBuilder.BuildBasicUser());

        // Act
        Func<Task> act = async () => await _cacheService.GetValueOrInitializeAsync(invalidKey, factory, duration);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Cache key cannot be null or empty*")
            .WithParameterName("key");
    }

    [Fact]
    public async Task GetValueOrInitializeAsync_WithNullFactory_ShouldThrowArgumentNullException()
    {
        // Arrange
        var key = "test-key";
        var duration = TimeSpan.FromMinutes(5);
        Func<Task<User>> nullFactory = null;

        // Act
        Func<Task> act = async () => await _cacheService.GetValueOrInitializeAsync(key, nullFactory, duration);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("functionToObtain");
    }

    [Fact]
    public async Task GetValueOrInitializeAsync_WhenRedisNotConnected_ShouldCallFactoryDirectly()
    {
        // Arrange
        _mockRedis.Setup(x => x.IsConnected).Returns(false);
        var key = "test-key";
        var user = TestDataBuilder.BuildBasicUser();
        var duration = TimeSpan.FromMinutes(5);
        var factoryCallCount = 0;

        Func<Task<User>> factory = async () =>
        {
            factoryCallCount++;
            await Task.Delay(10);
            return user;
        };

        // Act
        var result = await _cacheService.GetValueOrInitializeAsync(key, factory, duration);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        factoryCallCount.Should().Be(1);

        _mockLogger.Verify(x => x.LogDebug(
            It.Is<string>(s => s.Contains("Cache get and set skipped for key")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task GetValueOrInitializeAsync_WhenFactoryReturnsNull_ShouldNotCache()
    {
        // Arrange
        var key = "test-key";
        var duration = TimeSpan.FromMinutes(5);

        _mockDatabase.Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var factory = () => Task.FromResult<User>(null);

        // Act
        var result = await _cacheService.GetValueOrInitializeAsync(key, factory, duration);

        // Assert
        result.Should().BeNull();

        _mockDatabase.Verify(x => x.StringSetAsync(
            It.Is<RedisKey>(k => k == key),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), Times.Never);
    }

    [Fact]
    public async Task GetValueOrInitializeAsync_WhenCachingFails_ShouldStillReturnValue()
    {
        // Arrange
        var key = "test-key";
        var user = TestDataBuilder.BuildBasicUser();
        var duration = TimeSpan.FromMinutes(5);

        _mockDatabase.Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        _mockDatabase.Setup(x => x.StringSetAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisException("Cache write failed"));

        var factory = () => Task.FromResult(user);

        // Act
        var result = await _cacheService.GetValueOrInitializeAsync(key, factory, duration);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);

        // The error is logged from SetValueAsync, not GetValueAsync
        _mockLogger.Verify(x => x.LogError(
            It.Is<string>(s => s.Contains("Redis error while setting value") && s.Contains(key)),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);

        // Verify that the cache miss and factory call were logged
        _mockLogger.Verify(x => x.LogDebug(
            It.Is<string>(s => s.Contains("Cache miss for key") && s.Contains(key)),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetValueOrInitializeAsync_WithComplexCollection_ShouldWorkCorrectly()
    {
        // Arrange
        var key = "test-ads-list";
        var ads = TestDataBuilder.BuildAdList(10);
        var serializedAds = JsonSerializer.Serialize(ads);
        var duration = TimeSpan.FromMinutes(15);

        _mockDatabase.Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue(serializedAds));

        _mockDatabase.Setup(x => x.KeyExistsAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockDatabase.Setup(x => x.KeyExpireAsync(key, duration, It.IsAny<ExpireWhen>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var factory = () => Task.FromResult(TestDataBuilder.BuildAdList(10));

        // Act
        var result = await _cacheService.GetValueOrInitializeAsync(key, factory, duration);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(10);
        result.Should().BeEquivalentTo(ads);
    }

    #endregion

    #region RemoveAsync Tests

    [Fact]
    public async Task RemoveAsync_WithExistingKey_ShouldDeleteKey()
    {
        // Arrange
        var key = "test-key";

        _mockDatabase.Setup(x => x.KeyDeleteAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        _mockDatabase.Verify(x => x.KeyDeleteAsync(key, It.IsAny<CommandFlags>()), Times.Once);

        _mockLogger.Verify(x => x.LogDebug(
            It.Is<string>(s => s.Contains($"Successfully removed cache key: {key}")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_WithNonExistingKey_ShouldLogDebug()
    {
        // Arrange
        var key = "non-existing-key";

        _mockDatabase.Setup(x => x.KeyDeleteAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        _mockDatabase.Verify(x => x.KeyDeleteAsync(key, It.IsAny<CommandFlags>()), Times.Once);

        _mockLogger.Verify(x => x.LogDebug(
            It.Is<string>(s => s.Contains($"Cache key not found for removal: {key}")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RemoveAsync_WithInvalidKey_ShouldThrowArgumentException(string invalidKey)
    {
        // Act
        var act = async () => await _cacheService.RemoveAsync(invalidKey);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Cache key cannot be null or empty*")
            .WithParameterName("key");
    }

    [Fact]
    public async Task RemoveAsync_WhenRedisNotConnected_ShouldLogDebugAndReturn()
    {
        // Arrange
        _mockRedis.Setup(x => x.IsConnected).Returns(false);
        var key = "test-key";

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        _mockLogger.Verify(x => x.LogDebug(
            It.Is<string>(s => s.Contains("Cache removal skipped for key") && s.Contains("Redis not available")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);

        _mockDatabase.Verify(x => x.KeyDeleteAsync(It.Is<RedisKey>(k => k == key), It.IsAny<CommandFlags>()),
            Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_WhenRedisThrowsException_ShouldLogError()
    {
        // Arrange
        var key = "test-key";

        _mockDatabase.Setup(x => x.KeyDeleteAsync(key, It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisException("Connection timeout"));

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        _mockLogger.Verify(x => x.LogError(
            It.Is<string>(s => s.Contains("Redis error while removing cache key") && s.Contains(key)),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WithExistingKey_ShouldReturnTrue()
    {
        // Arrange
        var key = "test-key";

        _mockDatabase.Setup(x => x.KeyExistsAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeTrue();
        _mockDatabase.Verify(x => x.KeyExistsAsync(key, It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingKey_ShouldReturnFalse()
    {
        // Arrange
        var key = "non-existing-key";

        _mockDatabase.Setup(x => x.KeyExistsAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeFalse();
        _mockDatabase.Verify(x => x.KeyExistsAsync(key, It.IsAny<CommandFlags>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExistsAsync_WithInvalidKey_ShouldThrowArgumentException(string invalidKey)
    {
        // Act
        Func<Task> act = async () => await _cacheService.ExistsAsync(invalidKey);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Cache key cannot be null or empty*")
            .WithParameterName("key");
    }

    [Fact]
    public async Task ExistsAsync_WhenRedisNotConnected_ShouldReturnFalse()
    {
        // Arrange
        _mockRedis.Setup(x => x.IsConnected).Returns(false);
        var key = "test-key";

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeFalse();

        _mockLogger.Verify(x => x.LogDebug(
            It.Is<string>(s =>
                s.Contains("Cache existence check skipped for key") && s.Contains("Redis not available")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);

        _mockDatabase.Verify(x => x.KeyExistsAsync(It.Is<RedisKey>(k => k == key), It.IsAny<CommandFlags>()),
            Times.Never);
    }

    [Fact]
    public async Task ExistsAsync_WhenRedisThrowsException_ShouldLogErrorAndReturnFalse()
    {
        // Arrange
        var key = "test-key";

        _mockDatabase.Setup(x => x.KeyExistsAsync(key, It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisException("Connection timeout"));

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeFalse();

        _mockLogger.Verify(x => x.LogError(
            It.Is<string>(s => s.Contains("Redis error while checking existence of key") && s.Contains(key)),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);
    }

    #endregion

    #region IsConnected Tests

    [Fact]
    public void IsConnected_WhenRedisIsConnected_ShouldReturnTrue()
    {
        // Arrange
        _mockRedis.Setup(x => x.IsConnected).Returns(true);

        // Act
        var result = _cacheService.IsConnected();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsConnected_WhenRedisIsNotConnected_ShouldReturnFalse()
    {
        // Arrange
        _mockRedis.Setup(x => x.IsConnected).Returns(false);

        // Act
        var result = _cacheService.IsConnected();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsConnected_WhenExceptionOccurs_ShouldLogErrorAndReturnFalse()
    {
        // Arrange
        _mockRedis.Setup(x => x.IsConnected).Throws(new Exception("Connection check failed"));

        // Act
        var result = _cacheService.IsConnected();

        // Assert
        result.Should().BeFalse();

        _mockLogger.Verify(x => x.LogError(
            It.Is<string>(s => s.Contains("Error checking Redis connection")),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()), Times.Once);
    }

    #endregion

    #region Integration and Edge Case Tests

    [Fact]
    public async Task CacheLifecycle_SetGetRemove_ShouldWorkCorrectly()
    {
        // Arrange
        var key = "lifecycle-test-key";
        var user = TestDataBuilder.BuildBasicUser();
        var expiration = TimeSpan.FromMinutes(5);
        var serializedUser = JsonSerializer.Serialize(user);

        _mockDatabase.Setup(x => x.StringSetAsync(
                key,
                It.IsAny<RedisValue>(),
                expiration,
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockDatabase.Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue(serializedUser));

        _mockDatabase.Setup(x => x.KeyExistsAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockDatabase.Setup(x => x.KeyExpireAsync(key, expiration, It.IsAny<ExpireWhen>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockDatabase.Setup(x => x.KeyDeleteAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act & Assert - Set
        await _cacheService.SetValueAsync(key, user, expiration);
        _mockDatabase.Verify(x => x.StringSetAsync(
            key,
            It.IsAny<RedisValue>(),
            expiration,
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), Times.Once);

        // Act & Assert - Get
        var retrievedUser = await _cacheService.GetValueAsync<User>(key, expiration);
        retrievedUser.Should().NotBeNull();
        retrievedUser.Id.Should().Be(user.Id);

        // Act & Assert - Remove
        await _cacheService.RemoveAsync(key);
        _mockDatabase.Verify(x => x.KeyDeleteAsync(key, It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task ConcurrentCacheOperations_ShouldHandleCorrectly()
    {
        // Arrange
        var keys = Enumerable.Range(1, 10).Select(i => $"concurrent-key-{i}").ToList();
        var users = TestDataBuilder.BuildUserList(10);
        var expiration = TimeSpan.FromMinutes(5);

        foreach (var key in keys)
            _mockDatabase.Setup(x => x.StringSetAsync(
                    key,
                    It.IsAny<RedisValue>(),
                    expiration,
                    It.IsAny<bool>(),
                    It.IsAny<When>(),
                    It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

        // Act
        var tasks = keys.Select((key, index) =>
            _cacheService.SetValueAsync(key, users[index], expiration)
        ).ToList();

        await Task.WhenAll(tasks);

        // Assert
        foreach (var key in keys)
            _mockDatabase.Verify(x => x.StringSetAsync(
                key,
                It.IsAny<RedisValue>(),
                expiration,
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task SetValueAsync_WithDifferentExpirationTimes_ShouldRespectEachDuration()
    {
        // Arrange
        var key1 = "short-expiry";
        var key2 = "long-expiry";
        var user = TestDataBuilder.BuildBasicUser();
        var shortExpiry = TimeSpan.FromMinutes(1);
        var longExpiry = TimeSpan.FromHours(1);

        _mockDatabase.Setup(x => x.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _cacheService.SetValueAsync(key1, user, shortExpiry);
        await _cacheService.SetValueAsync(key2, user, longExpiry);

        // Assert
        _mockDatabase.Verify(x => x.StringSetAsync(
            key1,
            It.IsAny<RedisValue>(),
            shortExpiry,
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), Times.Once);

        _mockDatabase.Verify(x => x.StringSetAsync(
            key2,
            It.IsAny<RedisValue>(),
            longExpiry,
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(GetVariousEntityTypes))]
    public async Task SetValueAsync_WithDifferentEntityTypes_ShouldSerializeCorrectly<T>(T entity) where T : class
    {
        // Arrange
        var key = $"test-{typeof(T).Name}";
        var expiration = TimeSpan.FromMinutes(5);

        _mockDatabase.Setup(x => x.StringSetAsync(
                key,
                It.IsAny<RedisValue>(),
                expiration,
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _cacheService.SetValueAsync(key, entity, expiration);

        // Assert
        _mockDatabase.Verify(x => x.StringSetAsync(
            key,
            It.IsAny<RedisValue>(),
            expiration,
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    public static IEnumerable<object[]> GetVariousEntityTypes()
    {
        yield return new object[] { TestDataBuilder.BuildBasicUser() };
        yield return new object[] { TestDataBuilder.BuildBasicPet() };
        yield return new object[] { TestDataBuilder.BuildAd() };
        yield return new object[] { TestDataBuilder.BuildUserList() };
        yield return new object[] { TestDataBuilder.BuildPetList() };
    }

    #endregion
}