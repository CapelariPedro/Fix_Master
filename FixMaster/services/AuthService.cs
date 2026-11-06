using System.Linq;
using System.Threading.Tasks;
using FixMaster.Data;
using FixMaster.Infrastructure;
using FixMaster.Models;
using FixMaster.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FixMaster.Services;

public class AuthService : IAuthService
{
    // Usado quando o usuário não existe, para o tempo de resposta não revelar isso.
    private static readonly string HashFicticio = PasswordHasher.Hash("senha-ficticia");

    private readonly IDbContextFactory<AppDbContext> _factory;

    public AuthService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public bool ExisteUsuario()
    {
        using var db = _factory.CreateDbContext();
        return db.Usuarios.Any();
    }

    public async Task<Resultado> CriarOperadorAsync(string usuario, string senha)
    {
        usuario = usuario?.Trim() ?? "";

        if (usuario.Length < 3 || usuario.Length > 50)
            return Resultado.Falha("O usuário deve ter entre 3 e 50 caracteres.");
        if (string.IsNullOrEmpty(senha) || senha.Length < 8)
            return Resultado.Falha("A senha deve ter ao menos 8 caracteres.");

        await using var db = await _factory.CreateDbContextAsync();

        if (await db.Usuarios.AnyAsync())
            return Resultado.Falha("Já existe um operador cadastrado.");

        db.Usuarios.Add(new Usuario
        {
            Username = usuario,
            SenhaHash = PasswordHasher.Hash(senha)
        });
        await db.SaveChangesAsync();
        return Resultado.Ok();
    }

    public async Task<bool> AutenticarAsync(string usuario, string senha)
    {
        usuario = usuario?.Trim() ?? "";

        await using var db = await _factory.CreateDbContextAsync();
        var encontrado = await db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == usuario);

        if (encontrado is null)
        {
            PasswordHasher.Verify(senha, HashFicticio);
            return false;
        }

        return PasswordHasher.Verify(senha, encontrado.SenhaHash);
    }
}
