using Microsoft.EntityFrameworkCore;

namespace Stock.Api.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<Stocks> Stocks { get; set; }
    }
}