using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FixMaster.Data;
using FixMaster.Infrastructure;
using FixMaster.Models;
using FixMaster.Services.Interfaces;
using Microsoft.Data.Sqlite;
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

        var validacao = ValidarNovoUsuario(usuario, senha);
        if (validacao is not null)
            return validacao;

        await using var db = await _factory.CreateDbContextAsync();

        if (await db.Usuarios.AnyAsync())
            return Resultado.Falha("Já existe um operador cadastrado.");

        db.Usuarios.Add(new Usuario
        {
            Username = usuario,
            SenhaHash = PasswordHasher.Hash(senha),
            EhAdministrador = true
        });
        await db.SaveChangesAsync();
        return Resultado.Ok();
    }

    public async Task<Resultado> CriarUsuarioAsync(
        string usuarioAdministrador,
        string senhaAdministrador,
        string usuario,
        string senha,
        bool ehAdministrador)
    {
        usuario = usuario?.Trim() ?? "";
        usuarioAdministrador = usuarioAdministrador?.Trim() ?? "";

        var validacao = ValidarNovoUsuario(usuario, senha);
        if (validacao is not null)
            return validacao;

        if (string.IsNullOrWhiteSpace(usuarioAdministrador)
            || string.IsNullOrEmpty(senhaAdministrador))
        {
            return Resultado.Falha("Credenciais de administrador inválidas.");
        }

        await using var db = await _factory.CreateDbContextAsync();
        var administrador = await db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == usuarioAdministrador);

        if (administrador is null
            || !administrador.EhAdministrador
            || !PasswordHasher.Verify(senhaAdministrador, administrador.SenhaHash))
        {
            return Resultado.Falha("Credenciais de administrador inválidas.");
        }

        if (await db.Usuarios.AnyAsync(u => u.Username == usuario))
            return Resultado.Falha("Já existe um usuário com esse nome.");

        db.Usuarios.Add(new Usuario
        {
            Username = usuario,
            SenhaHash = PasswordHasher.Hash(senha),
            EhAdministrador = ehAdministrador
        });

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqliteException
                  {
                      SqliteExtendedErrorCode: 2067
                  })
        {
            return Resultado.Falha("Já existe um usuário com esse nome.");
        }
        catch (DbUpdateException exception)
        {
            var detalhe = exception.InnerException?.Message ?? exception.Message;
            return Resultado.Falha($"Não foi possível salvar o usuário: {detalhe}");
        }

        return Resultado.Ok();
    }

    public async Task<Resultado<IReadOnlyList<UsuarioResumo>>> ListarUsuariosAsync()
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var usuarios = await db.Usuarios
                .AsNoTracking()
                .OrderBy(u => u.Username)
                .Select(u => new UsuarioResumo(u.Id, u.Username, u.EhAdministrador))
                .ToListAsync();

            return Resultado<IReadOnlyList<UsuarioResumo>>.Ok(usuarios);
        }
        catch (SqliteException exception)
        {
            return Resultado<IReadOnlyList<UsuarioResumo>>.Falha(
                $"Não foi possível carregar os usuários: {exception.Message}");
        }
    }

    public async Task<Resultado> ExcluirUsuarioAsync(string usuarioAdministrador, int idUsuario)
    {
        usuarioAdministrador = usuarioAdministrador?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(usuarioAdministrador))
            return Resultado.Falha("Administrador inválido.");

        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var administrador = await db.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == usuarioAdministrador);
            if (administrador is null || !administrador.EhAdministrador)
                return Resultado.Falha("Somente um administrador pode excluir usuários.");

            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == idUsuario);
            if (usuario is null)
                return Resultado.Falha("O usuário selecionado não foi encontrado.");

            if (string.Equals(usuario.Username, administrador.Username, StringComparison.OrdinalIgnoreCase))
                return Resultado.Falha("Não é permitido excluir o próprio usuário.");

            if (usuario.EhAdministrador
                && await db.Usuarios.CountAsync(u => u.EhAdministrador) <= 1)
            {
                return Resultado.Falha("Não é possível excluir o último administrador.");
            }

            db.Usuarios.Remove(usuario);
            await db.SaveChangesAsync();
            return Resultado.Ok();
        }
        catch (DbUpdateException exception)
        {
            var detalhe = exception.InnerException?.Message ?? exception.Message;
            return Resultado.Falha($"Não foi possível excluir o usuário: {detalhe}");
        }
        catch (SqliteException exception)
        {
            return Resultado.Falha($"Não foi possível excluir o usuário: {exception.Message}");
        }
    }

    public async Task<UsuarioAutenticado?> AutenticarAsync(string usuario, string senha)
    {
        usuario = usuario?.Trim() ?? "";

        await using var db = await _factory.CreateDbContextAsync();
        var encontrado = await db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == usuario);

        if (encontrado is null)
        {
            PasswordHasher.Verify(senha, HashFicticio);
            return null;
        }

        return PasswordHasher.Verify(senha, encontrado.SenhaHash)
            ? new UsuarioAutenticado(encontrado.Username, encontrado.EhAdministrador)
            : null;
    }

    private static Resultado? ValidarNovoUsuario(string usuario, string senha)
    {
        if (usuario.Length < 3 || usuario.Length > 50)
            return Resultado.Falha("O usuário deve ter entre 3 e 50 caracteres.");
        if (string.IsNullOrEmpty(senha) || senha.Length < 8)
            return Resultado.Falha("A senha deve ter ao menos 8 caracteres.");

        return null;
    }
}
