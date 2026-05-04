using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using OrderService.Application.Commands;
using OrderService.Application.DTOs;
using OrderService.Application.Services;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Persistence.Repositories;
using OrderService.Domain.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using ProductService.gRPC;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderCommand>();

builder.Services.AddGrpcClient<ProductGrpc.ProductGrpcClient>(options => { options.Address = new Uri(builder.Configuration["GrpcSettings:ProductServiceUrl"]!); });

// MassTransit commented out due to NuGet package issue
// builder.Services.AddMassTransit(x =>
// {
//     x.UsingRabbitMq((context, cfg) => { cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h => { h.Username(builder.Configuration["RabbitMQ:Username"]!); h.Password(builder.Configuration["RabbitMQ:Password"]!); }); });
// });

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductGrpcClient, ProductGrpcClient>();

builder.Services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new() { ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidAudience = builder.Configuration["Jwt:Audience"], IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)) };
});

builder.Services.AddAuthorization();

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddHealthChecks().AddDbContextCheck<OrderDbContext>("sqlserver");

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();