using Microsoft.EntityFrameworkCore;

namespace Entities.Testing.Infrastructure.Data;

public class ProductContext : DbContext
{
    public ProductContext(DbContextOptions<ProductContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<UserAccount> UserAccounts { get; set; } = null!;
}