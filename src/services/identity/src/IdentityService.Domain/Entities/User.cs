using BuildingBlocks.Core.Abstractions;

namespace IdentityService.Domain.Entities;

public class User : Entity<Guid>
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public User()
    {
        Id = Guid.NewGuid();
    }

    public static User Create(string email, string passwordHash, string fullName, string? phoneNumber = null)
    {
        return new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName,
            PhoneNumber = phoneNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}