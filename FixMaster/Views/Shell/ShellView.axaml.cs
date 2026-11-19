using System;
using Avalonia.Controls;
using FixMaster.Services;
using FixMaster.ViewModels;

namespace FixMaster.Views.Shell;

public partial class ShellView : UserControl
{
    private bool _formatandoDocumento;
    private bool _formatandoCep;

    public ShellView()
    {
        InitializeComponent();
    }

    private void Documento_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_formatandoDocumento
            || sender is not TextBox caixaTexto
            || DataContext is not ShellViewModel viewModel)
        {
            return;
        }

        var cursorOriginal = Math.Clamp(caixaTexto.SelectionStart, 0, caixaTexto.Text?.Length ?? 0);
        var caracteresDocumentoAntesDoCursor = DocumentoValidator.ContarCaracteresDocumento(
            caixaTexto.Text?.Substring(0, cursorOriginal), viewModel.EhEmpresa);
        var textoFormatado = DocumentoValidator.FormatarEnquantoDigita(
            caixaTexto.Text, viewModel.EhEmpresa);

        if (caixaTexto.Text == textoFormatado)
            return;

        _formatandoDocumento = true;
        try
        {
            caixaTexto.Text = textoFormatado;
            caixaTexto.SelectionStart = PosicaoDoCursorDocumento(
                textoFormatado, caracteresDocumentoAntesDoCursor, viewModel.EhEmpresa);
            viewModel.Documento = textoFormatado;
        }
        finally
        {
            _formatandoDocumento = false;
        }
    }

    private void Cep_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_formatandoCep
            || sender is not TextBox caixaTexto
            || DataContext is not ShellViewModel viewModel)
        {
            return;
        }

        var cursorOriginal = Math.Clamp(caixaTexto.SelectionStart, 0, caixaTexto.Text?.Length ?? 0);
        var digitosAntesDoCursor = CepFormatter.Normalizar(
            caixaTexto.Text?.Substring(0, cursorOriginal)).Length;
        var textoFormatado = CepFormatter.FormatarEnquantoDigita(caixaTexto.Text);
        if (caixaTexto.Text == textoFormatado)
            return;

        _formatandoCep = true;
        try
        {
            caixaTexto.Text = textoFormatado;
            caixaTexto.SelectionStart = PosicaoDoCursorCep(textoFormatado, digitosAntesDoCursor);
            viewModel.Cep = textoFormatado;
        }
        finally
        {
            _formatandoCep = false;
        }
    }

    private static int PosicaoDoCursorCep(string texto, int quantidadeDigitos)
    {
        var posicao = 0;
        var encontrados = 0;
        while (posicao < texto.Length && encontrados < quantidadeDigitos)
        {
            if (texto[posicao] is >= '0' and <= '9')
                encontrados++;
            posicao++;
        }

        if (encontrados == quantidadeDigitos && posicao < texto.Length && texto[posicao] == '-')
            posicao++;

        return posicao;
    }

    private static int PosicaoDoCursorDocumento(
        string texto,
        int quantidadeCaracteresDocumento,
        bool ehEmpresa)
    {
        var posicao = 0;
        var encontrados = 0;
        while (posicao < texto.Length && encontrados < quantidadeCaracteresDocumento)
        {
            if (texto[posicao] is >= '0' and <= '9'
                || ehEmpresa && texto[posicao] is >= 'A' and <= 'Z')
            {
                encontrados++;
            }

            posicao++;
        }

        if (encontrados == quantidadeCaracteresDocumento
            && posicao < texto.Length
            && texto[posicao] is '.' or '/' or '-')
        {
            posicao++;
        }

        return posicao;
    }
}
