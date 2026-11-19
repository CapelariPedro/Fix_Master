using System;
using System.ComponentModel;
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
    private readonly IAgendaService _agendaService;
    private readonly IOrdemServicoService _ordemServicoService;
    private readonly IDashboardService _dashboardService;
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
        IEstoqueService estoqueService,
        IAgendaService agendaService,
        IOrdemServicoService ordemServicoService,
        IDashboardService dashboardService)
    {
        _auth = auth;
        _clienteService = clienteService;
        _estoqueService = estoqueService;
        _agendaService = agendaService;
        _ordemServicoService = ordemServicoService;
        _dashboardService = dashboardService;
        _currentScreen = _auth.ExisteUsuario()
            ? CriarTelaLogin()
            : new PrimeiroAcessoViewModel(
                _auth, () => CurrentScreen = CriarTelaLogin());
    }

    public object CurrentScreen
    {
        get => _currentScreen;
        private set
        {
            var anterior = _currentScreen;
            if (!SetProperty(ref _currentScreen, value))
                return;

            if (anterior is ShellViewModel shellAnterior)
                shellAnterior.PropertyChanged -= OnShellPropertyChanged;

            if (value is ShellViewModel shellNovo)
                shellNovo.PropertyChanged += OnShellPropertyChanged;

            NotificarAbas();
        }
    }

    // Abas ativas (usadas para destacar o botão da sidebar)
    public bool IsAbaInicio =>
        CurrentScreen is ShellViewModel s && s.IsDashboardView;

    public bool IsAbaClientes =>
        CurrentScreen is ShellViewModel s && s.IsClientesPanelAberto;

    public bool IsAbaEstoque =>
        CurrentScreen is ShellViewModel s && s.IsEstoqueView;

    public bool IsAbaAgenda =>
        CurrentScreen is ShellViewModel s && s.IsAgendaView;

    public bool IsAbaOrdensServico =>
        CurrentScreen is ShellViewModel s && s.IsOrdensServicoView;

    public bool IsAbaUsuarios =>
        CurrentScreen is ShellViewModel s && s.IsUsuariosView;

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

    private void OnShellPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        NotificarAbas();

    private void NotificarAbas()
    {
        OnPropertyChanged(nameof(IsAbaInicio));
        OnPropertyChanged(nameof(IsAbaClientes));
        OnPropertyChanged(nameof(IsAbaEstoque));
        OnPropertyChanged(nameof(IsAbaAgenda));
        OnPropertyChanged(nameof(IsAbaOrdensServico));
        OnPropertyChanged(nameof(IsAbaUsuarios));
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
    private async Task AbrirEstoqueAsync()
    {
        if (CurrentScreen is ShellViewModel shell)
            await shell.MostrarEstoqueAsync();
    }

    [RelayCommand]
    private async Task AbrirAgendaAsync()
    {
        if (CurrentScreen is ShellViewModel shell)
            await shell.MostrarAgendaAsync();
    }

    [RelayCommand]
    private async Task AbrirOrdensServicoAsync()
    {
        if (CurrentScreen is ShellViewModel shell)
            await shell.MostrarOrdensServicoAsync();
    }

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
        var shell = new ShellViewModel(
            _auth,
            _clienteService,
            _estoqueService,
            _agendaService,
            _ordemServicoService,
            _dashboardService,
            usuario);
        await shell.InicializarAsync();
        CurrentScreen = shell;
        IsPainelAutenticado = true;
    }

    private LoginViewModel CriarTelaLogin() =>
        new(_auth, AbrirTelaPrincipal);
}
