using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CommonLibs.RedisCache
{
    public static class RedisCacheExtensions
    {
        public static void AddRedisCacheManager(this IServiceCollection services, IConfiguration config)
        {
            if (services is null)
            {
                throw new System.ArgumentNullException(nameof(services));
            }

            if (config is null)
            {
                throw new System.ArgumentNullException(nameof(config));
            }

            services.Configure<RedisOptions>(config.GetSection(RedisOptions.Key));
            services.AddSingleton<IRedisCacheManager, RedisCacheManager>();
        }
    }
}