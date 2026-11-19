using System;
using System.Globalization;
using System.Text;

namespace FixMaster.Services;

public static class TextoHelper
{
    public static string NormalizarBusca(string? valor)
    {
        if (string.IsNullOrEmpty(valor))
            return string.Empty;

        var normalizado = valor.Trim();
        var formaNormal = normalizado.Normalize(NormalizationForm.FormD);
        var resultado = new StringBuilder(formaNormal.Length);

        foreach (var caractere in formaNormal)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(caractere);
            if (categoria == UnicodeCategory.NonSpacingMark)
                continue;

            resultado.Append(char.ToLowerInvariant(caractere));
        }

        return resultado.ToString();
    }

    public static bool Contem(string? texto, string? termo)
    {
        if (string.IsNullOrEmpty(termo))
            return string.IsNullOrEmpty(texto) && string.IsNullOrEmpty(termo);

        if (string.IsNullOrEmpty(texto))
            return false;

        return NormalizarBusca(texto)
            .Contains(NormalizarBusca(termo), StringComparison.Ordinal);
    }
}
