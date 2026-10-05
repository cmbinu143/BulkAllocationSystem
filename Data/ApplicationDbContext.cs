using BulkAllocation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkAllocation.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<AllocationBatch> AllocationBatches => Set<AllocationBatch>();
    public DbSet<AllocationResult> AllocationResults => Set<AllocationResult>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseInMemoryDatabase("BulkAllocationDb");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.SKU)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Price);

            entity.HasIndex(x => x.SKU)
                .IsUnique();
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Product)
                .WithOne(x => x.Inventory)
                .HasForeignKey<Inventory>(x => x.ProductId);

            entity.HasIndex(x => x.ProductId)
                .IsUnique();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CustomerName)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.AllocationBatch)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.AllocationBatchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AllocationBatch>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.BatchId)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(x => x.BatchId)
                .IsUnique();
        });

        modelBuilder.Entity<AllocationResult>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Message)
                .HasMaxLength(500);

            entity.HasOne<Order>()
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}