using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FixMaster.Infrastructure;
using FixMaster.Services.Interfaces;

namespace FixMaster.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _auth;
    private readonly Func<UsuarioAutenticado, Task> _aoAutenticar;

    [ObservableProperty]
    private string _usuario = "";

    [ObservableProperty]
    private string _senha = "";

    [ObservableProperty]
    private string? _mensagemErro;

    public LoginViewModel(IAuthService auth, Func<UsuarioAutenticado, Task> aoAutenticar)
    {
        _auth = auth;
        _aoAutenticar = aoAutenticar;
    }

    [RelayCommand]
    private async Task EntrarAsync()
    {
        MensagemErro = null;
        var usuarioAutenticado = await _auth.AutenticarAsync(Usuario, Senha);
        if (usuarioAutenticado is not null)
        {
            Senha = "";
            await _aoAutenticar(usuarioAutenticado);
            return;
        }

        MensagemErro = "Usuário ou senha inválidos.";
    }
}
