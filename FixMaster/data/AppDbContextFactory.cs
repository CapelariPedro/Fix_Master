using Microsoft.EntityFrameworkCore;

namespace FixMaster.Data;

// Um contexto curto por operação (ver docs/01-ARQUITETURA.md).
public class AppDbContextFactory : IDbContextFactory<AppDbContext>
{
    private readonly DbContextOptions<AppDbContext> _options;

    public AppDbContextFactory(DbContextOptions<AppDbContext> options) => _options = options;

    public AppDbContext CreateDbContext() => new(_options);
}
