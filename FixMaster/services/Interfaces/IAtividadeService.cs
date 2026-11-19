using System.Threading.Tasks;
using FixMaster.Infrastructure;

namespace FixMaster.Services.Interfaces;

public interface IAtividadeService
{
    Task<Resultado> RegistrarAsync(string tipo, string descricao, int? referenciaId = null);
}
