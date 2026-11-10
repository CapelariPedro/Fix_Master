namespace FixMaster.Infrastructure;

public record Resultado(bool Sucesso, string? Mensagem = null)
{
    public static Resultado Ok() => new(true);
    public static Resultado Falha(string mensagem) => new(false, mensagem);
}

public sealed record Resultado<T>(bool Sucesso, T? Valor = default, string? Mensagem = null)
{
    public static Resultado<T> Ok(T valor) => new(true, valor);
    public static Resultado<T> Falha(string mensagem) => new(false, default, mensagem);
}
