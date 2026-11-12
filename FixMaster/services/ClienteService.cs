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

public sealed class ClienteService : IClienteService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ClienteService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Resultado<IReadOnlyList<ClienteResumo>>> ListarAsync(string? filtro)
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var consulta = db.Clientes.AsNoTracking().Where(cliente => cliente.Ativo);
            filtro = filtro?.Trim();

            var clientes = await consulta
                .OrderBy(cliente => cliente.Nome)
                .ThenBy(cliente => cliente.Id)
                .Select(cliente => new
                {
                    cliente.Id,
                    cliente.Nome,
                    cliente.Documento,
                    cliente.Cep,
                    cliente.Telefone,
                    cliente.Endereco,
                    cliente.EhEmpresa
                })
                .ToListAsync();

            if (!string.IsNullOrEmpty(filtro))
            {
                var documento = DocumentoValidator.NormalizarDocumento(filtro, ehEmpresa: true);
                var cep = CepFormatter.Normalizar(filtro);
                var podeBuscarCep = filtro.All(caractere =>
                    char.IsDigit(caractere) || caractere is '-' or ' ' or '.');

                clientes = clientes
                    .Where(cliente =>
                        TextoHelper.Contem(cliente.Nome, filtro)
                        || TextoHelper.Contem(cliente.Telefone, filtro)
                        || (podeBuscarCep && cep.Length > 0 && TextoHelper.Contem(cliente.Cep, cep))
                        || (documento.Length > 0 && TextoHelper.Contem(cliente.Documento, documento)))
                    .ToList();
            }

            IReadOnlyList<ClienteResumo> resultado = clientes
                .Select(cliente => CriarResumo(
                    cliente.Id, cliente.Nome, cliente.Documento, cliente.Cep,
                    cliente.Telefone, cliente.Endereco, cliente.EhEmpresa))
                .ToList();
            return Resultado<IReadOnlyList<ClienteResumo>>.Ok(resultado);
        }
        catch (SqliteException exception)
        {
            return Resultado<IReadOnlyList<ClienteResumo>>.Falha(
                $"Não foi possível carregar os clientes: {exception.Message}");
        }
    }

    public async Task<Resultado<ClienteResumo>> ObterAsync(int id)
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var cliente = await db.Clientes
                .AsNoTracking()
                .Where(item => item.Id == id && item.Ativo)
                .Select(item => new
                {
                    item.Id,
                    item.Nome,
                    item.Documento,
                    item.Cep,
                    item.Telefone,
                    item.Endereco,
                    item.EhEmpresa
                })
                .FirstOrDefaultAsync();

            return cliente is null
                ? Resultado<ClienteResumo>.Falha("Cliente não encontrado.")
                : Resultado<ClienteResumo>.Ok(CriarResumo(
                    cliente.Id, cliente.Nome, cliente.Documento, cliente.Cep,
                    cliente.Telefone, cliente.Endereco, cliente.EhEmpresa));
        }
        catch (SqliteException exception)
        {
            return Resultado<ClienteResumo>.Falha(
                $"Não foi possível carregar o cliente: {exception.Message}");
        }
    }

    public async Task<Resultado> SalvarAsync(Cliente cliente)
    {
        var validacao = Validar(cliente);
        if (validacao is not null)
            return Resultado.Falha(validacao);

        cliente.Nome = cliente.Nome.Trim();
        cliente.Documento = DocumentoValidator.NormalizarDocumento(
            cliente.Documento, cliente.EhEmpresa);
        cliente.Cep = CepFormatter.Normalizar(cliente.Cep);
        cliente.Telefone = cliente.Telefone.Trim();
        cliente.Endereco = cliente.Endereco.Trim();

        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var documentoEmUso = await db.Clientes.AnyAsync(item =>
                item.Documento == cliente.Documento && item.Id != cliente.Id);
            if (documentoEmUso)
                return Resultado.Falha("Documento: já existe um cliente cadastrado com este CPF ou CNPJ.");

            if (cliente.Id == 0)
            {
                db.Clientes.Add(cliente);
            }
            else
            {
                var existente = await db.Clientes.FirstOrDefaultAsync(item => item.Id == cliente.Id);
                if (existente is null || !existente.Ativo)
                    return Resultado.Falha("O cliente que você tentou atualizar não foi encontrado.");

                existente.Nome = cliente.Nome;
                existente.Documento = cliente.Documento;
                existente.Cep = cliente.Cep;
                existente.Telefone = cliente.Telefone;
                existente.Endereco = cliente.Endereco;
                existente.EhEmpresa = cliente.EhEmpresa;
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
            return Resultado.Falha("Documento: já existe um cliente cadastrado com este CPF ou CNPJ.");
        }
        catch (DbUpdateException exception)
        {
            var detalhe = exception.InnerException?.Message ?? exception.Message;
            return Resultado.Falha($"Não foi possível salvar o cliente: {detalhe}");
        }
        catch (SqliteException exception)
        {
            return Resultado.Falha($"Não foi possível salvar o cliente: {exception.Message}");
        }
    }

    public async Task<Resultado> ExcluirOuInativarAsync(int id)
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var cliente = await db.Clientes.FirstOrDefaultAsync(item => item.Id == id && item.Ativo);
            if (cliente is null)
                return Resultado.Falha("O cliente selecionado não foi encontrado.");

            db.Clientes.Remove(cliente);
            await db.SaveChangesAsync();
            return Resultado.Ok("Cliente excluído com sucesso.");
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqliteException sqliteException
                  && sqliteException.SqliteErrorCode == 19
                  && sqliteException.Message.Contains(
                      "FOREIGN KEY constraint failed",
                      StringComparison.OrdinalIgnoreCase))
        {
            return await InativarClienteAsync(id);
        }
        catch (DbUpdateException exception)
        {
            var detalhe = exception.InnerException?.Message ?? exception.Message;
            return Resultado.Falha($"Não foi possível excluir o cliente: {detalhe}");
        }
        catch (SqliteException exception)
        {
            return Resultado.Falha($"Não foi possível excluir o cliente: {exception.Message}");
        }
    }

    private static string? Validar(Cliente cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.Nome))
            return "Nome: informe o nome do cliente.";
        if (cliente.Nome.Trim().Length > 150)
            return "Nome: use no máximo 150 caracteres.";
        if (!DocumentoValidator.EhValido(cliente.Documento, cliente.EhEmpresa))
        {
            var tipo = cliente.EhEmpresa ? "CNPJ" : "CPF";
            return $"Documento: informe um {tipo} válido.";
        }
        if (!CepFormatter.EhValido(cliente.Cep))
            return "CEP: informe os 8 números do CEP ou deixe o campo vazio.";
        if (string.IsNullOrWhiteSpace(cliente.Telefone))
            return "Telefone: informe um telefone para contato.";
        if (cliente.Telefone.Trim().Length > 30)
            return "Telefone: use no máximo 30 caracteres.";
        if (string.IsNullOrWhiteSpace(cliente.Endereco))
            return "Endereço: informe o endereço do cliente.";
        if (cliente.Endereco.Trim().Length > 250)
            return "Endereço: use no máximo 250 caracteres.";

        return null;
    }

    private static ClienteResumo CriarResumo(
        int id,
        string nome,
        string documento,
        string cep,
        string telefone,
        string endereco,
        bool ehEmpresa) =>
        new(id, nome, documento, DocumentoValidator.Formatar(documento, ehEmpresa),
            CepFormatter.Formatar(cep),
            telefone, endereco, ehEmpresa);

    private static string EscaparLike(string valor) =>
        valor.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);

    private async Task<Resultado> InativarClienteAsync(int id)
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var cliente = await db.Clientes.FirstOrDefaultAsync(item => item.Id == id && item.Ativo);
            if (cliente is null)
                return Resultado.Falha("O cliente selecionado não foi encontrado.");

            cliente.Ativo = false;
            await db.SaveChangesAsync();
            return Resultado.Ok(
                "Cliente inativado porque possui agendamento ou ordem de serviço vinculado.");
        }
        catch (DbUpdateException exception)
        {
            var detalhe = exception.InnerException?.Message ?? exception.Message;
            return Resultado.Falha($"Não foi possível inativar o cliente vinculado: {detalhe}");
        }
        catch (SqliteException exception)
        {
            return Resultado.Falha($"Não foi possível inativar o cliente vinculado: {exception.Message}");
        }
    }
}
