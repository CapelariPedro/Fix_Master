using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FixMaster.Data;

// Usada só pelo comando "dotnet ef" para gerar migrações.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=fixmaster.design.db3")
            .Options;
        return new AppDbContext(options);
    }
}
