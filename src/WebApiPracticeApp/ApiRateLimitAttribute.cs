using CommonLibs.RedisRateLimiter;

namespace CommonLibs.WebApiPracticeApp;


[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
class ApiRateLimitAttribute: Attribute {
  public int Window {get; set;}
  public TimeUnit Unit {get; set;}
  public int MaxRequests { get; set;}
}

