namespace FixMaster.Models;

public sealed class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Documento { get; set; } = "";
    public string Cep { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string Endereco { get; set; } = "";
    public bool EhEmpresa { get; set; }
    public bool Ativo { get; set; } = true;
}
