using System.Text.Json;
using StackExchange.Redis;

namespace BulkAllocation.Api.Services;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<bool> AcquireLockAsync(
        string key,
        string value,
        TimeSpan expiry)
    {
        var database = _redis.GetDatabase();

        return await database.StringSetAsync(
            key,
            value,
            expiry,
            When.NotExists);
    }

    public async Task ReleaseLockAsync(
        string key,
        string value)
    {
        var database = _redis.GetDatabase();

        const string script = """
            if redis.call("GET", KEYS[1]) == ARGV[1] then
                return redis.call("DEL", KEYS[1])
            end
            return 0
            """;

        await database.ScriptEvaluateAsync(
            script,
            new RedisKey[] { key },
            new RedisValue[] { value });
    }

    public async Task SetBatchProgressAsync(
        string batchId,
        int totalOrders,
        int processedOrders,
        string status)
    {
        var database = _redis.GetDatabase();

        var progress = totalOrders == 0
            ? 0
            : (int)((double)processedOrders / totalOrders * 100);

        var data = new
        {
            batchId,
            status,
            totalOrders,
            processedOrders,
            progress
        };

        var json = JsonSerializer.Serialize(data);

        await database.StringSetAsync(
            $"allocation:batch:{batchId}",
            json,
            TimeSpan.FromHours(1));
    }

    public async Task<string?> GetBatchProgressAsync(
        string batchId)
    {
        var database = _redis.GetDatabase();

        return await database.StringGetAsync(
            $"allocation:batch:{batchId}");
    }
}