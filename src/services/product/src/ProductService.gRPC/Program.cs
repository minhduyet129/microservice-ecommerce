using ProductService.gRPC.Services;
using ProductService.Infrastructure.Persistence;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddDbContext<ProductDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();

app.MapGrpcService<ProductGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();