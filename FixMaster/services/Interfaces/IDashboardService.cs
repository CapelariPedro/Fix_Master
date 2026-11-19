using System.Collections.Generic;
using System.Threading.Tasks;
using FixMaster.Infrastructure;
using FixMaster.Models;

namespace FixMaster.Services.Interfaces;

public interface IDashboardService
{
    Task<Resultado<DashboardIndicadores>> ObterIndicadoresDoDiaAsync();
    Task<Resultado<IReadOnlyList<AgendamentoResumo>>> ObterAgendamentosDoDiaAsync();
    Task<Resultado<IReadOnlyList<AtividadeRecente>>> ObterAtividadesRecentesAsync(int quantidade = 20);
    Task<Resultado<IReadOnlyList<OrdemServicoResumo>>> ObterOrdensParaConcluirAsync();
    Task<Resultado<IReadOnlyList<OrdemServicoResumo>>> ObterOrdensConcluidasDoDiaAsync();
}