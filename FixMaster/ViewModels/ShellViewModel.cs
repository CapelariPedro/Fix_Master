using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FixMaster.Infrastructure;
using FixMaster.Services.Interfaces;

namespace FixMaster.ViewModels;

public sealed partial class ShellViewModel : ViewModelBase
{
    private readonly IAuthService _auth;
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

    public ShellViewModel(IAuthService auth, UsuarioAutenticado usuarioLogado)
    {
        _auth = auth;
        _usuarioLogado = usuarioLogado;
        IsAdministrador = usuarioLogado.EhAdministrador;
    }

    public ObservableCollection<UsuarioResumo> Usuarios { get; } = [];

    public bool IsOperador => !IsAdministrador;

    public Task InicializarAsync() =>
        IsAdministrador ? CarregarUsuariosAsync() : Task.CompletedTask;

    [RelayCommand]
    private async Task AtualizarAsync() => await CarregarUsuariosAsync();

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
