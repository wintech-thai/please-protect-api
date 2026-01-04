using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using System.Text.Json;
using StackExchange.Redis;

namespace Its.Otep.Api.Utils
{
    public class RedisHelper : IRedisHelper
    {
        private readonly IDatabase _db;
        private readonly RedLockFactory _redlockFactory;

        public RedisHelper(IConnectionMultiplexer connection)
        {
            _db = connection.GetDatabase();
            _redlockFactory = RedLockFactory.Create(new List<RedLockMultiplexer>
            {
                new RedLockMultiplexer(connection)
            });
        }

        public Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
            => _db.StringSetAsync(key, value, expiry);

        public async Task<string?> GetAsync(string key)
        {
            var value = await _db.StringGetAsync(key);
            return value.HasValue ? value.ToString() : null;
        }

        public async Task SetObjectAsync<T>(string key, T obj, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(obj);
            await _db.StringSetAsync(key, json, expiry);
        }

        public async Task<T?> GetObjectAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty) return default;
            
            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task<string> PublishMessageAsync(string stream, string message)
        {
            var msgId = await _db.StreamAddAsync(stream,
                [new NameValueEntry("message", message)]);
                
            return msgId.ToString();
        }

        // --- เพิ่ม RedLock wrapper ---
        public async Task<IRedLock> AcquireRedLockAsync(
            string resource,
            TimeSpan expiry,
            TimeSpan? wait = null,
            TimeSpan? retry = null)
        {
            wait ??= TimeSpan.FromSeconds(5);
            retry ??= TimeSpan.FromMilliseconds(200);

            var redLock = await _redlockFactory.CreateLockAsync(
                resource,
                expiry,
                wait.Value,
                retry.Value
            );

            return redLock;
        }

        public Task<bool> DeleteAsync(string key)
            => _db.KeyDeleteAsync(key);
    }
}
