namespace FixMaster.Models;

public sealed class ItemOS
{
    public int Id { get; set; }
    public int OrdemServicoId { get; set; }
    public int ItemEstoqueId { get; set; }
    public int QuantidadeUtilizada { get; set; }
    public decimal ValorUnitario { get; set; }
    public OrdemServico? OrdemServico { get; set; }
    public ItemEstoque? ItemEstoque { get; set; }
    public decimal Subtotal => QuantidadeUtilizada * ValorUnitario;
}
