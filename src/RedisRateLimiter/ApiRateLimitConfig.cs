using CommonLibs.RedisCache;
using StackExchange.Redis;

namespace CommonLibs.RedisRateLimiter;


public enum TimeUnit
{
  S = 1,
  M = 60,
  H = 3600,
  D = 86400
}

public class ApiRateLimitConfig
{
  // TODO: impose restriction on setters;
  public int Window {get; set;}
  public TimeUnit Unit {get; set;}
  public int MaxRequests { get; set;}
  public string? Path {get; set;}
  public string? UniqueClientIdentifier {get; set;}
  private int _windowInSeconds = 0;
  private string? _redisConfigKeyForClient = null;

  public int WindowInSeconds { get {
    if(_windowInSeconds > 0) {
      return _windowInSeconds;
    }
    _windowInSeconds = (int)Unit * Window;
    return _windowInSeconds;
  }}

  public string RedisConfigKeyForClient {
    get {
      if(_redisConfigKeyForClient is not null) {
        return _redisConfigKeyForClient;
      }
      _redisConfigKeyForClient = $"client:{UniqueClientIdentifier}|path:{Path}|window:{WindowInSeconds}";
      return _redisConfigKeyForClient;
    }
  }


}

public class ThrottleResponse
{
  public bool ShouldThrottle {get; set;}
}

public interface IApiThrottler
{
  Task<ThrottleResponse> ShouldThrottle(IEnumerable<ApiRateLimitConfig> rateLimitRules);
}

public class Throttler: IApiThrottler
{
  private readonly IRedisCacheManager _redisCacheManager;
  private const string _shouldThrottleLuaScript = @"
    local current_time = redis.call('TIME')
    local num_windows = ARGV[1]
    for i=2, num_windows*2, 2 do
        local window = ARGV[i]
        local max_requests = ARGV[i+1]
        local curr_key = KEYS[i/2]
        local trim_time = tonumber(current_time[1]) - window
        redis.call('ZREMRANGEBYSCORE', curr_key, 0, trim_time)
        local request_count = redis.call('ZCARD',curr_key)
        if request_count >= tonumber(max_requests) then
            return 1
        end
    end
    for i=2, num_windows*2, 2 do
        local curr_key = KEYS[i/2]
        local window = ARGV[i]
        redis.call('ZADD', curr_key, current_time[1], current_time[1] .. current_time[2])
        redis.call('EXPIRE', curr_key, window)
    end
    return 0
    ";

  public Throttler(IRedisCacheManager redisCacheManager)
  {
    _redisCacheManager = redisCacheManager;
  }

  public async Task<ThrottleResponse> ShouldThrottle(IEnumerable<ApiRateLimitConfig> rateLimitRules)
  {
    var orederRules = rateLimitRules.OrderBy(rule => rule.WindowInSeconds).ToList();
     var keys = orederRules.Select(x => new RedisKey(x.RedisConfigKeyForClient)).ToArray();
    var args = new List<RedisValue>{orederRules.Count()};
    foreach (var rule in orederRules)
    {
        args.Add(rule.WindowInSeconds);
        args.Add(rule.MaxRequests);
    }
    var shouldThrottle = (int) await _redisCacheManager.GetDatabase().ScriptEvaluateAsync(_shouldThrottleLuaScript, keys,args.ToArray()) == 1;
    return new ThrottleResponse
    {
      ShouldThrottle = shouldThrottle
    };
  }
}

