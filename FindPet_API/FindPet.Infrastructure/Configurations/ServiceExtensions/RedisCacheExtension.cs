using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace FindPet.Infrastructure.Configurations.ServiceExtensions;

public static class RedisCacheExtension
{
    public static void ConfigureRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");
        var instanceName = configuration["Redis:InstanceName"] ?? "FindPet_";

        if (string.IsNullOrEmpty(redisConnection))
        {
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger<IConnectionMultiplexer>>();
            logger?.LogWarning("Redis connection string is not configured. Caching will be disabled.");

            return;
        }

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();


            try
            {
                var configurationOptions = ConfigurationOptions.Parse(redisConnection, true);

                // Production-ready settings
                configurationOptions.AbortOnConnectFail = false; // Continue if Redis is down
                configurationOptions.ConnectTimeout = 10000; // 10 seconds
                configurationOptions.SyncTimeout = 5000; // 5 seconds
                configurationOptions.AsyncTimeout = 5000; // 5 seconds
                configurationOptions.ConnectRetry = 3; // Retry 3 times
                configurationOptions.KeepAlive = 60; // Keep connection alive
                configurationOptions.ReconnectRetryPolicy = new ExponentialRetry(5000); // Exponential backoff

                // Logging
                configurationOptions.ChannelPrefix = instanceName;

                var multiplexer = ConnectionMultiplexer.Connect(configurationOptions);

                // Connection events
                multiplexer.ConnectionFailed += (sender, args) =>
                {
                    logger.LogError($"Redis connection failed: {args.Exception?.Message ?? "Unknown error"}");
                };

                multiplexer.ConnectionRestored += (sender, args) =>
                {
                    logger.LogInformation("Redis connection restored");
                };

                multiplexer.ErrorMessage += (sender, args) => { logger.LogError($"Redis error: {args.Message}"); };

                logger.LogInformation("Redis connection established successfully");

                return multiplexer;
            }
            catch (RedisConnectionException ex)
            {
                logger.LogError($"Failed to connect to Redis: {ex.Message}");
                throw;
            }
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = instanceName;
        });
    }
}