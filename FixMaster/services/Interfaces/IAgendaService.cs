using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FixMaster.Infrastructure;
using FixMaster.Models;

namespace FixMaster.Services.Interfaces;

public interface IAgendaService
{
    Task<Resultado<IReadOnlyList<AgendamentoResumo>>> ListarDiaAsync(DateTime data);
    Task<Resultado<IReadOnlyList<DateTime>>> ListarMesAsync(int ano, int mes);
    Task<Resultado> SalvarAsync(Agendamento agendamento);
    Task<Resultado> AlterarStatusAsync(int id, StatusAgendamento status);
    Task<Resultado<AgendamentoResumo?>> BuscarConflitoAsync(
        DateTime inicio,
        int duracaoMinutos,
        int? ignorarId = null);
    Task<Resultado<bool>> TemConflitoAsync(
        DateTime inicio,
        int duracaoMinutos,
        int? ignorarId = null);
}
