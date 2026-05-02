using MediatR;
using ProductService.Application.DTOs;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Queries;

public record GetAllProductsQuery : IRequest<List<ProductDto>>;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetAllProductsQueryHandler(IProductRepository productRepository) { _productRepository = productRepository; }

    public async Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken ct)
    {
        var products = await _productRepository.GetAllAsync(ct);
        return products.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.DiscountPrice, p.StockQuantity, p.ImageUrl, p.CategoryId, p.SKU)).ToList();
    }
}

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;