using IdentityService.Application.DTOs;
using IdentityService.Domain.Repositories;
using MediatR;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Application.Queries;

public record LoginQuery(LoginRequest Request) : IRequest<AuthResponse>;

public class LoginQueryValidator : AbstractValidator<LoginQuery>
{
    public LoginQueryValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Request.Password).NotEmpty();
    }
}

public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public LoginQueryHandler(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Request.Email, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Request.Password, user.PasswordHash))
        {
            throw new Exception("Invalid credentials");
        }

        if (!user.IsActive)
        {
            throw new Exception("User account is deactivated");
        }

        user.UpdateLastLogin();

        var token = GenerateJwtToken(user);

        return new AuthResponse(token, user.Email, user.FullName, new List<string> { "User" });
    }

    private string GenerateJwtToken(Domain.Entities.User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, "User")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}