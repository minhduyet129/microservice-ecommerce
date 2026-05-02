using FluentValidation;
using MediatR;
using ProductService.Application.DTOs;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Commands;

public record CreateProductCommand(CreateProductRequest Request) : IRequest<ProductDto>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Price).GreaterThan(0);
        RuleFor(x => x.Request.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;

    public CreateProductCommandHandler(IProductRepository productRepository) { _productRepository = productRepository; }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = Domain.Entities.Product.Create(request.Request.Name, request.Request.Description, request.Request.Price, request.Request.StockQuantity, request.Request.CategoryId, request.Request.SKU);
        await _productRepository.AddAsync(product, ct);
        return new ProductDto(product.Id, product.Name, product.Description, product.Price, product.DiscountPrice, product.StockQuantity, product.ImageUrl, product.CategoryId, product.SKU);
    }
}

public record UpdateProductCommand(UpdateProductRequest Request) : IRequest<ProductDto>;
public record DeleteProductCommand(Guid Id) : IRequest<bool>;