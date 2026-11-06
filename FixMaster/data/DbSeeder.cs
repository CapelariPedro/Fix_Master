using System.Linq;
using FixMaster.Models;
using FixMaster.Services;
using Microsoft.EntityFrameworkCore;

namespace FixMaster.Data;

public sealed class DbSeeder
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public DbSeeder(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public void Seed()
    {
        using var db = _factory.CreateDbContext();
        if (db.Usuarios.Any())
            return;

        db.Usuarios.Add(new Usuario
        {
            Username = "admin",
            SenhaHash = PasswordHasher.Hash("admin")
        });
        db.SaveChanges();
    }
}
