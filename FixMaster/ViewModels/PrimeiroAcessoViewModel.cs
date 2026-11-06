using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FixMaster.Services.Interfaces;

namespace FixMaster.ViewModels;

public partial class PrimeiroAcessoViewModel : ViewModelBase
{
    private readonly IAuthService _auth;
    private readonly Action _aoCriar;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CriarCommand))]
    private string _usuario = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CriarCommand))]
    private string _senha = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CriarCommand))]
    private string _confirmarSenha = "";

    [ObservableProperty]
    private string? _mensagemErro;

    public PrimeiroAcessoViewModel(IAuthService auth, Action aoCriar)
    {
        _auth = auth;
        _aoCriar = aoCriar;
    }

    private bool PodeCriar()
        => !string.IsNullOrWhiteSpace(Usuario)
        && !string.IsNullOrEmpty(Senha)
        && !string.IsNullOrEmpty(ConfirmarSenha);

    [RelayCommand(CanExecute = nameof(PodeCriar))]
    private async Task CriarAsync()
    {
        MensagemErro = null;

        if (Senha != ConfirmarSenha)
        {
            MensagemErro = "As senhas não conferem.";
            return;
        }

        var resultado = await _auth.CriarOperadorAsync(Usuario, Senha);
        if (resultado.Sucesso)
        {
            Senha = "";
            ConfirmarSenha = "";
            _aoCriar();
        }
        else
        {
            MensagemErro = resultado.Mensagem;
        }
    }
}
