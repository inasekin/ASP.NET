using Microsoft.EntityFrameworkCore;

namespace DockerBlazor.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}