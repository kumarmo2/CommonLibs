using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CommonLibs.RedisCache;
namespace CommonLibs.RedisRateLimiter.Extensions
{
    public static class IServiceCollectionExtension
    {
        public static void AddApiThrottler(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRedisCacheManager(configuration);
            services.AddSingleton<IApiThrottler, Throttler>();
        }
    }
}
