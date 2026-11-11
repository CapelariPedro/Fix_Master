using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FixMaster.Infrastructure;
using FixMaster.Models;
using FixMaster.Services;
using FixMaster.Services.Interfaces;

namespace FixMaster.ViewModels;

public sealed partial class ShellViewModel : ViewModelBase
{
    private readonly IAuthService _auth;
    private readonly IClienteService _clienteService;
    private readonly UsuarioAutenticado _usuarioLogado;

    [ObservableProperty]
    private bool _isAdministrador;

    [ObservableProperty]
    private bool _isCarregando;

    [ObservableProperty]
    private string? _mensagemErro;

    [ObservableProperty]
    private string? _mensagemSucesso;

    [ObservableProperty]
    private UsuarioResumo? _usuarioParaExcluir;

    [ObservableProperty]
    private bool _isConfirmacaoExclusaoAberta;

    [ObservableProperty]
    private bool _isClientesPanelAberto = true;

    [ObservableProperty]
    private bool _isUsuariosPanelAberto;

    [ObservableProperty]
    private string _filtroClientes = "";

    [ObservableProperty]
    private ClienteResumo? _clienteSelecionado;

    [ObservableProperty]
    private bool _isFormularioClienteAberto;

    [ObservableProperty]
    private bool _isConfirmacaoExclusaoClienteAberta;

    [ObservableProperty]
    private ClienteResumo? _clienteParaExcluir;

    [ObservableProperty]
    private int _idClienteEmEdicao;

    [ObservableProperty]
    private string _nome = "";

    [ObservableProperty]
    private string _documento = "";

    [ObservableProperty]
    private string _cep = "";

    [ObservableProperty]
    private string _telefone = "";

    [ObservableProperty]
    private string _endereco = "";

    [ObservableProperty]
    private bool _ehEmpresa;

    [ObservableProperty]
    private bool _isSalvandoCliente;

    [ObservableProperty]
    private string? _mensagemErroCliente;

    [ObservableProperty]
    private string? _mensagemClientes;

    public ShellViewModel(
        IAuthService auth,
        IClienteService clienteService,
        UsuarioAutenticado usuarioLogado)
    {
        _auth = auth;
        _clienteService = clienteService;
        _usuarioLogado = usuarioLogado;
        IsAdministrador = usuarioLogado.EhAdministrador;
        IsUsuariosPanelAberto = usuarioLogado.EhAdministrador;
    }

    public ObservableCollection<UsuarioResumo> Usuarios { get; } = [];
    public ObservableCollection<ClienteResumo> Clientes { get; } = [];
    public ObservableCollection<ClienteResumo> ClientesFiltrados { get; } = [];

    public bool IsOperador => !IsAdministrador;
    public bool IsUsuariosView => IsAdministrador && IsUsuariosPanelAberto;
    public int ColSpanClientes => IsUsuariosView ? 1 : 2;
    public int ColunaUsuarios => IsClientesPanelAberto ? 1 : 0;
    public int ColSpanUsuarios => IsClientesPanelAberto ? 1 : 2;
    public bool TemClientes => ClientesFiltrados.Count > 0;
    public bool NenhumClienteEncontrado => !IsCarregando && !TemClientes;
    public bool TemClienteSelecionado => ClienteSelecionado is not null;
    public bool NenhumClienteSelecionado => !TemClienteSelecionado;
    public string TituloDocumento => EhEmpresa ? "CNPJ" : "CPF";
    public int MaxCaracteresDocumento =>
        DocumentoValidator.LimiteCaracteresFormatados(EhEmpresa);
    public string NomeCampoCliente => EhEmpresa ? "RAZÃO SOCIAL" : "NOME COMPLETO";
    public string PlaceholderDocumento => EhEmpresa ? "00.000.000/0000-00" : "000.000.000-00";
    public string TituloFormularioCliente =>
        IdClienteEmEdicao == 0 ? "NOVO CLIENTE" : "EDITAR CLIENTE";
    public bool IsEditandoCliente => IdClienteEmEdicao != 0;
    public string PerfilClienteSelecionado =>
        ClienteSelecionado?.EhEmpresa == true ? "PESSOA JURÍDICA" : "PESSOA FÍSICA";
    public int MaxCaracteresCep => 9;

    public async Task InicializarAsync()
    {
        await CarregarClientesAsync();
        if (IsAdministrador)
            await CarregarUsuariosAsync();
    }

    [RelayCommand]
    public void MostrarInicio()
    {
        IsClientesPanelAberto = true;
        IsUsuariosPanelAberto = IsAdministrador;
    }

    public void MostrarClientes() => IsClientesPanelAberto = true;

    public void MostrarUsuarios()
    {
        if (!IsAdministrador)
            return;

        IsUsuariosPanelAberto = true;
    }

    [RelayCommand]
    private async Task AtualizarAsync()
    {
        if (IsUsuariosPanelAberto && IsAdministrador)
            await CarregarUsuariosAsync();
        if (IsClientesPanelAberto)
            await CarregarClientesAsync();
    }

    [RelayCommand]
    private async Task CarregarUsuariosAsync()
    {
        if (!IsAdministrador)
            return;

        IsCarregando = true;
        MensagemErro = null;
        MensagemSucesso = null;

        var resultado = await _auth.ListarUsuariosAsync();
        if (!resultado.Sucesso || resultado.Valor is null)
        {
            MensagemErro = resultado.Mensagem ?? "Não foi possível carregar os usuários.";
            IsCarregando = false;
            return;
        }

        Usuarios.Clear();
        foreach (var usuario in resultado.Valor)
            Usuarios.Add(usuario);

        IsCarregando = false;
    }

    [RelayCommand]
    private async Task CarregarClientesAsync()
    {
        IsCarregando = true;
        MensagemErro = null;

        var idSelecionado = ClienteSelecionado?.Id;
        var resultado = await _clienteService.ListarAsync(null);
        if (!resultado.Sucesso || resultado.Valor is null)
        {
            MensagemErro = resultado.Mensagem ?? "Não foi possível carregar os clientes.";
            IsCarregando = false;
            return;
        }

        Clientes.Clear();
        foreach (var cliente in resultado.Valor)
            Clientes.Add(cliente);

        AplicarFiltro();
        ClienteSelecionado = ClientesFiltrados.FirstOrDefault(cliente => cliente.Id == idSelecionado)
            ?? ClientesFiltrados.FirstOrDefault();
        OnPropertyChanged(nameof(PerfilClienteSelecionado));
        IsCarregando = false;
    }

    partial void OnFiltroClientesChanged(string value) => AplicarFiltro();

    partial void OnIsUsuariosPanelAbertoChanged(bool value)
    {
        OnPropertyChanged(nameof(IsUsuariosView));
        OnPropertyChanged(nameof(ColSpanClientes));
    }

    partial void OnIsClientesPanelAbertoChanged(bool value)
    {
        OnPropertyChanged(nameof(ColSpanClientes));
        OnPropertyChanged(nameof(ColunaUsuarios));
        OnPropertyChanged(nameof(ColSpanUsuarios));
    }

    partial void OnClienteSelecionadoChanged(ClienteResumo? value) =>
        OnClienteSelecionadoChangedProperties();

    partial void OnEhEmpresaChanged(bool value)
    {
        OnPropertyChanged(nameof(TituloDocumento));
        OnPropertyChanged(nameof(NomeCampoCliente));
        OnPropertyChanged(nameof(PlaceholderDocumento));
        OnPropertyChanged(nameof(MaxCaracteresDocumento));
        Documento = DocumentoValidator.FormatarEnquantoDigita(Documento, value);
    }

    partial void OnIsCarregandoChanged(bool value) =>
        OnPropertyChanged(nameof(NenhumClienteEncontrado));

    partial void OnIdClienteEmEdicaoChanged(int value) =>
        OnIdClienteEmEdicaoChangedProperties();

    private void OnClienteSelecionadoChangedProperties()
    {
        OnPropertyChanged(nameof(PerfilClienteSelecionado));
        OnPropertyChanged(nameof(TemClienteSelecionado));
        OnPropertyChanged(nameof(NenhumClienteSelecionado));
        AtualizarSelecaoVisual();
    }

    private void AtualizarSelecaoVisual()
    {
        var idSelecionado = ClienteSelecionado?.Id;
        for (var indice = 0; indice < Clientes.Count; indice++)
        {
            var atual = Clientes[indice];
            var selecionado = atual.Id == idSelecionado;
            if (atual.EstaSelecionado == selecionado)
                continue;

            var atualizado = atual with { EstaSelecionado = selecionado };
            Clientes[indice] = atualizado;
            var indiceFiltrado = ClientesFiltrados.IndexOf(atual);
            if (indiceFiltrado >= 0)
                ClientesFiltrados[indiceFiltrado] = atualizado;
        }
    }

    private void OnIdClienteEmEdicaoChangedProperties()
    {
        OnPropertyChanged(nameof(TituloFormularioCliente));
        OnPropertyChanged(nameof(IsEditandoCliente));
    }

    private void AplicarFiltro()
    {
        var filtro = FiltroClientes.Trim();
        ClientesFiltrados.Clear();
        foreach (var cliente in Clientes.Where(cliente =>
                     string.IsNullOrEmpty(filtro)
                     || cliente.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase)
                     || cliente.Telefone.Contains(filtro, StringComparison.OrdinalIgnoreCase)
                     || cliente.CepFormatado.Contains(filtro, StringComparison.OrdinalIgnoreCase)
                     || cliente.Documento.Contains(filtro, StringComparison.OrdinalIgnoreCase)
                     || cliente.DocumentoFormatado.Contains(filtro, StringComparison.OrdinalIgnoreCase)))
        {
            ClientesFiltrados.Add(cliente);
        }

        if (ClienteSelecionado is null
            || ClientesFiltrados.All(cliente => cliente.Id != ClienteSelecionado.Id))
        {
            ClienteSelecionado = ClientesFiltrados.FirstOrDefault();
        }

        OnPropertyChanged(nameof(TemClientes));
        OnPropertyChanged(nameof(NenhumClienteEncontrado));
    }

    [RelayCommand]
    private void SelecionarCliente(ClienteResumo cliente)
    {
        ClienteSelecionado = cliente;
    }

    [RelayCommand]
    private void FecharPainelClientes() => IsClientesPanelAberto = false;

    [RelayCommand]
    private void FecharPainelUsuarios()
    {
        IsUsuariosPanelAberto = false;
    }

    [RelayCommand]
    private void NovoCliente()
    {
        LimparFormulario();
        IsFormularioClienteAberto = true;
    }

    [RelayCommand]
    private async Task EditarClienteAsync()
    {
        if (ClienteSelecionado is null)
            return;

        var resultado = await _clienteService.ObterAsync(ClienteSelecionado.Id);
        if (!resultado.Sucesso || resultado.Valor is null)
        {
            MensagemErro = resultado.Mensagem ?? "Não foi possível carregar o cliente.";
            return;
        }

        var cliente = resultado.Valor;
        IdClienteEmEdicao = cliente.Id;
        Nome = cliente.Nome;
        Documento = cliente.DocumentoFormatado;
        Cep = cliente.CepFormatado;
        Telefone = cliente.Telefone;
        Endereco = cliente.Endereco;
        EhEmpresa = cliente.EhEmpresa;
        MensagemErroCliente = null;
        IsFormularioClienteAberto = true;
    }

    [RelayCommand]
    private async Task SalvarClienteAsync()
    {
        MensagemErroCliente = null;
        IsSalvandoCliente = true;

        var cliente = new Cliente
        {
            Id = IdClienteEmEdicao,
            Nome = Nome,
            Documento = Documento,
            Cep = Cep,
            Telefone = Telefone,
            Endereco = Endereco,
            EhEmpresa = EhEmpresa
        };
        var resultado = await _clienteService.SalvarAsync(cliente);

        IsSalvandoCliente = false;
        if (!resultado.Sucesso)
        {
            MensagemErroCliente = resultado.Mensagem ?? "Não foi possível salvar o cliente.";
            return;
        }

        MensagemClientes = IdClienteEmEdicao == 0
            ? "Cliente cadastrado com sucesso."
            : "Dados do cliente atualizados com sucesso.";
        IsFormularioClienteAberto = false;
        FiltroClientes = "";
        var idSalvo = cliente.Id;
        await CarregarClientesAsync();
        ClienteSelecionado = Clientes.FirstOrDefault(item => item.Id == idSalvo);
        OnPropertyChanged(nameof(PerfilClienteSelecionado));
    }

    [RelayCommand]
    private void CancelarFormularioCliente()
    {
        IsFormularioClienteAberto = false;
        LimparFormulario();
    }

    private void LimparFormulario()
    {
        IdClienteEmEdicao = 0;
        Nome = "";
        Documento = "";
        Cep = "";
        Telefone = "";
        Endereco = "";
        EhEmpresa = false;
        MensagemErroCliente = null;
    }

    [RelayCommand]
    private void SolicitarExclusaoCliente()
    {
        if (ClienteSelecionado is null)
            return;

        ClienteParaExcluir = ClienteSelecionado;
        IsConfirmacaoExclusaoClienteAberta = true;
    }

    [RelayCommand]
    private void CancelarExclusaoCliente()
    {
        IsConfirmacaoExclusaoClienteAberta = false;
        ClienteParaExcluir = null;
    }

    [RelayCommand]
    private async Task ConfirmarExclusaoClienteAsync()
    {
        if (ClienteParaExcluir is null)
            return;

        var cliente = ClienteParaExcluir;
        var resultado = await _clienteService.ExcluirOuInativarAsync(cliente.Id);
        IsConfirmacaoExclusaoClienteAberta = false;
        ClienteParaExcluir = null;
        if (!resultado.Sucesso)
        {
            MensagemErro = resultado.Mensagem ?? "Não foi possível excluir o cliente.";
            return;
        }

        MensagemClientes = resultado.Mensagem ?? "Cliente excluído ou inativado com sucesso.";
        await CarregarClientesAsync();
        if (ClienteSelecionado?.Id == cliente.Id)
            ClienteSelecionado = Clientes.FirstOrDefault();
    }

    [RelayCommand]
    private void SolicitarExclusao(UsuarioResumo usuario)
    {
        MensagemErro = null;
        MensagemSucesso = null;
        UsuarioParaExcluir = usuario;
        IsConfirmacaoExclusaoAberta = true;
    }

    [RelayCommand]
    private void CancelarExclusao()
    {
        IsConfirmacaoExclusaoAberta = false;
        UsuarioParaExcluir = null;
    }

    [RelayCommand]
    private async Task ConfirmarExclusaoAsync()
    {
        if (UsuarioParaExcluir is null)
            return;

        MensagemErro = null;
        MensagemSucesso = null;
        var resultado = await _auth.ExcluirUsuarioAsync(
            _usuarioLogado.Username, UsuarioParaExcluir.Id);

        if (!resultado.Sucesso)
        {
            MensagemErro = resultado.Mensagem ?? "Não foi possível excluir o usuário.";
            IsConfirmacaoExclusaoAberta = false;
            UsuarioParaExcluir = null;
            return;
        }

        Usuarios.Remove(UsuarioParaExcluir);
        MensagemSucesso = "Usuário excluído com sucesso.";
        IsConfirmacaoExclusaoAberta = false;
        UsuarioParaExcluir = null;
    }
}
