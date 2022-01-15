using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CommonLibs.RedisCache
{
    public interface IRedisCacheManager
    {
        IDatabase GetDatabase();
        Task SetRecord<T>(string key, T value);
        Task<T> GetRecord<T>(string key);
        Task<T> GetRecord<T>(string key, Func<Task<T>> dbCallback);
        Task RemoveKey(string key);
        Task LPush<T>(string key, T value);
        Task LPush<T>(string key, IEnumerable<T> values);
        Task RPush<T>(string key, T value);
        Task<long> LLen(string key);
        Task<IEnumerable<T>> LRange<T>(string key, long start = 0, long stop = -1);
        Task<T> LPop<T>(string key);
        Task<T> RPop<T>(string key);
    }
}
