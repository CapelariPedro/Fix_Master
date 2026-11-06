namespace FixMaster.Infrastructure;

public record Resultado(bool Sucesso, string? Mensagem = null)
{
    public static Resultado Ok() => new(true);
    public static Resultado Falha(string mensagem) => new(false, mensagem);
}
