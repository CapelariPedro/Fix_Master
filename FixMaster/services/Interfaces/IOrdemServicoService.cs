using System.Collections.Generic;
using System.Threading.Tasks;
using FixMaster.Infrastructure;
using FixMaster.Models;

namespace FixMaster.Services.Interfaces;

public interface IOrdemServicoService
{
    Task<Resultado<IReadOnlyList<OrdemServico>>> ListarAsync();
    Task<Resultado<IReadOnlyList<AgendamentoResumo>>> ListarAgendamentosDisponiveisAsync();
    Task<Resultado<OrdemServico>> ObterAsync(int id);
    Task<Resultado<int>> CriarAsync(OrdemServico ordem);
    Task<Resultado> AtualizarAsync(OrdemServico ordem);
    Task<Resultado> ConcluirAsync(int ordemServicoId);
}
