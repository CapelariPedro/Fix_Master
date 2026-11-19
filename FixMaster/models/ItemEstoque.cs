namespace FixMaster.Models;

public sealed class ItemEstoque
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Descricao { get; set; } = "";
    public string Categoria { get; set; } = "";
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public string Prateleira { get; set; } = "";
    public int Posicao { get; set; }
    public bool Ativo { get; set; } = true;
}
