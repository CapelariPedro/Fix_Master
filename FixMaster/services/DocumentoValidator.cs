using System;
using System.Linq;

namespace FixMaster.Services;

public static class DocumentoValidator
{
    public static int LimiteDigitos(bool ehEmpresa) => ehEmpresa ? 14 : 11;

    public static int LimiteCaracteresFormatados(bool ehEmpresa) => ehEmpresa ? 18 : 14;

    public static string NormalizarDocumento(string? documento, bool ehEmpresa)
    {
        if (string.IsNullOrEmpty(documento))
            return "";

        var caracteres = new char[documento.Length];
        var quantidade = 0;
        foreach (var caractere in documento)
        {
            if (caractere is >= '0' and <= '9')
                caracteres[quantidade++] = caractere;
            else if (ehEmpresa && caractere is >= 'a' and <= 'z')
                caracteres[quantidade++] = (char)(caractere - ('a' - 'A'));
            else if (ehEmpresa && caractere is >= 'A' and <= 'Z')
                caracteres[quantidade++] = caractere;
        }

        return new string(caracteres, 0, quantidade);
    }

    public static string SomenteDigitos(string? documento)
    {
        if (string.IsNullOrEmpty(documento))
            return "";

        var digitos = new char[documento.Length];
        var quantidade = 0;
        foreach (var caractere in documento)
        {
            if (caractere is >= '0' and <= '9')
                digitos[quantidade++] = caractere;
        }

        return new string(digitos, 0, quantidade);
    }

    public static int ContarCaracteresDocumento(string? documento, bool ehEmpresa) =>
        NormalizarDocumento(documento, ehEmpresa).Length;

    public static bool EhValido(string? documento, bool ehEmpresa)
    {
        if (documento is null || !documento.All(caractere =>
                caractere is >= '0' and <= '9'
                or >= 'a' and <= 'z'
                or >= 'A' and <= 'Z'
                or '.' or '-' or '/' or ' ' or '\t'))
        {
            return false;
        }

        var normalizado = NormalizarDocumento(documento, ehEmpresa);
        if (!ehEmpresa)
            return !documento.Any(caractere => caractere is >= 'a' and <= 'z' or >= 'A' and <= 'Z')
                && ValidarCpf(normalizado);

        return ValidarCnpj(normalizado);
    }

    public static string Formatar(string documento, bool ehEmpresa)
    {
        return FormatarEnquantoDigita(documento, ehEmpresa);
    }

    public static string FormatarEnquantoDigita(string? documento, bool ehEmpresa)
    {
        var caracteresDocumento = NormalizarDocumento(documento, ehEmpresa);
        var limite = LimiteDigitos(ehEmpresa);
        if (caracteresDocumento.Length > limite)
            caracteresDocumento = caracteresDocumento[..limite];

        var resultado = new System.Text.StringBuilder(LimiteCaracteresFormatados(ehEmpresa));
        if (ehEmpresa)
        {
            AdicionarGrupo(resultado, caracteresDocumento, 0, 2);
            AdicionarSeparadorSeNecessario(resultado, caracteresDocumento.Length, 2, '.');
            AdicionarGrupo(resultado, caracteresDocumento, 2, 3);
            AdicionarSeparadorSeNecessario(resultado, caracteresDocumento.Length, 5, '.');
            AdicionarGrupo(resultado, caracteresDocumento, 5, 3);
            AdicionarSeparadorSeNecessario(resultado, caracteresDocumento.Length, 8, '/');
            AdicionarGrupo(resultado, caracteresDocumento, 8, 4);
            AdicionarSeparadorSeNecessario(resultado, caracteresDocumento.Length, 12, '-');
            AdicionarGrupo(resultado, caracteresDocumento, 12, 2);
        }
        else
        {
            AdicionarGrupo(resultado, caracteresDocumento, 0, 3);
            AdicionarSeparadorSeNecessario(resultado, caracteresDocumento.Length, 3, '.');
            AdicionarGrupo(resultado, caracteresDocumento, 3, 3);
            AdicionarSeparadorSeNecessario(resultado, caracteresDocumento.Length, 6, '.');
            AdicionarGrupo(resultado, caracteresDocumento, 6, 3);
            AdicionarSeparadorSeNecessario(resultado, caracteresDocumento.Length, 9, '-');
            AdicionarGrupo(resultado, caracteresDocumento, 9, 2);
        }

        return resultado.ToString();
    }

    private static void AdicionarGrupo(
        System.Text.StringBuilder resultado,
        string digitos,
        int inicio,
        int tamanho)
    {
        if (digitos.Length <= inicio)
            return;

        resultado.Append(digitos, inicio, Math.Min(tamanho, digitos.Length - inicio));
    }

    private static void AdicionarSeparadorSeNecessario(
        System.Text.StringBuilder resultado,
        int quantidadeDigitos,
        int limiteGrupo,
        char separador)
    {
        if (quantidadeDigitos > limiteGrupo)
            resultado.Append(separador);
    }

    private static bool ValidarCpf(string documento)
    {
        if (documento.Length != 11 || TodosIguais(documento))
            return false;

        var primeiroDigito = CalcularDigito(documento.AsSpan(0, 9), 10);
        if (documento[9] - '0' != primeiroDigito)
            return false;

        var segundoDigito = CalcularDigito(documento.AsSpan(0, 10), 11);
        return documento[10] - '0' == segundoDigito;
    }

    private static bool ValidarCnpj(string documento)
    {
        if (documento.Length != 14
            || documento.Any(caractere => caractere is not (>= '0' and <= '9')
                and not (>= 'A' and <= 'Z'))
            || documento[12] is < '0' or > '9'
            || documento[13] is < '0' or > '9'
            || TodosIguais(documento))
            return false;

        ReadOnlySpan<int> pesosPrimeiroDigito = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        ReadOnlySpan<int> pesosSegundoDigito = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var primeiroDigito = CalcularDigito(documento.AsSpan(0, 12), pesosPrimeiroDigito);
        if (documento[12] - '0' != primeiroDigito)
            return false;

        var segundoDigito = CalcularDigito(documento.AsSpan(0, 13), pesosSegundoDigito);
        return documento[13] - '0' == segundoDigito;
    }

    private static int CalcularDigito(ReadOnlySpan<char> documento, int pesoInicial)
    {
        var soma = 0;
        for (var indice = 0; indice < documento.Length; indice++)
            soma += (documento[indice] - '0') * (pesoInicial - indice);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static int CalcularDigito(ReadOnlySpan<char> documento, ReadOnlySpan<int> pesos)
    {
        var soma = 0;
        for (var indice = 0; indice < documento.Length; indice++)
            soma += ValorCnpj(documento[indice]) * pesos[indice];

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static int ValorCnpj(char caractere) => caractere - '0';

    private static bool TodosIguais(string documento)
    {
        for (var indice = 1; indice < documento.Length; indice++)
        {
            if (documento[indice] != documento[0])
                return false;
        }

        return true;
    }
}
