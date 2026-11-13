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

public sealed class OrdemServicoService : IOrdemServicoService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public OrdemServicoService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Resultado<IReadOnlyList<OrdemServico>>> ListarAsync()
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var ordens = await db.OrdensServico
                .AsNoTracking()
                .Include(ordem => ordem.Agendamento)
                .ThenInclude(agendamento => agendamento!.Cliente)
                .Include(ordem => ordem.Itens)
                .ThenInclude(item => item.ItemEstoque)
                .OrderByDescending(ordem => ordem.DataEmissao)
                .ToListAsync();
            return Resultado<IReadOnlyList<OrdemServico>>.Ok(ordens);
        }
        catch (Exception exception) when (exception is DbException or InvalidOperationException)
        {
            return Resultado<IReadOnlyList<OrdemServico>>.Falha(
                $"Não foi possível carregar as ordens de serviço: {exception.Message}");
        }
    }

    public async Task<Resultado<IReadOnlyList<AgendamentoResumo>>> ListarAgendamentosDisponiveisAsync()
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var agendamentos = await db.Agendamentos
                .AsNoTracking()
                .Where(agendamento =>
                    agendamento.Status != StatusAgendamento.Cancelado
                    && !db.OrdensServico.Any(ordem => ordem.AgendamentoId == agendamento.Id))
                .OrderByDescending(agendamento => agendamento.DataHoraInicio)
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
        catch (Exception exception) when (exception is DbException or InvalidOperationException)
        {
            return Resultado<IReadOnlyList<AgendamentoResumo>>.Falha(
                $"Não foi possível carregar os agendamentos disponíveis: {exception.Message}");
        }
    }

    public async Task<Resultado<OrdemServico>> ObterAsync(int id)
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var ordem = await db.OrdensServico
                .AsNoTracking()
                .Include(item => item.Agendamento)
                .ThenInclude(agendamento => agendamento!.Cliente)
                .Include(item => item.Itens)
                .ThenInclude(item => item.ItemEstoque)
                .FirstOrDefaultAsync(item => item.Id == id);
            return ordem is null
                ? Resultado<OrdemServico>.Falha("Ordem de serviço não encontrada.")
                : Resultado<OrdemServico>.Ok(ordem);
        }
        catch (Exception exception) when (exception is DbException or InvalidOperationException)
        {
            return Resultado<OrdemServico>.Falha(
                $"Não foi possível carregar a ordem de serviço: {exception.Message}");
        }
    }

    public async Task<Resultado<int>> CriarAsync(OrdemServico ordem)
    {
        var erro = Validar(ordem);
        if (erro is not null)
            return Resultado<int>.Falha(erro);

        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var agendamentoValido = await db.Agendamentos.AnyAsync(agendamento =>
                agendamento.Id == ordem.AgendamentoId
                && agendamento.Status != StatusAgendamento.Cancelado);
            if (!agendamentoValido)
                return Resultado<int>.Falha("Selecione um agendamento válido e não cancelado.");

            if (await db.OrdensServico.AnyAsync(item => item.AgendamentoId == ordem.AgendamentoId))
                return Resultado<int>.Falha("Já existe uma ordem de serviço para este agendamento.");

            foreach (var item in ordem.Itens)
            {
                var estoque = await db.ItensEstoque
                    .AsNoTracking()
                    .FirstOrDefaultAsync(material => material.Id == item.ItemEstoqueId && material.Ativo);
                if (estoque is null)
                    return Resultado<int>.Falha("Um dos materiais selecionados não está disponível.");
                item.ValorUnitario = estoque.PrecoUnitario;
            }

            ordem.RelatorioTecnico = ordem.RelatorioTecnico.Trim();
            ordem.DataEmissao = DateTime.Now;
            ordem.DataConclusao = null;
            ordem.Status = StatusOS.Aberta;
            db.OrdensServico.Add(ordem);
            await db.SaveChangesAsync();
            return Resultado<int>.Ok(ordem.Id);
        }
        catch (DbUpdateException exception)
        {
            return Resultado<int>.Falha(
                $"Não foi possível criar a ordem de serviço: {exception.GetBaseException().Message}");
        }
        catch (DbException exception)
        {
            return Resultado<int>.Falha(
                $"Não foi possível criar a ordem de serviço: {exception.Message}");
        }
    }

    public async Task<Resultado> AtualizarAsync(OrdemServico ordem)
    {
        var erro = Validar(ordem);
        if (erro is not null)
            return Resultado.Falha(erro);

        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var atual = await db.OrdensServico
                .Include(item => item.Itens)
                .FirstOrDefaultAsync(item => item.Id == ordem.Id);
            if (atual is null)
                return Resultado.Falha("Ordem de serviço não encontrada.");
            if (atual.Status != StatusOS.Aberta)
                return Resultado.Falha("Somente ordens abertas podem ser alteradas.");
            if (atual.AgendamentoId != ordem.AgendamentoId)
                return Resultado.Falha("O agendamento vinculado à ordem não pode ser alterado.");

            atual.RelatorioTecnico = ordem.RelatorioTecnico.Trim();
            atual.MaoDeObra = ordem.MaoDeObra;
            var itensAtuais = atual.Itens.ToDictionary(item => item.ItemEstoqueId);
            var idsRecebidos = ordem.Itens.Select(item => item.ItemEstoqueId).ToHashSet();
            db.ItensOS.RemoveRange(atual.Itens.Where(item => !idsRecebidos.Contains(item.ItemEstoqueId)));

            foreach (var item in ordem.Itens)
            {
                if (itensAtuais.TryGetValue(item.ItemEstoqueId, out var itemExistente))
                {
                    itemExistente.QuantidadeUtilizada = item.QuantidadeUtilizada;
                    continue;
                }

                var estoque = await db.ItensEstoque
                    .AsNoTracking()
                    .FirstOrDefaultAsync(material => material.Id == item.ItemEstoqueId && material.Ativo);
                if (estoque is null)
                    return Resultado.Falha("Um dos novos materiais selecionados não está disponível.");

                atual.Itens.Add(new ItemOS
                {
                    ItemEstoqueId = estoque.Id,
                    QuantidadeUtilizada = item.QuantidadeUtilizada,
                    ValorUnitario = estoque.PrecoUnitario,
                    OrdemServicoId = atual.Id
                });
            }

            await db.SaveChangesAsync();
            return Resultado.Ok("Ordem de serviço atualizada.");
        }
        catch (DbUpdateException exception)
        {
            return Resultado.Falha(
                $"Não foi possível atualizar a ordem de serviço: {exception.GetBaseException().Message}");
        }
        catch (DbException exception)
        {
            return Resultado.Falha(
                $"Não foi possível atualizar a ordem de serviço: {exception.Message}");
        }
    }

    public async Task<Resultado> ConcluirAsync(int ordemServicoId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        try
        {
            await using var transacao = await db.Database.BeginTransactionAsync();
            var ordem = await db.OrdensServico
                .Include(item => item.Agendamento)
                .Include(item => item.Itens)
                .ThenInclude(item => item.ItemEstoque)
                .FirstOrDefaultAsync(item => item.Id == ordemServicoId);
            if (ordem is null)
                return Resultado.Falha("Ordem de serviço não encontrada.");
            if (ordem.Status == StatusOS.Concluida)
                return Resultado.Falha("Esta ordem de serviço já foi concluída.");
            if (ordem.Status != StatusOS.Aberta || ordem.Agendamento is null)
                return Resultado.Falha("Esta ordem não pode ser concluída.");

            foreach (var item in ordem.Itens)
            {
                if (item.QuantidadeUtilizada <= 0
                    || item.ItemEstoque is null
                    || item.ItemEstoque.Quantidade < item.QuantidadeUtilizada)
                {
                    return Resultado.Falha(
                        $"Estoque insuficiente: {item.ItemEstoque?.Nome ?? "material indisponível"}.");
                }
            }

            foreach (var item in ordem.Itens)
                item.ItemEstoque!.Quantidade -= item.QuantidadeUtilizada;

            ordem.Status = StatusOS.Concluida;
            ordem.DataConclusao = DateTime.Now;
            ordem.Agendamento.Status = StatusAgendamento.Concluido;
            db.AtividadesRecentes.Add(new AtividadeRecente
            {
                DataHora = DateTime.Now,
                Tipo = "ORDEM_SERVICO_CONCLUIDA",
                Descricao = $"OS #{ordem.Id} concluída. Total: {CalcularTotal(ordem):C2}",
                ReferenciaId = ordem.Id
            });

            await db.SaveChangesAsync();
            await transacao.CommitAsync();
            return Resultado.Ok("Ordem concluída e estoque atualizado.");
        }
        catch (Exception exception) when (
            exception is DbUpdateException or DbException or InvalidOperationException)
        {
            return Resultado.Falha(
                $"Falha ao concluir a ordem. Nenhuma alteração foi aplicada: {exception.GetBaseException().Message}");
        }
    }

    public static decimal CalcularTotal(OrdemServico ordem) =>
        ordem.MaoDeObra + ordem.Itens.Sum(item => item.QuantidadeUtilizada * item.ValorUnitario);

    private static string? Validar(OrdemServico ordem)
    {
        if (ordem.AgendamentoId <= 0)
            return "Selecione um agendamento.";
        if (ordem.MaoDeObra is < 0 or > 99999999.99m)
            return "O valor da mão de obra deve estar entre R$ 0,00 e R$ 99.999.999,99.";
        if (ordem.RelatorioTecnico is null || string.IsNullOrWhiteSpace(ordem.RelatorioTecnico))
            return "Informe o relatório técnico.";
        if (ordem.RelatorioTecnico.Trim().Length > 5000)
            return "O relatório técnico deve ter no máximo 5.000 caracteres.";
        if (ordem.Itens.Any(item => item.ItemEstoqueId <= 0 || item.QuantidadeUtilizada <= 0))
            return "Informe materiais e quantidades inteiras maiores que zero.";
        if (ordem.Itens.GroupBy(item => item.ItemEstoqueId).Any(grupo => grupo.Count() > 1))
            return "Cada material pode ser incluído apenas uma vez na ordem.";
        return null;
    }
}
