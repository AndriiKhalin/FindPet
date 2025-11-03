using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace FindPet.Infrastructure.Configurations.ServiceExtensions;

public static class RedisCacheExtension
{
    public static void ConfigureRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");

        if (string.IsNullOrEmpty(redisConnection))
        {
            throw new InvalidOperationException("Redis connection string is not configured.");
        }

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configurationOptions = ConfigurationOptions.Parse(redisConnection, true);
            configurationOptions.AbortOnConnectFail = false;
            configurationOptions.ConnectTimeout = 10000;
            configurationOptions.SyncTimeout = 10000;

            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = configuration["Redis:InstanceName"] ?? "FindPet_";
        });
    }
}