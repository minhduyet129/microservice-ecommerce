namespace IdentityService.Application.DTOs;

public record RegisterRequest(string Email, string Password, string FullName, string? PhoneNumber);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Email, string FullName, List<string> Roles);