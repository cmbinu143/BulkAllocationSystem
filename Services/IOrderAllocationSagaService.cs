using BulkAllocation.Api.Models;

namespace BulkAllocation.Api.Services;

public interface IOrderAllocationSagaService
{
    Task CancelOrderAsync(Order order);
}