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

public sealed class EstoqueService : IEstoqueService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public EstoqueService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Resultado<string>> GerarCodigoAsync(
        string categoria,
        string? tipoInsumo = null)
    {
        if (!CategoriaMaterial.TentarObterPrefixo(categoria, tipoInsumo, out var prefixo))
            return Resultado<string>.Falha("Tipo: selecione um tipo de material válido.");

        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            for (var tentativa = 0; tentativa < 20; tentativa++)
            {
                var codigo = CategoriaMaterial.GerarCodigo(prefixo);
                if (!await db.ItensEstoque.AnyAsync(item => item.Codigo == codigo))
                    return Resultado<string>.Ok(codigo);
            }

            return Resultado<string>.Falha(
                "Não foi possível gerar um código disponível. Tente novamente.");
        }
        catch (SqliteException exception)
        {
            return Resultado<string>.Falha(
                $"Não foi possível gerar o código do material: {exception.Message}");
        }
    }

    public async Task<Resultado<IReadOnlyList<ItemEstoque>>> BuscarAsync(string? termo)
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var consulta = db.ItensEstoque.AsNoTracking().Where(item => item.Ativo);
            termo = termo?.Trim();

            IReadOnlyList<ItemEstoque> itens = await consulta
                .OrderBy(item => item.Nome)
                .ThenBy(item => item.Codigo)
                .ToListAsync();

            if (!string.IsNullOrEmpty(termo))
            {
                itens = itens
                    .Where(item =>
                        TextoHelper.Contem(item.Nome, termo)
                        || TextoHelper.Contem(item.Codigo, termo)
                        || TextoHelper.Contem(item.Categoria, termo)
                        || TextoHelper.Contem(item.Descricao, termo))
                    .ToList();
            }

            return Resultado<IReadOnlyList<ItemEstoque>>.Ok(itens);
        }
        catch (SqliteException exception)
        {
            return Resultado<IReadOnlyList<ItemEstoque>>.Falha(
                $"Não foi possível buscar os materiais: {exception.Message}");
        }
    }

    public async Task<Resultado<IReadOnlyList<ItemEstoque>>> ObterMatrizAsync()
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            IReadOnlyList<ItemEstoque> itens = await db.ItensEstoque
                .AsNoTracking()
                .Where(item => item.Ativo)
                .OrderBy(item => item.Prateleira)
                .ThenBy(item => item.Posicao)
                .ToListAsync();
            return Resultado<IReadOnlyList<ItemEstoque>>.Ok(itens);
        }
        catch (SqliteException exception)
        {
            return Resultado<IReadOnlyList<ItemEstoque>>.Falha(
                $"Não foi possível carregar a matriz do estoque: {exception.Message}");
        }
    }

    public async Task<Resultado<ItemEstoque>> ObterAsync(int id)
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var item = await db.ItensEstoque
                .AsNoTracking()
                .FirstOrDefaultAsync(material => material.Id == id && material.Ativo);
            return item is null
                ? Resultado<ItemEstoque>.Falha("Material não encontrado.")
                : Resultado<ItemEstoque>.Ok(item);
        }
        catch (SqliteException exception)
        {
            return Resultado<ItemEstoque>.Falha(
                $"Não foi possível carregar o material: {exception.Message}");
        }
    }

    public async Task<Resultado> SalvarAsync(ItemEstoque item)
    {
        var erro = Validar(item);
        if (erro is not null)
            return Resultado.Falha(erro);

        item.Codigo = item.Codigo.Trim();
        item.Nome = item.Nome.Trim();
        item.Descricao = item.Descricao.Trim();
        item.Categoria = item.Categoria.Trim();
        LocalizacaoValidator.TryNormalizar(
            item.Prateleira, item.Posicao, out var prateleira, out _);
        item.Prateleira = prateleira;

        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            if (await db.ItensEstoque.AnyAsync(material =>
                    material.Codigo == item.Codigo && material.Id != item.Id))
            {
                return Resultado.Falha("Código: já existe um material com este código.");
            }

            if (await db.ItensEstoque.AnyAsync(material =>
                    material.Ativo
                    && material.Prateleira == item.Prateleira
                    && material.Posicao == item.Posicao
                    && material.Id != item.Id))
            {
                return Resultado.Falha(
                    $"Localização {item.Prateleira}{item.Posicao}: já está ocupada por outro material.");
            }

            if (item.Id == 0)
            {
                db.ItensEstoque.Add(item);
            }
            else
            {
                var existente = await db.ItensEstoque.FirstOrDefaultAsync(material =>
                    material.Id == item.Id && material.Ativo);
                if (existente is null)
                    return Resultado.Falha("O material que você tentou atualizar não foi encontrado.");

                existente.Codigo = item.Codigo;
                existente.Nome = item.Nome;
                existente.Descricao = item.Descricao;
                existente.Categoria = item.Categoria;
                existente.Quantidade = item.Quantidade;
                existente.PrecoUnitario = item.PrecoUnitario;
                existente.Prateleira = item.Prateleira;
                existente.Posicao = item.Posicao;
            }

            await db.SaveChangesAsync();
            return Resultado.Ok();
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqliteException
                  {
                      SqliteExtendedErrorCode: 2067
                  })
        {
            return await ObterMensagemConflitoAsync(item);
        }
        catch (DbUpdateException exception)
        {
            var detalhe = exception.InnerException?.Message ?? exception.Message;
            return Resultado.Falha($"Não foi possível salvar o material: {detalhe}");
        }
        catch (SqliteException exception)
        {
            return Resultado.Falha($"Não foi possível salvar o material: {exception.Message}");
        }
    }

    public async Task<Resultado> DesativarAsync(int id)
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var item = await db.ItensEstoque.FirstOrDefaultAsync(material =>
                material.Id == id && material.Ativo);
            if (item is null)
                return Resultado.Falha("O material selecionado não foi encontrado.");

            item.Ativo = false;
            await db.SaveChangesAsync();
            return Resultado.Ok("Material removido do estoque.");
        }
        catch (DbUpdateException exception)
        {
            var detalhe = exception.InnerException?.Message ?? exception.Message;
            return Resultado.Falha($"Não foi possível remover o material: {detalhe}");
        }
        catch (SqliteException exception)
        {
            return Resultado.Falha($"Não foi possível remover o material: {exception.Message}");
        }
    }

    private async Task<Resultado> ObterMensagemConflitoAsync(ItemEstoque item)
    {
        await using var db = await _factory.CreateDbContextAsync();
        if (await db.ItensEstoque.AnyAsync(material =>
                material.Codigo == item.Codigo && material.Id != item.Id))
        {
            return Resultado.Falha("Código: já existe um material com este código.");
        }

        return Resultado.Falha(
            $"Localização {item.Prateleira}{item.Posicao}: já está ocupada por outro material.");
    }

    private static string? Validar(ItemEstoque item)
    {
        if (string.IsNullOrWhiteSpace(item.Codigo))
            return "Código: informe o código do material.";
        if (item.Codigo.Trim().Length > 40)
            return "Código: use no máximo 40 caracteres.";
        if (string.IsNullOrWhiteSpace(item.Nome))
            return "Nome: informe o nome do material.";
        if (item.Nome.Trim().Length > 150)
            return "Nome: use no máximo 150 caracteres.";
        if (item.Descricao.Trim().Length > 500)
            return "Descrição: use no máximo 500 caracteres.";
        if (string.IsNullOrWhiteSpace(item.Categoria))
            return "Categoria: informe a categoria do material.";
        if (item.Categoria.Trim().Length > 80)
            return "Categoria: use no máximo 80 caracteres.";
        if (item.Quantidade < 0)
            return "Quantidade: informe um valor igual ou maior que zero.";
        if (item.PrecoUnitario is < 0 or > 99999999.99m)
            return "Preço unitário: informe um valor entre R$ 0,00 e R$ 99.999.999,99.";
        if (!LocalizacaoValidator.TryNormalizar(
                item.Prateleira, item.Posicao, out _, out var erro))
        {
            return erro;
        }

        return null;
    }

    private static string EscaparLike(string valor) =>
        valor.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}
