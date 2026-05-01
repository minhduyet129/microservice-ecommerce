using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (!await context.Users.AnyAsync())
        {
            var adminUser = User.Create(
                email: "admin@ecommerce.com",
                passwordHash: BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                fullName: "Administrator",
                phoneNumber: "+1234567890"
            );

            context.Users.Add(adminUser);

            var adminRole = UserRole.Create(adminUser.Id, "Admin");
            var userRole = UserRole.Create(adminUser.Id, "User");

            context.UserRoles.AddRange(adminRole, userRole);

            await context.SaveChangesAsync();
        }
    }
}