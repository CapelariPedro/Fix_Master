using System.Collections.Generic;
using System.Threading.Tasks;
using FixMaster.Infrastructure;

namespace FixMaster.Services.Interfaces;

public interface IAuthService
{
    // Consulta local e rápida; usada na abertura para decidir entre Login e Primeiro acesso.
    bool ExisteUsuario();

    Task<Resultado> CriarOperadorAsync(string usuario, string senha);

    Task<Resultado> CriarUsuarioAsync(
        string usuarioAdministrador,
        string senhaAdministrador,
        string usuario,
        string senha,
        bool ehAdministrador);

    Task<Resultado<IReadOnlyList<UsuarioResumo>>> ListarUsuariosAsync();

    Task<Resultado> ExcluirUsuarioAsync(string usuarioAdministrador, int idUsuario);

    Task<UsuarioAutenticado?> AutenticarAsync(string usuario, string senha);
}
