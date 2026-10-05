namespace BulkAllocation.Api.Services;

public interface IRedisService
{
    Task<bool> AcquireLockAsync(
        string key,
        string value,
        TimeSpan expiry);

    Task ReleaseLockAsync(
        string key,
        string value);

    Task SetBatchProgressAsync(
        string batchId,
        int totalOrders,
        int processedOrders,
        string status);

    Task<string?> GetBatchProgressAsync(
        string batchId);
}