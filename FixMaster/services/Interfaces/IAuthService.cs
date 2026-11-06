using System.Threading.Tasks;
using FixMaster.Infrastructure;

namespace FixMaster.Services.Interfaces;

public interface IAuthService
{
    // Consulta local e rápida; usada na abertura para decidir entre Login e Primeiro acesso.
    bool ExisteUsuario();

    Task<Resultado> CriarOperadorAsync(string usuario, string senha);

    Task<bool> AutenticarAsync(string usuario, string senha);
}
