using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using FixMaster.Infrastructure;
using FixMaster.Services.Interfaces;

namespace FixMaster.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    private readonly IAuthService _auth;
    private readonly IClienteService _clienteService;
    private readonly IEstoqueService _estoqueService;
    private object _currentScreen;
    private bool _isAdministrador;
    private bool _isPainelAutenticado;
    private string _usuarioLogadoNome = "";
    private string _perfilLogado = "";
    private bool _isCadastroUsuarioAberto;
    private CadastroUsuarioViewModel? _cadastroUsuario;

    public MainViewModel(
        IAuthService auth,
        IClienteService clienteService,
        IEstoqueService estoqueService)
    {
        _auth = auth;
        _clienteService = clienteService;
        _estoqueService = estoqueService;
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

    public bool IsPainelAutenticado
    {
        get => _isPainelAutenticado;
        private set => SetProperty(ref _isPainelAutenticado, value);
    }

    public string UsuarioLogadoNome
    {
        get => _usuarioLogadoNome;
        private set => SetProperty(ref _usuarioLogadoNome, value);
    }

    public string PerfilLogado
    {
        get => _perfilLogado;
        private set => SetProperty(ref _perfilLogado, value);
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
        if (!IsAdministrador)
            return;

        CadastroUsuario = new CadastroUsuarioViewModel(
            _auth, () => IsCadastroUsuarioAberto = false);
        IsCadastroUsuarioAberto = true;
    }

    [RelayCommand]
    private void AbrirInicio() =>
        (CurrentScreen as ShellViewModel)?.MostrarInicio();

    [RelayCommand]
    private void AbrirClientes() =>
        (CurrentScreen as ShellViewModel)?.MostrarClientes();

    [RelayCommand]
    private void AbrirEstoque() =>
        (CurrentScreen as ShellViewModel)?.MostrarEstoque();

    [RelayCommand]
    private void AbrirUsuarios()
    {
        if (IsAdministrador)
            (CurrentScreen as ShellViewModel)?.MostrarUsuarios();
    }

    private async Task AbrirTelaPrincipal(UsuarioAutenticado usuario)
    {
        IsAdministrador = usuario.EhAdministrador;
        UsuarioLogadoNome = usuario.Username;
        PerfilLogado = usuario.EhAdministrador ? "ADMINISTRADOR" : "OPERADOR";
        var shell = new ShellViewModel(_auth, _clienteService, _estoqueService, usuario);
        await shell.InicializarAsync();
        CurrentScreen = shell;
        IsPainelAutenticado = true;
    }

    private LoginViewModel CriarTelaLogin() =>
        new(_auth, AbrirTelaPrincipal);
}
