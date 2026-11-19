using System;
using System.Data.Common;
using System.Threading.Tasks;
using FixMaster.Data;
using FixMaster.Infrastructure;
using FixMaster.Models;
using FixMaster.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FixMaster.Services;

public sealed class AtividadeService : IAtividadeService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public AtividadeService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Resultado> RegistrarAsync(string tipo, string descricao, int? referenciaId = null)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            return Resultado.Falha("Informe o tipo da atividade.");

        if (string.IsNullOrWhiteSpace(descricao))
            return Resultado.Falha("Informe a descrição da atividade.");

        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            db.AtividadesRecentes.Add(new AtividadeRecente
            {
                DataHora = DateTime.Now,
                Tipo = tipo.Trim(),
                Descricao = descricao.Trim(),
                ReferenciaId = referenciaId
            });

            await db.SaveChangesAsync();
            return Resultado.Ok();
        }
        catch (Exception exception) when (exception is DbUpdateException or DbException or InvalidOperationException)
        {
            return Resultado.Falha(
                $"Não foi possível registrar a atividade: {exception.GetBaseException().Message}");
        }
    }
}
