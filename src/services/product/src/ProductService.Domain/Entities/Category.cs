using BuildingBlocks.Core.Abstractions;

namespace ProductService.Domain.Entities;

public class Category : Entity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public Category()
    {
        Id = Guid.NewGuid();
    }

    public static Category Create(string name, string? description = null, int sortOrder = 0)
    {
        return new Category { Name = name, Description = description, SortOrder = sortOrder, IsActive = true, CreatedAt = DateTime.UtcNow };
    }
}