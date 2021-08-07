using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Api.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }
        
        public DbSet<Products> Products { get; set; }
    }
}