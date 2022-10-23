namespace CommonLibs.RedisRateLimiter.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CommonLibs.RedisCache;


public static class IServiceCollectionExtension
{
  public static void AddApiThrottler(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddRedisCacheManager(configuration);
    services.AddSingleton<IApiThrottler, Throttler>();
  }
}



