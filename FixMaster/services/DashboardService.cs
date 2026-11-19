using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using FixMaster.Data;
using FixMaster.Infrastructure;
using FixMaster.Models;
using FixMaster.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FixMaster.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public DashboardService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Resultado<DashboardIndicadores>> ObterIndicadoresDoDiaAsync()
    {
        try
        {
            var inicio = DateTime.Today;
            var fim = inicio.AddDays(1);

            await using var db = await _factory.CreateDbContextAsync();
            var indicadores = await db.Agendamentos
                .AsNoTracking()
                .Where(agendamento =>
                    agendamento.DataHoraInicio >= inicio
                    && agendamento.DataHoraInicio < fim
                    && agendamento.Status != StatusAgendamento.Cancelado)
                .GroupBy(agendamento => agendamento.Status)
                .Select(grupo => new
                {
                    Status = grupo.Key,
                    Quantidade = grupo.Count()
                })
                .ToListAsync();

            var pendentes = indicadores.FirstOrDefault(item => item.Status == StatusAgendamento.Pendente)?.Quantidade ?? 0;
            var emAndamento = indicadores.FirstOrDefault(item => item.Status == StatusAgendamento.EmAndamento)?.Quantidade ?? 0;
            var concluidos = indicadores.FirstOrDefault(item => item.Status == StatusAgendamento.Concluido)?.Quantidade ?? 0;
            var total = pendentes + emAndamento + concluidos;

            return Resultado<DashboardIndicadores>.Ok(new DashboardIndicadores
            {
                Marcados = total,
                Pendentes = pendentes,
                EmAndamento = emAndamento,
                Concluidos = concluidos
            });
        }
        catch (Exception exception) when (exception is DbException or InvalidOperationException)
        {
            return Resultado<DashboardIndicadores>.Falha(
                $"Não foi possível carregar os indicadores do dia: {exception.Message}");
        }
    }

    public async Task<Resultado<IReadOnlyList<AgendamentoResumo>>> ObterAgendamentosDoDiaAsync()
    {
        try
        {
            var inicio = DateTime.Today;
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
                    agendamento.Status,
                    db.OrdensServico.Any(ordem => ordem.AgendamentoId == agendamento.Id)))
                .ToListAsync();

            return Resultado<IReadOnlyList<AgendamentoResumo>>.Ok(agendamentos);
        }
        catch (Exception exception) when (exception is DbException or InvalidOperationException)
        {
            return Resultado<IReadOnlyList<AgendamentoResumo>>.Falha(
                $"Não foi possível carregar os agendamentos do dia: {exception.Message}");
        }
    }

    public async Task<Resultado<IReadOnlyList<AtividadeRecente>>> ObterAtividadesRecentesAsync(int quantidade = 20)
    {
        if (quantidade <= 0)
            return Resultado<IReadOnlyList<AtividadeRecente>>.Ok(Array.Empty<AtividadeRecente>());

        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var atividades = await db.AtividadesRecentes
                .AsNoTracking()
                .OrderByDescending(atividade => atividade.DataHora)
                .Take(quantidade)
                .ToListAsync();

            return Resultado<IReadOnlyList<AtividadeRecente>>.Ok(atividades);
        }
        catch (Exception exception) when (exception is DbException or InvalidOperationException)
        {
            return Resultado<IReadOnlyList<AtividadeRecente>>.Falha(
                $"Não foi possível carregar as atividades recentes: {exception.Message}");
        }
    }

    public async Task<Resultado<IReadOnlyList<OrdemServicoResumo>>> ObterOrdensParaConcluirAsync()
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var ordens = await db.OrdensServico
                .AsNoTracking()
                .Include(ordem => ordem.Agendamento).ThenInclude(agendamento => agendamento!.Cliente)
                .Include(ordem => ordem.Itens)
                .Where(ordem => ordem.Status == StatusOS.Aberta)
                .OrderBy(ordem => ordem.Agendamento!.DataHoraInicio)
                .ToListAsync();

            return Resultado<IReadOnlyList<OrdemServicoResumo>>.Ok(
                ordens.Select(Mapear).ToList());
        }
        catch (Exception exception) when (exception is DbException or InvalidOperationException)
        {
            return Resultado<IReadOnlyList<OrdemServicoResumo>>.Falha(
                $"Não foi possível carregar as OS a concluir: {exception.Message}");
        }
    }

    public async Task<Resultado<IReadOnlyList<OrdemServicoResumo>>> ObterOrdensConcluidasDoDiaAsync()
    {
        try
        {
            var inicio = DateTime.Today;
            var fim = inicio.AddDays(1);

            await using var db = await _factory.CreateDbContextAsync();
            var ordens = await db.OrdensServico
                .AsNoTracking()
                .Include(ordem => ordem.Agendamento).ThenInclude(agendamento => agendamento!.Cliente)
                .Include(ordem => ordem.Itens)
                .Where(ordem => ordem.Status == StatusOS.Concluida
                    && ordem.DataConclusao >= inicio
                    && ordem.DataConclusao < fim)
                .OrderByDescending(ordem => ordem.DataConclusao)
                .ToListAsync();

            return Resultado<IReadOnlyList<OrdemServicoResumo>>.Ok(
                ordens.Select(Mapear).ToList());
        }
        catch (Exception exception) when (exception is DbException or InvalidOperationException)
        {
            return Resultado<IReadOnlyList<OrdemServicoResumo>>.Falha(
                $"Não foi possível carregar as OS concluídas hoje: {exception.Message}");
        }
    }

    private static OrdemServicoResumo Mapear(OrdemServico ordem) => new(
        ordem.Id,
        ordem.AgendamentoId,
        ordem.Agendamento?.Cliente?.Nome ?? "Cliente",
        ordem.Agendamento?.DescricaoServico ?? "",
        ordem.Agendamento?.DataHoraInicio ?? default,
        OrdemServicoService.CalcularTotal(ordem));
}