# Bulk Allocation System

A mini bulk order allocation system built using **ASP.NET Core Web API**, **Entity Framework Core**, **Redis**, and **SQL-based persistence**.

The system accepts multiple customer orders and allocates limited inventory based on business priority rules while supporting partial allocation, batch progress tracking, distributed locking, cancellation, and Saga-style compensation.

## Technology Stack

* .NET 9 / ASP.NET Core Web API
* C#
* Entity Framework Core
* SQL / In-Memory database configuration
* Redis
* Swagger / OpenAPI
* Repository Pattern
* Strategy Pattern
* Saga Compensation
* Dependency Injection
* REST APIs

## Architecture

The application follows a layered architecture:

```text
BulkAllocation.Api
│
├── Controllers
│   ├── AllocationController
│   ├── OrdersController
│   └── ProductsController
│
├── Data
│   ├── ApplicationDbContext
│   └── DbSeeder
│
├── DTOs
│
├── Enums
│
├── Models
│   ├── Order
│   ├── OrderItem
│   ├── Product
│   ├── Inventory
│   ├── AllocationBatch
│   └── AllocationResult
│
├── Repositories
│   ├── OrderRepository
│   ├── ProductRepository
│   └── InventoryRepository
│
├── Services
│   ├── AllocationService
│   ├── RedisService
│   └── OrderAllocationSagaService
│
└── Strategies
    └── PriorityAllocationStrategy
```

## Business Rules

Orders are allocated according to the following rules:

1. High-priority orders are processed before Medium-priority orders.
2. Medium-priority orders are processed before Low-priority orders.
3. Orders with the same priority are processed based on creation time.
4. Partial allocation is allowed when sufficient inventory is not available.
5. Inventory cannot be allocated beyond the available quantity.
6. Order and item allocation status is tracked.
7. Cancelled allocated orders trigger compensation and release reserved inventory.

## Allocation Status

The system supports:

* `Allocated`
* `PartiallyAllocated`
* `Failed`
* `Cancelled`
* `Compensated`

## Redis

Redis is used for:

### Distributed Lock

A Redis distributed lock prevents multiple allocation processes from running concurrently.

```text
bulk-allocation:lock
```

This helps prevent concurrent allocation processes from allocating the same inventory.

### Batch Progress

Redis stores allocation batch progress, including:

* Batch ID
* Total orders
* Processed orders
* Status
* Progress percentage

Example:

```json
{
  "batchId": "example-batch-id",
  "status": "Completed",
  "totalOrders": 4,
  "processedOrders": 4,
  "progress": 100
}
```

## Saga Compensation

The system implements Saga-style compensation for order cancellation.

### Normal allocation flow

```text
Create Orders
     ↓
Create Allocation Batch
     ↓
Acquire Redis Lock
     ↓
Sort Orders by Priority
     ↓
Reserve Inventory
     ↓
Allocate Order Items
     ↓
Update Order Status
     ↓
Update Redis Progress
     ↓
Complete Batch
     ↓
Release Redis Lock
```

### Compensation flow

When an allocated or partially allocated order is cancelled:

```text
Cancel Order
     ↓
Check Allocation
     ↓
Release Reserved Inventory
     ↓
Remove Allocation from Order Item
     ↓
Mark Order as Compensated
```

This ensures that inventory reserved for the cancelled order becomes available again.

## API Endpoints

### Products

```http
GET /api/products
```

Returns available products and inventory.

### Create Bulk Orders

```http
POST /api/orders/bulk
```

Example request:

```json
{
  "orders": [
    {
      "customerName": "Customer High",
      "priority": 3,
      "items": [
        {
          "productId": 1,
          "quantity": 3
        }
      ]
    },
    {
      "customerName": "Customer Medium",
      "priority": 2,
      "items": [
        {
          "productId": 2,
          "quantity": 5
        }
      ]
    }
  ]
}
```

### Get Orders

```http
GET /api/orders
```

### Get Order

```http
GET /api/orders/{id}
```

### Cancel Order

```http
POST /api/orders/{id}/cancel
```

For an allocated or partially allocated order, the cancellation triggers Saga compensation.

### Run Allocation

```http
POST /api/allocation/run
```

Starts the bulk allocation process.

### Get Batch Progress

```http
GET /api/allocation/batches/{batchId}
```

Returns the current batch status and progress.

## Example Allocation Result

Example response:

```json
{
  "batchId": "337647bff0f14bd19d0182e82781c030",
  "status": "Completed",
  "totalOrders": 4,
  "allocatedOrders": 3,
  "partiallyAllocatedOrders": 1,
  "failedOrders": 0
}
```

## Design Patterns Used

### Repository Pattern

Repositories separate data-access logic from business logic.

Examples:

* `IOrderRepository`
* `IProductRepository`
* `IInventoryRepository`

### Strategy Pattern

`IAllocationStrategy` is used to determine the order processing sequence.

Current implementation:

```text
PriorityAllocationStrategy
```

This keeps allocation ordering independent from the allocation service.

### Saga Pattern

`OrderAllocationSagaService` handles compensation when an allocated order is cancelled.

### Dependency Injection

Services and repositories are registered through ASP.NET Core dependency injection.

## SOLID Principles

The implementation follows SOLID principles by separating responsibilities:

* Controllers handle HTTP requests and responses.
* Services contain business logic.
* Repositories handle persistence.
* Strategies handle allocation ordering.
* Redis service handles distributed locking and progress.
* Saga service handles compensation.

Interfaces are used to keep components loosely coupled and testable.

## Running the Project

### Prerequisites

Install:

* .NET 9 SDK
* Redis
* Visual Studio 2022 or another .NET-compatible IDE

### Start Redis

Make sure Redis is running before starting the API.

Default Redis configuration:

```text
localhost:6379
```

### Run the API

From the project directory:

```bash
dotnet restore
dotnet build
dotnet run
```

Swagger is available when the application starts.

Example:

```text
http://localhost:5145/swagger
```

The actual port may vary depending on the local launch configuration.

## Testing the Allocation Flow

1. Get products:

```http
GET /api/products
```

2. Create multiple orders:

```http
POST /api/orders/bulk
```

3. Run allocation:

```http
POST /api/allocation/run
```

4. Check orders:

```http
GET /api/orders
```

5. Check batch progress:

```http
GET /api/allocation/batches/{batchId}
```

6. Cancel an allocated or partially allocated order:

```http
POST /api/orders/{id}/cancel
```

7. Verify that the order is `Compensated` and inventory has been released.

## Assumptions

* Inventory is limited and shared across orders.
* Partial allocation is permitted.
* Payment and external confirmation are simulated/not implemented as external services.
* Redis is used for distributed locking and batch progress.
* Allocation is protected against concurrent execution using a Redis lock.

## Completed Features

* [x] Product API
* [x] Bulk order creation
* [x] Priority-based allocation
* [x] Partial allocation
* [x] Failed allocation handling
* [x] Allocation batch tracking
* [x] Redis distributed locking
* [x] Redis batch progress
* [x] Repository Pattern
* [x] Strategy Pattern
* [x] Saga compensation
* [x] Order cancellation
* [x] Inventory release during compensation
* [x] Swagger API documentation

## Future Improvements

Possible enhancements include:

* Unit and integration tests
* Docker Compose for API, database, and Redis
* Idempotency support
* CSV order upload
* Payment service integration
* Message queue integration
* Improved transaction handling for distributed Saga steps
* React frontend integration

## Repository

GitHub:

https://github.com/cmbinu143/BulkAllocationSystem
