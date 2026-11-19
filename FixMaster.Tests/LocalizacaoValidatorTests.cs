using FixMaster.Services;
using Xunit;

namespace FixMaster.Tests;

public sealed class LocalizacaoValidatorTests
{
    [Theory]
    [InlineData("A", 1, "A")]
    [InlineData("G", 8, "G")]
    [InlineData("g", 4, "G")]
    public void TryNormalizar_AceitaLimitesENormaliza(
        string prateleira,
        int posicao,
        string esperado)
    {
        var valido = LocalizacaoValidator.TryNormalizar(
            prateleira, posicao, out var normalizada, out var erro);

        Assert.True(valido, erro);
        Assert.Equal(esperado, normalizada);
        Assert.Null(erro);
    }

    [Theory]
    [InlineData("Y", 1)]
    [InlineData("H", 1)]
    [InlineData("X", 1)]
    [InlineData("0", 1)]
    [InlineData("", 1)]
    [InlineData("AB", 1)]
    [InlineData("A", 0)]
    [InlineData("A", 9)]
    public void TryNormalizar_RejeitaCoordenadaForaDoDeposito(
        string prateleira,
        int posicao)
    {
        var valido = LocalizacaoValidator.TryNormalizar(
            prateleira, posicao, out _, out var erro);

        Assert.False(valido);
        Assert.False(string.IsNullOrWhiteSpace(erro));
    }
}
