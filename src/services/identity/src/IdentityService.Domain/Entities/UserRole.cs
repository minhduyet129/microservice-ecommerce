using BuildingBlocks.Core.Abstractions;

namespace IdentityService.Domain.Entities;

public class UserRole : Entity<Guid>
{
    public Guid UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;

    public UserRole()
    {
        Id = Guid.NewGuid();
    }

    public static UserRole Create(Guid userId, string roleName)
    {
        return new UserRole
        {
            UserId = userId,
            RoleName = roleName,
            CreatedAt = DateTime.UtcNow
        };
    }
}