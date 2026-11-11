using System.Collections.Generic;
using System.Threading.Tasks;
using FixMaster.Infrastructure;
using FixMaster.Models;

namespace FixMaster.Services.Interfaces;

public interface IClienteService
{
    Task<Resultado<IReadOnlyList<ClienteResumo>>> ListarAsync(string? filtro);
    Task<Resultado<ClienteResumo>> ObterAsync(int id);
    Task<Resultado> SalvarAsync(Cliente cliente);
    Task<Resultado> ExcluirOuInativarAsync(int id);
}
