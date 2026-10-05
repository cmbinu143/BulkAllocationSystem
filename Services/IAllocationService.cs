namespace BulkAllocation.Api.Services;

public interface IAllocationService
{
    Task<int> RunAllocationAsync();
}