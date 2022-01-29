using StackExchange.Redis;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
// using Newtonsoft.Json;
using ProtoBuf;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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

        public async Task RPush<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty", nameof(key));
            }
            var cache = _redis.Value.GetDatabase();
            var serializedValue = Serialize(value);
            await cache.ListRightPushAsync(key, serializedValue);
        }

        public async Task LPush<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty", nameof(key));
            }
            var cache = _redis.Value.GetDatabase();
            var serializedValue = Serialize(value);
            await cache.ListLeftPushAsync(key, serializedValue);
        }
        public async Task LPush<T>(string key, IEnumerable<T> values)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty", nameof(key));
            }
            var cache = _redis.Value.GetDatabase();
            var serializedValues = values.Select(value => new RedisValue(Serialize(value))).ToArray();
            // var serializedValue = Serialize(value);
            await cache.ListLeftPushAsync(key, serializedValues);
        }

        public async Task<long> LLen(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty", nameof(key));
            }
            var cache = _redis.Value.GetDatabase();
            return await cache.ListLengthAsync(key);
        }

        // Returns:
        //     True if the specified member was not already present in the set, else False
        public async Task<bool> SAdd<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty", nameof(key));
            }
            var cache = _redis.Value.GetDatabase();
            var serialized = Serialize(value);
            return await cache.SetAddAsync(key, serialized);
        }

        public async Task<long> SCard(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty", nameof(key));
            }
            var cache = _redis.Value.GetDatabase();
            return await cache.SetLengthAsync(key);
        }

        public async Task<T> SPop<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty", nameof(key));
            }
            var cache = _redis.Value.GetDatabase();
            var value = await cache.SetPopAsync(key);
            if (value == RedisValue.Null)
            {
                return default(T);
            }
            return DeserializeRedisValue<T>(value);
        }

        public async Task<IEnumerable<T>> SMembers<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty", nameof(key));
            }
            var cache = _redis.Value.GetDatabase();
            var values = await cache.SetMembersAsync(key);
            if (values is null)
            {
                return Enumerable.Empty<T>();
            }
            return values.Select(value => DeserializeRedisValue<T>(value));
        }

        public async Task<IEnumerable<T>> LRange<T>(string key, long start = 0, long stop = -1)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty", nameof(key));
            }
            var cache = _redis.Value.GetDatabase();
            var values = await cache.ListRangeAsync(key, start, stop);
            if (values is null)
            {
                return Enumerable.Empty<T>();
            }
            return values.Select(value => DeserializeRedisValue<T>(value));
        }

        public async Task<T> RPop<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty", nameof(key));
            }
            var cache = _redis.Value.GetDatabase();
            var value = await cache.ListRightPopAsync(key);

            if (value == RedisValue.Null)
            {
                return default(T);
            }
            return DeserializeRedisValue<T>(value);
        }

        public async Task<T> LPop<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty", nameof(key));
            }
            var cache = _redis.Value.GetDatabase();
            var value = await cache.ListLeftPopAsync(key);

            if (value == RedisValue.Null)
            {
                return default(T);
            }
            return DeserializeRedisValue<T>(value);
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

        private string Serialize<T>(T value)
        {
            string serialized = null;

            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, value);
                serialized = Convert.ToBase64String(stream.ToArray());
            }
            return serialized;
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
            var serialized = Serialize(value);
            await cache.StringSetAsync(key, serialized);
        }
    }
}
