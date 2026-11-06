using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FixMaster.Services.Interfaces;

namespace FixMaster.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _auth;
    private readonly Action _aoAutenticar;

    [ObservableProperty]
    private string _usuario = "";

    [ObservableProperty]
    private string _senha = "";

    [ObservableProperty]
    private string? _mensagemErro;

    public LoginViewModel(IAuthService auth, Action aoAutenticar)
    {
        _auth = auth;
        _aoAutenticar = aoAutenticar;
    }

    [RelayCommand]
    private async Task EntrarAsync()
    {
        MensagemErro = null;
        if (await _auth.AutenticarAsync(Usuario, Senha))
        {
            Senha = "";
            _aoAutenticar();
            return;
        }

        MensagemErro = "Usuário ou senha inválidos.";
    }
}
