using ProductService.gRPC;

namespace OrderService.Application.Services;

public interface IProductGrpcClient
{
    Task<ProductResponse?> GetProductAsync(Guid productId);
    Task<bool> ReduceStockAsync(Guid productId, int quantity);
}

public class ProductGrpcClient : IProductGrpcClient
{
    private readonly ProductGrpc.ProductGrpcClient _client;

    public ProductGrpcClient(ProductGrpc.ProductGrpcClient client) { _client = client; }

    public async Task<ProductResponse?> GetProductAsync(Guid productId)
    {
        try
        {
            var response = await _client.GetProductAsync(new ProductRequest { ProductId = productId.ToString() });
            return response.IsValid ? response : null;
        }
        catch { return null; }
    }

    public async Task<bool> ReduceStockAsync(Guid productId, int quantity)
    {
        try
        {
            var response = await _client.ReduceStockAsync(new StockReductionRequest { ProductId = productId.ToString(), Quantity = quantity });
            return response.Success;
        }
        catch { return false; }
    }
}