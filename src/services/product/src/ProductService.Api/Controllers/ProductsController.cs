using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Commands;
using ProductService.Application.DTOs;
using ProductService.Application.Queries;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator) { _mediator = mediator; }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll() => Ok(await _mediator.Send(new GetAllProductsQuery()));

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequest request)
    {
        var result = await _mediator.Send(new CreateProductCommand(request));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> Delete(Guid id) => Ok(await _mediator.Send(new DeleteProductCommand(id)));
}