using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context) { _context = context; }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) 
        => await _context.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default) 
        => await _context.Products.Where(p => p.IsActive).ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default) 
        => await _context.Products.Where(p => p.CategoryId == categoryId && p.IsActive).ToListAsync(ct);

    public async Task<Product> AddAsync(Product product, CancellationToken ct = default)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _context.Products.FindAsync(new object[] { id }, ct);
        if (product != null) { product.Deactivate(); await _context.SaveChangesAsync(ct); }
    }

    public async Task<bool> IsInStockAsync(Guid productId, int quantity, CancellationToken ct = default)
    {
        var product = await _context.Products.FindAsync(new object[] { productId }, ct);
        return product != null && product.StockQuantity >= quantity;
    }
}