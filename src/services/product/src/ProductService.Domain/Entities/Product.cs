using BuildingBlocks.Core.Abstractions;

namespace ProductService.Domain.Entities;

public class Product : Entity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? SKU { get; set; }

    public Product()
    {
        Id = Guid.NewGuid();
    }

    public static Product Create(string name, string? description, decimal price, int stockQuantity, Guid categoryId, string? sku = null)
    {
        return new Product { Name = name, Description = description, Price = price, StockQuantity = stockQuantity, CategoryId = categoryId, SKU = sku ?? $"SKU-{Guid.NewGuid().ToString()[..8].ToUpper()}", IsActive = true, CreatedAt = DateTime.UtcNow };
    }

    public void UpdateStock(int quantity)
    {
        StockQuantity += quantity;
        if (StockQuantity < 0) StockQuantity = 0;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}