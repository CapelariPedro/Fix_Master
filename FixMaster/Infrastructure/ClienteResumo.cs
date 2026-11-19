namespace FixMaster.Infrastructure;

public sealed record ClienteResumo(
    int Id,
    string Nome,
    string Documento,
    string DocumentoFormatado,
    string CepFormatado,
    string Telefone,
    string Endereco,
    bool EhEmpresa)
{
    public string Perfil => EhEmpresa ? "EMPRESA" : "PESSOA";
    public bool EstaSelecionado { get; init; }
}
