using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using FixMaster.Infrastructure;
using FixMaster.Services.Interfaces;

namespace FixMaster.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    private readonly IAuthService _auth;
    private object _currentScreen;
    private bool _isAdministrador;
    private bool _isCadastroUsuarioAberto;
    private CadastroUsuarioViewModel? _cadastroUsuario;

    public MainViewModel(IAuthService auth)
    {
        _auth = auth;
        _currentScreen = _auth.ExisteUsuario()
            ? CriarTelaLogin()
            : new PrimeiroAcessoViewModel(
                _auth, () => CurrentScreen = CriarTelaLogin());
    }

    public object CurrentScreen
    {
        get => _currentScreen;
        private set => SetProperty(ref _currentScreen, value);
    }

    public bool IsAdministrador
    {
        get => _isAdministrador;
        private set => SetProperty(ref _isAdministrador, value);
    }

    public bool IsCadastroUsuarioAberto
    {
        get => _isCadastroUsuarioAberto;
        private set => SetProperty(ref _isCadastroUsuarioAberto, value);
    }

    public CadastroUsuarioViewModel? CadastroUsuario
    {
        get => _cadastroUsuario;
        private set => SetProperty(ref _cadastroUsuario, value);
    }

    [RelayCommand]
    private void AbrirCadastroUsuario()
    {
        CadastroUsuario = new CadastroUsuarioViewModel(
            _auth, () => IsCadastroUsuarioAberto = false);
        IsCadastroUsuarioAberto = true;
    }

    private async Task AbrirTelaPrincipal(UsuarioAutenticado usuario)
    {
        IsAdministrador = usuario.EhAdministrador;
        var shell = new ShellViewModel(_auth, usuario);
        await shell.InicializarAsync();
        CurrentScreen = shell;
    }

    private LoginViewModel CriarTelaLogin() =>
        new(_auth, AbrirTelaPrincipal);
}
