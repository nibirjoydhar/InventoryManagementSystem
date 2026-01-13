using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Inventory.Infrastructure.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connection = "Host=localhost;Database=InventoryDb;Username=postgres;Password=root";
            optionsBuilder.UseNpgsql(connection);
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
