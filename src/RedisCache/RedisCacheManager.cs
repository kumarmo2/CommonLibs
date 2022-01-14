using StackExchange.Redis;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
// using Newtonsoft.Json;
using ProtoBuf;
using System.IO;

namespace CommonLibs.RedisCache
{
    public class RedisCacheManager : IRedisCacheManager
    {
        private readonly Lazy<ConnectionMultiplexer> _redis;
        private readonly RedisOptions _redisOptions;
        public RedisCacheManager(IOptions<RedisOptions> redisOptions)
        {
            _redisOptions = redisOptions.Value;
            if (!_redisOptions.UseCache)
            {
                return;
            }
            _redis = new Lazy<ConnectionMultiplexer>(() =>
            {
                var options = new ConfigurationOptions();
                options.EndPoints.Add(redisOptions.Value.ConnectionString);
                options.ConnectTimeout = 5000;

                // These commands were actually used when envoy was used as proxy.
                // TODO: make these options optional using some configuration.
                // These unavailable commands are actually unavailable for envoy acting as redis proxy.
                // var unAvailableCommands = new HashSet<string>()
                // {
                // "ECHO"
                // };
                // var commandMap = CommandMap.Create(unAvailableCommands, false);
                // options.CommandMap = commandMap;
                return ConnectionMultiplexer.Connect(options);
            });
        }

        public IDatabase GetDatabase() => _redis.Value.GetDatabase();

        public async Task<T> GetRecord<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));
            }
            var cache = _redis.Value.GetDatabase();
            var redisValue = await cache.StringGetAsync(key);

            if (redisValue.IsNull)
            {
                return default(T);
            }
            // TODO: need to check that it doesn't fail for null values.
            return DeserializeRedisValue<T>(redisValue);
        }

        private T DeserializeRedisValue<T>(RedisValue redisValue)
        {
            // TODO: need to check that it doesn't fail for null values.
            var base64EncodedString = redisValue.ToString();
            var bytes = Convert.FromBase64String(base64EncodedString);
            return Serializer.Deserialize<T>(bytes.AsSpan());
        }

        public async Task<T> GetRecord<T>(string key, Func<Task<T>> dbCallback)
        {

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));
            }
            if (!_redisOptions.UseCache)
            {
                Console.WriteLine(">> Not Using Cache <<");
                return await dbCallback();
            }
            var cache = _redis.Value.GetDatabase();
            var redisValue = await cache.StringGetAsync(key);

            if (redisValue.IsNull)
            {
                Console.WriteLine("Found null in cache, calling dbCallback");
                var value = await dbCallback();
                await SetRecord(key, value);
                return value;
            }
            Console.WriteLine("Found value in cache");
            return DeserializeRedisValue<T>(redisValue);
        }

        public async Task RemoveKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));
            }
            if (!_redisOptions.UseCache)
            {
                return;
            }

            var cache = _redis.Value.GetDatabase();
            await cache.KeyDeleteAsync(key);
        }

        public async Task SetRecord<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));
            }
            if (!_redisOptions.UseCache)
            {
                return;
            }
            var cache = _redis.Value.GetDatabase();
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, value);
                // base64 encoding can be used to represent any binary data.
                var serialized = Convert.ToBase64String(stream.ToArray());
                await cache.StringSetAsync(key, serialized);
            }
        }
    }
}
