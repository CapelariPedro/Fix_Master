using System.Collections.Generic;
using System.Threading.Tasks;
using FixMaster.Infrastructure;
using FixMaster.Models;

namespace FixMaster.Services.Interfaces;

public interface IEstoqueService
{
    Task<Resultado<string>> GerarCodigoAsync(string categoria, string? tipoInsumo = null);
    Task<Resultado<IReadOnlyList<ItemEstoque>>> BuscarAsync(string? termo);
    Task<Resultado<IReadOnlyList<ItemEstoque>>> ObterMatrizAsync();
    Task<Resultado<ItemEstoque>> ObterAsync(int id);
    Task<Resultado> SalvarAsync(ItemEstoque item);
    Task<Resultado> DesativarAsync(int id);
}
