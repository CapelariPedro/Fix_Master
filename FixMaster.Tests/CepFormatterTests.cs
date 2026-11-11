using FixMaster.Services;
using Xunit;

namespace FixMaster.Tests;

public sealed class CepFormatterTests
{
    [Theory]
    [InlineData("12345678", "12345-678")]
    [InlineData("123456789", "12345-678")]
    [InlineData("1234", "1234")]
    [InlineData("123456", "12345-6")]
    public void FormatarEnquantoDigita_AplicaMascaraELimite(string entrada, string esperado)
    {
        Assert.Equal(esperado, CepFormatter.FormatarEnquantoDigita(entrada));
    }

    [Theory]
    [InlineData("12345678", true)]
    [InlineData("12345-678", true)]
    [InlineData("", true)]
    [InlineData("1234", false)]
    [InlineData("12345-67", false)]
    [InlineData("12.345-678", false)]
    [InlineData("1234-5678", false)]
    public void EhValido_ValidaCepCompletoOuVazio(string cep, bool esperado)
    {
        Assert.Equal(esperado, CepFormatter.EhValido(cep));
    }
}
