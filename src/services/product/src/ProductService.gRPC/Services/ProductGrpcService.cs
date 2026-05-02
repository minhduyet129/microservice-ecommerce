using Grpc.Core;
using ProductService.Domain.Repositories;

namespace ProductService.gRPC.Services;

public class ProductGrpcService : ProductGrpc.ProductGrpcBase
{
    private readonly IProductRepository _productRepository;

    public ProductGrpcService(IProductRepository productRepository) { _productRepository = productRepository; }

    public override async Task<ProductResponse> GetProduct(ProductRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProductId, out var productId)) return new ProductResponse { IsValid = false };
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return new ProductResponse { IsValid = false };
        return new ProductResponse { Id = product.Id.ToString(), Name = product.Name, Description = product.Description ?? "", Price = (double)product.Price, DiscountPrice = product.DiscountPrice.HasValue ? (double)product.DiscountPrice.Value : 0, StockQuantity = product.StockQuantity, Sku = product.SKU ?? "", IsValid = true };
    }

    public override async Task<StockResponse> CheckStock(StockRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProductId, out var productId)) return new StockResponse { Success = false, Message = "Invalid product ID" };
        var isInStock = await _productRepository.IsInStockAsync(productId, request.Quantity);
        return new StockResponse { Success = isInStock, Message = isInStock ? "Stock available" : "Insufficient stock" };
    }

    public override async Task<StockResponse> ReduceStock(StockReductionRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProductId, out var productId)) return new StockResponse { Success = false, Message = "Invalid product ID" };
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return new StockResponse { Success = false, Message = "Product not found" };
        if (product.StockQuantity < request.Quantity) return new StockResponse { Success = false, Message = "Insufficient stock" };
        product.UpdateStock(-request.Quantity);
        await _productRepository.UpdateAsync(product);
        return new StockResponse { Success = true, Message = "Stock reduced successfully" };
    }
}