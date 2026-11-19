namespace FixMaster.Services;

public static class CepFormatter
{
    public static string Normalizar(string? cep)
    {
        if (string.IsNullOrEmpty(cep))
            return "";

        var digitos = new char[cep.Length];
        var quantidade = 0;
        foreach (var caractere in cep)
        {
            if (caractere is >= '0' and <= '9')
                digitos[quantidade++] = caractere;
        }

        return new string(digitos, 0, quantidade);
    }

    public static string FormatarEnquantoDigita(string? cep)
    {
        var digitos = Normalizar(cep);
        if (digitos.Length > 8)
            digitos = digitos[..8];

        return digitos.Length > 5
            ? $"{digitos[..5]}-{digitos[5..]}"
            : digitos;
    }

    public static string Formatar(string? cep)
    {
        var digitos = Normalizar(cep);
        return digitos.Length == 8 ? $"{digitos[..5]}-{digitos[5..]}" : digitos;
    }

    public static bool EhValido(string? cep)
    {
        if (string.IsNullOrWhiteSpace(cep))
            return true;

        if (cep.Length == 9 && cep[5] != '-')
            return false;
        if (cep.Length is not 8 and not 9)
            return false;

        for (var indice = 0; indice < cep.Length; indice++)
        {
            if (indice == 5 && cep.Length == 9)
                continue;
            if (cep[indice] is not (>= '0' and <= '9'))
                return false;
        }

        return Normalizar(cep).Length == 8;
    }

    public static int ContarDigitos(string? cep) => Normalizar(cep).Length;
}
