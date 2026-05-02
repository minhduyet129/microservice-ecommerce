using ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Infrastructure.Persistence;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.SKU).IsUnique();
        });
    }
}