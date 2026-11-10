using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FixMaster.Services.Interfaces;

namespace FixMaster.ViewModels;

public partial class CadastroUsuarioViewModel : ViewModelBase
{
    private readonly IAuthService _auth;
    private readonly Action _aoFechar;

    [ObservableProperty]
    private string _usuarioAdministrador = "";

    [ObservableProperty]
    private string _senhaAdministrador = "";

    [ObservableProperty]
    private string _usuario = "";

    [ObservableProperty]
    private string _senha = "";

    [ObservableProperty]
    private string _confirmarSenha = "";

    [ObservableProperty]
    private bool _novoUsuarioAdministrador;

    [ObservableProperty]
    private string? _mensagemErro;

    [ObservableProperty]
    private string? _mensagemSucesso;

    public CadastroUsuarioViewModel(IAuthService auth, Action aoFechar)
    {
        _auth = auth;
        _aoFechar = aoFechar;
    }

    [RelayCommand]
    private async Task CriarAsync()
    {
        MensagemErro = null;
        MensagemSucesso = null;

        if (string.IsNullOrWhiteSpace(UsuarioAdministrador)
            || string.IsNullOrEmpty(SenhaAdministrador))
        {
            MensagemErro = "Informe as credenciais do administrador.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Usuario)
            || string.IsNullOrEmpty(Senha)
            || string.IsNullOrEmpty(ConfirmarSenha))
        {
            MensagemErro = "Preencha todos os dados do novo usuário.";
            return;
        }

        if (Senha != ConfirmarSenha)
        {
            MensagemErro = "As senhas não conferem.";
            return;
        }

        var resultado = await _auth.CriarUsuarioAsync(
            UsuarioAdministrador, SenhaAdministrador, Usuario, Senha,
            NovoUsuarioAdministrador);
        if (resultado.Sucesso)
        {
            MensagemSucesso = NovoUsuarioAdministrador
                ? "Administrador cadastrado com sucesso."
                : "Usuário cadastrado com sucesso.";
            Usuario = "";
            Senha = "";
            ConfirmarSenha = "";
            return;
        }

        MensagemErro = resultado.Mensagem;
    }

    [RelayCommand]
    private void Cancelar() => _aoFechar();
}
