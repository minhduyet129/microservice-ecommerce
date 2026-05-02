using ProductService.Domain.Entities;

namespace ProductService.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsInStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
}