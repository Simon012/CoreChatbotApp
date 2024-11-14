using CoreChatbotApp.Infrastructure.DataLayer.Cache;
using CoreChatbotApp.Utilities;
using CoreChatbotApp.Utilities.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreChatbotApp.Infrastructure.DataLayer.Cache.Redis
{
    public class RedisCacheManager : ICacheManager
    {
        private readonly IDatabase _cache;
        private readonly ILogger _logger;
        private readonly string _cachePrefix;
        private readonly string _conversationLockKeyPrefix = Enum.GetName(StorageType.ConversationLock);

        public RedisCacheManager(IRedisCacheConnection connection, ILogger<RedisCacheManager> logger, IOptions<CacheConfig> options)
        {
            _cache = connection.Connection.GetDatabase();
            _logger = logger;
            _cachePrefix = options.Value.CacheAppPrefix + "_" + options.Value.CacheSlotPrefix;
        }

        public Task<bool> AcquireLockAsync(string UserIdAad, int timeoutInSeconds = 60)
        {
            string lockKey = $"{_cachePrefix}_{_conversationLockKeyPrefix}_{UserIdAad}";

            // Try to acquire the lock by setting a key with a short expiry time
            // The value "1" is arbitrary, it's just to set something at the key
            // The When.NotExists condition ensures that this will only succeed if the key doesn't exist
            return _cache.StringSetAsync(lockKey, "1", TimeSpan.FromSeconds(timeoutInSeconds), When.NotExists);
        }

        public Task<bool> ReleaseLockAsync(string UserIdAad)
        {
            string lockKey = $"{_cachePrefix}_{_conversationLockKeyPrefix}_{UserIdAad}";
            // Release the lock by deleting the key
            return _cache.KeyDeleteAsync(lockKey);
        }

        public async Task<bool> FlushCache()
        {
            // Get all keys
            IEnumerable<RedisKey> keys = _cache.Multiplexer.GetServer(_cache.Multiplexer.GetEndPoints().First()).Keys();

            // Delete all keys that match the prefix
            foreach (RedisKey key in keys)
            {
                if (key.ToString().StartsWith(_cachePrefix))
                {
                    await _cache.KeyDeleteAsync(key);
                }
            }

            return true;
        }

        public async Task<string> GetStringAsync(StorageType storageType, string UserIdAad)
        {
            string actualKey = $"{_cachePrefix}_{Enum.GetName(storageType)}_{UserIdAad}";

            string temp = await _cache.StringGetAsync(actualKey);

            if (temp == null)
            {
                return null;
            }
            // (string str, DateTime? timestamp) = UntimestampString(temp);

            if (string.IsNullOrEmpty(temp))
            {
                return null;
            }
            else
            {
                return temp;
            }
        }

        public Task<bool> SetStringAsync(StorageType storageType, string UserIdAad, string value, TimeSpan? expiry = null)
        {
            string actualKey = $"{_cachePrefix}_{Enum.GetName(storageType)}_{UserIdAad}";

            if (string.IsNullOrEmpty(value))
            {
                return Task.FromResult(false);
            }

            // string timestampedValue = TimestampString(value);

            _logger.LogTrace($"{Constants.SystemConstants.LoggingPrefix} CacheOp: Setting key {actualKey} with value {value} and expiry {expiry}");

            return _cache.StringSetAsync(actualKey, value, expiry);
        }

        public Task<bool> RemoveAsync(StorageType storageType, string UserIdAad)
        {
            string actualKey = $"{_cachePrefix}_{Enum.GetName(storageType)}_{UserIdAad}";
            return _cache.KeyDeleteAsync(actualKey);
        }

        public string TimestampString(string str)
        {
            return $"{str};{DateTime.UtcNow}";
        }

        public (string str, DateTime? timestamp) UntimestampString(string str)
        {
            string[] parts = str.Split(';');
            if (parts.Length < 2)
            {
                _logger.LogError($"{Constants.SystemConstants.LoggingPrefix} Cache-Error: Timestamp not found in string: {str}");
                return (str, null); // Use null to indicate absence of a valid timestamp
            }
            else
            {
                if (DateTime.TryParse(parts[parts.Length - 1], out DateTime parsedTimestamp))
                {
                    // Reconstruct the original string if necessary
                    string originalString = string.Join(";", parts.Take(parts.Length - 1));
                    return (originalString, parsedTimestamp);
                }
                else
                {
                    _logger.LogError($"{Constants.SystemConstants.LoggingPrefix} Cache-Error: Unable to parse timestamp from string: {str}");
                    return (str, null); // Indicate failure to parse timestamp
                }
            }
        }
    }
}
