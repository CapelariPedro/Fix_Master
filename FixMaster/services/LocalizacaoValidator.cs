namespace FixMaster.Services;

public static class LocalizacaoValidator
{
    public static bool TryNormalizar(
        string? prateleira,
        int posicao,
        out string prateleiraNormalizada,
        out string? erro)
    {
        prateleiraNormalizada = (prateleira ?? "").Trim().ToUpperInvariant();
        erro = null;

        if (prateleiraNormalizada.Length != 1
            || prateleiraNormalizada[0] is < 'A' or > 'G')
        {
            erro = "Prateleira: informe uma letra de A a G.";
            return false;
        }

        if (posicao is < 1 or > 8)
        {
            erro = "Posição: informe um número de 1 a 8.";
            return false;
        }

        return true;
    }
}
