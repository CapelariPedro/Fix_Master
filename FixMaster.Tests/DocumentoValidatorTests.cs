using FixMaster.Services;
using Xunit;

namespace FixMaster.Tests;

public sealed class DocumentoValidatorTests
{
    [Theory]
    [InlineData("529.982.247-25", false)]
    [InlineData("11.222.333/0001-81", true)]
    [InlineData("12.ABC.345/01DE-35", true)]
    [InlineData("12.abc.345/01de-35", true)]
    public void EhValido_AceitaDocumentosValidos(string documento, bool ehEmpresa)
    {
        Assert.True(DocumentoValidator.EhValido(documento, ehEmpresa));
    }

    [Theory]
    [InlineData("111.111.111-11", false)]
    [InlineData("529.982.247-24", false)]
    [InlineData("11.222.333/0001-80", true)]
    [InlineData("12.ABC.345/01DE-36", true)]
    [InlineData("12.ABC.345/01DE-A5", true)]
    [InlineData("abc529.982.247-25", false)]
    [InlineData("529.982.247-25", true)]
    [InlineData("11.222.333/0001-81", false)]
    [InlineData("12.ABC.345/01DE-35", false)]
    public void EhValido_RejeitaDocumentoInvalidoOuDeTipoIncorreto(
        string documento,
        bool ehEmpresa)
    {
        Assert.False(DocumentoValidator.EhValido(documento, ehEmpresa));
    }

    [Theory]
    [InlineData("52998224725", false, "529.982.247-25")]
    [InlineData("11222333000181", true, "11.222.333/0001-81")]
    [InlineData("12abc34501de35", true, "12.ABC.345/01DE-35")]
    public void Formatar_AplicaMascaraDoTipoDePessoa(
        string documento,
        bool ehEmpresa,
        string esperado)
    {
        Assert.Equal(esperado, DocumentoValidator.Formatar(documento, ehEmpresa));
    }

    [Theory]
    [InlineData("52998224725", false, "529.982.247-25")]
    [InlineData("5299", false, "529.9")]
    [InlineData("11222333000181", true, "11.222.333/0001-81")]
    [InlineData("112223", true, "11.222.3")]
    [InlineData("52998224725999", false, "529.982.247-25")]
    [InlineData("11222333000181999", true, "11.222.333/0001-81")]
    [InlineData("12abc34501de35", true, "12.ABC.345/01DE-35")]
    [InlineData("12abc34501de35999", true, "12.ABC.345/01DE-35")]
    public void FormatarEnquantoDigita_FormataEAplicaLimiteDeDigitos(
        string documento,
        bool ehEmpresa,
        string esperado)
    {
        Assert.Equal(esperado, DocumentoValidator.FormatarEnquantoDigita(documento, ehEmpresa));
    }

    [Theory]
    [InlineData(false, 11, 14)]
    [InlineData(true, 14, 18)]
    public void LimitesCorrespondemAoTipoDeDocumento(
        bool ehEmpresa,
        int limiteDigitos,
        int limiteFormatado)
    {
        Assert.Equal(limiteDigitos, DocumentoValidator.LimiteDigitos(ehEmpresa));
        Assert.Equal(limiteFormatado, DocumentoValidator.LimiteCaracteresFormatados(ehEmpresa));
    }

    [Fact]
    public void NormalizarDocumento_ConverteCnpjAlfanumericoParaMaiusculas()
    {
        Assert.Equal("12ABC34501DE35",
            DocumentoValidator.NormalizarDocumento("12.abc.345/01de-35", true));
    }
}
