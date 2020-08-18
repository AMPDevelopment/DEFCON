using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;

namespace Defcon.Library.Extensions
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Add StackExchange.Redis with its serialization provider.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="redisConfiguration">The redis configuration.</param>
        /// <typeparam name="T">The type of serializer. <see cref="ISerializer" />.</typeparam>
        public static IServiceCollection AddStackExchangeRedisExtensions<T>(this IServiceCollection services, RedisConfiguration redisConfiguration)
            where T : class, ISerializer, new()
        {
            services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
            services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
            services.AddSingleton<ISerializer, T>();

            services.AddSingleton(provider => provider.GetRequiredService<IRedisCacheClient>().GetDbFromConfiguration());

            services.AddSingleton(redisConfiguration);

            return services;
        }
    }
}