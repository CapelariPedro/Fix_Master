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

public sealed class AgendaService : IAgendaService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public AgendaService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Resultado<IReadOnlyList<AgendamentoResumo>>> ListarDiaAsync(DateTime data)
    {
        try
        {
            var inicio = data.Date;
            var fim = inicio.AddDays(1);
            await using var db = await _factory.CreateDbContextAsync();
            var agendamentos = await db.Agendamentos
                .AsNoTracking()
                .Where(agendamento =>
                    agendamento.DataHoraInicio >= inicio
                    && agendamento.DataHoraInicio < fim)
                .OrderBy(agendamento => agendamento.DataHoraInicio)
                .Select(agendamento => new AgendamentoResumo(
                    agendamento.Id,
                    agendamento.ClienteId,
                    agendamento.Cliente!.Nome,
                    agendamento.DataHoraInicio,
                    agendamento.DataHoraFim,
                    agendamento.DuracaoMinutos,
                    agendamento.DescricaoServico,
                    agendamento.Status))
                .ToListAsync();

            return Resultado<IReadOnlyList<AgendamentoResumo>>.Ok(agendamentos);
        }
        catch (SqliteException exception)
        {
            return Resultado<IReadOnlyList<AgendamentoResumo>>.Falha(
                $"Não foi possível carregar a agenda do dia: {exception.Message}");
        }
    }

    public async Task<Resultado<IReadOnlyList<DateTime>>> ListarMesAsync(int ano, int mes)
    {
        if (ano is < 1 or > 9999 || mes is < 1 or > 12)
            return Resultado<IReadOnlyList<DateTime>>.Falha("Informe um mês e ano válidos.");

        try
        {
            var inicio = new DateTime(ano, mes, 1);
            var fim = inicio.AddMonths(1);
            await using var db = await _factory.CreateDbContextAsync();
            var datas = await db.Agendamentos
                .AsNoTracking()
                .Where(agendamento =>
                    agendamento.Status != StatusAgendamento.Cancelado
                    && agendamento.DataHoraInicio >= inicio
                    && agendamento.DataHoraInicio < fim)
                .Select(agendamento => agendamento.DataHoraInicio.Date)
                .Distinct()
                .OrderBy(data => data)
                .ToListAsync();

            return Resultado<IReadOnlyList<DateTime>>.Ok(datas);
        }
        catch (SqliteException exception)
        {
            return Resultado<IReadOnlyList<DateTime>>.Falha(
                $"Não foi possível carregar os dias com atendimentos: {exception.Message}");
        }
    }

    public async Task<Resultado> SalvarAsync(Agendamento agendamento)
    {
        var erroValidacao = Validar(agendamento);
        if (erroValidacao is not null)
            return Resultado.Falha(erroValidacao);

        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var clienteExiste = await db.Clientes.AnyAsync(cliente =>
                cliente.Id == agendamento.ClienteId && cliente.Ativo);
            if (!clienteExiste)
                return Resultado.Falha("Selecione um cliente ativo para o atendimento.");

            var conflito = await BuscarConflitoAsync(
                db,
                agendamento.DataHoraInicio,
                agendamento.DuracaoMinutos,
                agendamento.Id == 0 ? null : agendamento.Id);
            if (conflito is not null)
            {
                return Resultado.Falha(
                    $"Conflito de horário com {conflito.ClienteNome}, "
                    + $"{conflito.Horario}: {conflito.DescricaoServico}.");
            }

            agendamento.DataHoraFim =
                agendamento.DataHoraInicio.AddMinutes(agendamento.DuracaoMinutos);
            agendamento.DescricaoServico = agendamento.DescricaoServico.Trim();

            if (agendamento.Id == 0)
            {
                agendamento.Status = StatusAgendamento.Pendente;
                db.Agendamentos.Add(agendamento);
            }
            else
            {
                var existente = await db.Agendamentos
                    .FirstOrDefaultAsync(item => item.Id == agendamento.Id);
                if (existente is null)
                    return Resultado.Falha("O agendamento que você tentou editar não foi encontrado.");

                existente.ClienteId = agendamento.ClienteId;
                existente.DataHoraInicio = agendamento.DataHoraInicio;
                existente.DuracaoMinutos = agendamento.DuracaoMinutos;
                existente.DataHoraFim = agendamento.DataHoraFim;
                existente.DescricaoServico = agendamento.DescricaoServico;
            }

            await db.SaveChangesAsync();
            return Resultado.Ok();
        }
        catch (DbUpdateException exception)
        {
            var detalhe = exception.InnerException?.Message ?? exception.Message;
            return Resultado.Falha($"Não foi possível salvar o agendamento: {detalhe}");
        }
        catch (SqliteException exception)
        {
            return Resultado.Falha($"Não foi possível salvar o agendamento: {exception.Message}");
        }
    }

    public async Task<Resultado> AlterarStatusAsync(int id, StatusAgendamento status)
    {
        if (!Enum.IsDefined(status))
            return Resultado.Falha("O status informado não é válido.");

        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var agendamento = await db.Agendamentos
                .FirstOrDefaultAsync(item => item.Id == id);
            if (agendamento is null)
                return Resultado.Falha("O agendamento selecionado não foi encontrado.");

            if (status != StatusAgendamento.Cancelado)
            {
                var conflito = await BuscarConflitoAsync(
                    db,
                    agendamento.DataHoraInicio,
                    agendamento.DuracaoMinutos,
                    agendamento.Id);
                if (conflito is not null)
                {
                    return Resultado.Falha(
                        $"Não foi possível reativar o horário: conflito com "
                        + $"{conflito.ClienteNome}, {conflito.Horario}.");
                }
            }

            agendamento.Status = status;
            await db.SaveChangesAsync();
            return Resultado.Ok();
        }
        catch (DbUpdateException exception)
        {
            var detalhe = exception.InnerException?.Message ?? exception.Message;
            return Resultado.Falha($"Não foi possível alterar o status: {detalhe}");
        }
        catch (SqliteException exception)
        {
            return Resultado.Falha($"Não foi possível alterar o status: {exception.Message}");
        }
    }

    public async Task<Resultado<AgendamentoResumo?>> BuscarConflitoAsync(
        DateTime inicio,
        int duracaoMinutos,
        int? ignorarId = null)
    {
        if (duracaoMinutos is < 1 or > 1440)
            return Resultado<AgendamentoResumo?>.Falha("A duração deve ser entre 1 e 1440 minutos.");

        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var conflito = await BuscarConflitoAsync(db, inicio, duracaoMinutos, ignorarId);
            return Resultado<AgendamentoResumo?>.Ok(conflito);
        }
        catch (SqliteException exception)
        {
            return Resultado<AgendamentoResumo?>.Falha(
                $"Não foi possível verificar conflitos: {exception.Message}");
        }
    }

    public async Task<Resultado<bool>> TemConflitoAsync(
        DateTime inicio,
        int duracaoMinutos,
        int? ignorarId = null)
    {
        var resultado = await BuscarConflitoAsync(inicio, duracaoMinutos, ignorarId);
        return resultado.Sucesso
            ? Resultado<bool>.Ok(resultado.Valor is not null)
            : Resultado<bool>.Falha(resultado.Mensagem ?? "Não foi possível verificar conflitos.");
    }

    private static async Task<AgendamentoResumo?> BuscarConflitoAsync(
        AppDbContext db,
        DateTime inicio,
        int duracaoMinutos,
        int? ignorarId)
    {
        var fim = inicio.AddMinutes(duracaoMinutos);
        return await db.Agendamentos
            .AsNoTracking()
            .Where(agendamento =>
                agendamento.Status != StatusAgendamento.Cancelado
                && (!ignorarId.HasValue || agendamento.Id != ignorarId.Value)
                && agendamento.DataHoraInicio < fim
                && agendamento.DataHoraFim > inicio)
            .OrderBy(agendamento => agendamento.DataHoraInicio)
            .Select(agendamento => new AgendamentoResumo(
                agendamento.Id,
                agendamento.ClienteId,
                agendamento.Cliente!.Nome,
                agendamento.DataHoraInicio,
                agendamento.DataHoraFim,
                agendamento.DuracaoMinutos,
                agendamento.DescricaoServico,
                agendamento.Status))
            .FirstOrDefaultAsync();
    }

    private static string? Validar(Agendamento agendamento)
    {
        if (agendamento.ClienteId <= 0)
            return "Selecione um cliente para o atendimento.";
        if (agendamento.DuracaoMinutos is < 1 or > 1440)
            return "A duração deve ser entre 1 e 1440 minutos.";
        if (string.IsNullOrWhiteSpace(agendamento.DescricaoServico))
            return "Informe a descrição do serviço.";
        if (agendamento.DescricaoServico.Trim().Length > 500)
            return "A descrição do serviço deve ter no máximo 500 caracteres.";
        if (agendamento.DataHoraInicio == default)
            return "Informe uma data e hora válidas.";
        if (agendamento.DataHoraInicio > DateTime.MaxValue.AddMinutes(-agendamento.DuracaoMinutos))
            return "Informe uma data e hora válidas.";

        return null;
    }
}
