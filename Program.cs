using BulkAllocation.Api.Data;
using StackExchange.Redis;
using BulkAllocation.Api.Repositories;
using BulkAllocation.Api.Services;
using BulkAllocation.Api.Strategies;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>();

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379"));

builder.Services.AddScoped<IRedisService, RedisService>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

builder.Services.AddScoped<IAllocationService, AllocationService>();

builder.Services.AddScoped<IOrderAllocationSagaService,
    OrderAllocationSagaService>();

builder.Services.AddScoped<IAllocationStrategy,
    PriorityAllocationStrategy>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await DbSeeder.SeedAsync(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Temporarily disabled so HTTP Swagger/API works reliably
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();