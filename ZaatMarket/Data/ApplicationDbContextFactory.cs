using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ZaatMarket.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use the same PostgreSQL connection string as appsettings.json
        optionsBuilder.UseNpgsql("Data Source=App_Data/zaatmarket.db");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}