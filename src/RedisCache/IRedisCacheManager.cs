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
    }
}