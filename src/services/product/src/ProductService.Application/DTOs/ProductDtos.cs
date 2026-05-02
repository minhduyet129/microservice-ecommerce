namespace ProductService.Application.DTOs;

public record ProductDto(Guid Id, string Name, string? Description, decimal Price, decimal? DiscountPrice, int StockQuantity, string? ImageUrl, Guid CategoryId, string? SKU);
public record CreateProductRequest(string Name, string? Description, decimal Price, int StockQuantity, Guid CategoryId, string? SKU);
public record UpdateProductRequest(Guid Id, string Name, string? Description, decimal Price, int StockQuantity, Guid CategoryId, decimal? DiscountPrice);