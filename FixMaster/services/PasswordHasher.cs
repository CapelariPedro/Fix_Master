using System;
using System.Security.Cryptography;

namespace FixMaster.Services;

public static class PasswordHasher
{
    private const string Algoritmo = "PBKDF2-SHA256";
    private const int Iteracoes = 600_000;
    private const int TamanhoSalt = 16;
    private const int TamanhoHash = 32;

    public static string Hash(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);
        return $"{Algoritmo}${Iteracoes}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string senha, string hashArmazenado)
    {
        try
        {
            var partes = hashArmazenado.Split('$');
            if (partes.Length != 4 || partes[0] != Algoritmo) return false;

            var iteracoes = int.Parse(partes[1]);
            var salt = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);

            var calculado = Rfc2898DeriveBytes.Pbkdf2(
                senha, salt, iteracoes, HashAlgorithmName.SHA256, esperado.Length);

            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException) { return false; }
        catch (OverflowException) { return false; }
    }
}
