using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FixMaster.Models;
using FixMaster.Services;
using FixMaster.Services.Interfaces;

namespace FixMaster.ViewModels;

public sealed partial class EstoqueViewModel : ViewModelBase
{
    private readonly IEstoqueService _estoqueService;
    private IReadOnlyList<ItemEstoque> _itensMatriz = [];
    private string _categoriaDoCodigo = "";
    private string _tipoInsumoDoCodigo = "";
    private int _geracaoCodigoAtual;

    [ObservableProperty]
    private bool _isCarregando;

    [ObservableProperty]
    private string? _mensagemErro;

    [ObservableProperty]
    private string? _mensagemSucesso;

    [ObservableProperty]
    private string? _mensagemErroFormulario;

    [ObservableProperty]
    private string _termoBusca = "";

    [ObservableProperty]
    private ItemEstoque? _materialSelecionado;

    [ObservableProperty]
    private bool _isFormularioAberto;

    [ObservableProperty]
    private bool _isConfirmacaoRemocaoAberta;

    [ObservableProperty]
    private int _idMaterialEmEdicao;

    [ObservableProperty]
    private string _codigo = "";

    [ObservableProperty]
    private string _nome = "";

    [ObservableProperty]
    private string _descricao = "";

    [ObservableProperty]
    private string _categoria = "";

    [ObservableProperty]
    private string _tipoInsumo = "";

    [ObservableProperty]
    private string _quantidade = "0";

    [ObservableProperty]
    private string _precoUnitario = "0,00";

    [ObservableProperty]
    private string _prateleira = "";

    [ObservableProperty]
    private string _posicao = "";

    public EstoqueViewModel(IEstoqueService estoqueService) =>
        _estoqueService = estoqueService;

    public ObservableCollection<ItemEstoque> Materiais { get; } = [];
    public ObservableCollection<LinhaPrateleiraViewModel> Prateleiras { get; } = [];
    public ObservableCollection<string> TiposMaterial { get; } =
        new(CategoriaMaterial.Tipos);
    public IReadOnlyList<string> TiposInsumo { get; } =
        ["Elétrico", "Hidráulico"];
    public bool EhInsumo => Categoria == "Insumos";
    public string LocalizacaoDestaque => MaterialSelecionado is null
        ? "--"
        : $"{MaterialSelecionado.Prateleira}{MaterialSelecionado.Posicao}";
    public string PrecoSelecionado => MaterialSelecionado?.PrecoUnitario.ToString(
        "C2", CultureInfo.GetCultureInfo("pt-BR")) ?? "";
    public bool NenhumMaterialEncontrado => !IsCarregando && Materiais.Count == 0;
    public bool TemMaterialSelecionado => MaterialSelecionado is not null;
    public string TituloFormulario => IdMaterialEmEdicao == 0
        ? "CADASTRAR MATERIAL"
        : "EDITAR MATERIAL";

    public async Task InicializarAsync() => await AtualizarAsync();

    public async Task AtualizarCodigoPorCategoriaAsync(
        string? categoria,
        string? tipoInsumo = null)
    {
        categoria ??= "";
        var categoriaMudou = categoria != _categoriaDoCodigo;
        if (categoriaMudou)
        {
            _categoriaDoCodigo = categoria;
            TipoInsumo = "";
            tipoInsumo = "";
        }
        else
        {
            tipoInsumo ??= TipoInsumo;
        }

        if (categoria != "Insumos")
        {
            tipoInsumo = "";
            TipoInsumo = "";
        }
        else if (TipoInsumo != tipoInsumo)
        {
            TipoInsumo = tipoInsumo ?? "";
        }

        tipoInsumo ??= "";
        if (!categoriaMudou && tipoInsumo == _tipoInsumoDoCodigo)
            return;

        _tipoInsumoDoCodigo = tipoInsumo;
        var geracao = ++_geracaoCodigoAtual;
        Codigo = "";
        MensagemErroFormulario = null;
        if (string.IsNullOrWhiteSpace(categoria))
            return;
        if (categoria == "Insumos" && string.IsNullOrWhiteSpace(tipoInsumo))
            return;

        var resultado = await _estoqueService.GerarCodigoAsync(categoria, tipoInsumo);
        if (geracao != _geracaoCodigoAtual
            || categoria != Categoria
            || tipoInsumo != TipoInsumo)
            return;

        if (!resultado.Sucesso || resultado.Valor is null)
        {
            MensagemErroFormulario = resultado.Mensagem
                ?? "Não foi possível gerar o código do material.";
            return;
        }

        Codigo = resultado.Valor;
    }

    [RelayCommand]
    private async Task AtualizarAsync()
    {
        IsCarregando = true;
        MensagemErro = null;
        MensagemSucesso = null;

        var resultadoBusca = await _estoqueService.BuscarAsync(TermoBusca);
        if (!resultadoBusca.Sucesso || resultadoBusca.Valor is null)
        {
            MensagemErro = resultadoBusca.Mensagem ?? "Não foi possível carregar os materiais.";
            Materiais.Clear();
            MaterialSelecionado = null;
            _itensMatriz = [];
            AtualizarMatriz();
            IsCarregando = false;
            OnPropertyChanged(nameof(NenhumMaterialEncontrado));
            return;
        }

        var resultadoMatriz = await _estoqueService.ObterMatrizAsync();
        if (!resultadoMatriz.Sucesso || resultadoMatriz.Valor is null)
        {
            MensagemErro = resultadoMatriz.Mensagem ?? "Não foi possível carregar a matriz do estoque.";
            _itensMatriz = [];
        }
        else
        {
            _itensMatriz = resultadoMatriz.Valor;
        }

        Materiais.Clear();
        foreach (var material in resultadoBusca.Valor)
            Materiais.Add(material);

        if (MaterialSelecionado is null
            || Materiais.All(item => item.Id != MaterialSelecionado.Id))
        {
            MaterialSelecionado = Materiais.FirstOrDefault();
        }

        AtualizarMatriz();
        IsCarregando = false;
        OnPropertyChanged(nameof(NenhumMaterialEncontrado));
    }

    [RelayCommand]
    private void NovoMaterial()
    {
        LimparFormulario();
        IsFormularioAberto = true;
    }

    [RelayCommand]
    private async Task LimparBuscaAsync()
    {
        TermoBusca = "";
        await AtualizarAsync();
    }

    [RelayCommand]
    private void EditarMaterial()
    {
        if (MaterialSelecionado is null)
            return;

        IdMaterialEmEdicao = MaterialSelecionado.Id;
        Codigo = MaterialSelecionado.Codigo;
        Nome = MaterialSelecionado.Nome;
        Descricao = MaterialSelecionado.Descricao;
        if (!TiposMaterial.Contains(MaterialSelecionado.Categoria))
            TiposMaterial.Add(MaterialSelecionado.Categoria);
        _categoriaDoCodigo = MaterialSelecionado.Categoria;
        Categoria = MaterialSelecionado.Categoria;
        _tipoInsumoDoCodigo = MaterialSelecionado.Codigo.StartsWith(
            "INS-E-", StringComparison.OrdinalIgnoreCase)
            ? "Elétrico"
            : MaterialSelecionado.Codigo.StartsWith("INS-H-", StringComparison.OrdinalIgnoreCase)
                ? "Hidráulico"
                : "";
        TipoInsumo = _tipoInsumoDoCodigo;
        Quantidade = MaterialSelecionado.Quantidade.ToString(CultureInfo.InvariantCulture);
        PrecoUnitario = MaterialSelecionado.PrecoUnitario.ToString(
            "0.00", CultureInfo.GetCultureInfo("pt-BR"));
        Prateleira = MaterialSelecionado.Prateleira;
        Posicao = MaterialSelecionado.Posicao.ToString(CultureInfo.InvariantCulture);
        MensagemErroFormulario = null;
        IsFormularioAberto = true;
    }

    [RelayCommand]
    private async Task SalvarMaterialAsync()
    {
        MensagemErroFormulario = null;
        if (!int.TryParse(Quantidade, NumberStyles.Integer, CultureInfo.CurrentCulture, out var quantidade))
        {
            MensagemErroFormulario = "Quantidade: informe um número inteiro igual ou maior que zero.";
            return;
        }

        if (!int.TryParse(Posicao, NumberStyles.Integer, CultureInfo.CurrentCulture, out var posicao))
        {
            MensagemErroFormulario = "Posição: informe um número de 1 a 8.";
            return;
        }

        if (!decimal.TryParse(
                PrecoUnitario, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-BR"), out var preco)
            && !decimal.TryParse(
                PrecoUnitario, NumberStyles.Number, CultureInfo.InvariantCulture, out preco))
        {
            MensagemErroFormulario = "Preço unitário: informe um valor numérico válido.";
            return;
        }

        var material = new ItemEstoque
        {
            Id = IdMaterialEmEdicao,
            Codigo = Codigo,
            Nome = Nome,
            Descricao = Descricao,
            Categoria = Categoria,
            Quantidade = quantidade,
            PrecoUnitario = preco,
            Prateleira = Prateleira,
            Posicao = posicao
        };

        var resultado = await _estoqueService.SalvarAsync(material);
        if (!resultado.Sucesso)
        {
            MensagemErroFormulario = resultado.Mensagem ?? "Não foi possível salvar o material.";
            return;
        }

        IsFormularioAberto = false;
        TermoBusca = "";
        MaterialSelecionado = null;
        var mensagemSucesso = IdMaterialEmEdicao == 0
            ? "Material cadastrado com sucesso."
            : "Material atualizado com sucesso.";
        await AtualizarAsync();
        MaterialSelecionado = Materiais.FirstOrDefault(item => item.Id == material.Id);
        MensagemSucesso = mensagemSucesso;
    }

    [RelayCommand]
    private void CancelarFormulario()
    {
        IsFormularioAberto = false;
        LimparFormulario();
    }

    [RelayCommand]
    private void SelecionarMaterial(ItemEstoque material) =>
        MaterialSelecionado = material;

    [RelayCommand]
    private void SelecionarCelula(EstoqueCelulaViewModel celula)
    {
        if (celula.Material is not null)
            MaterialSelecionado = celula.Material;
    }

    [RelayCommand]
    private void SolicitarRemocao()
    {
        if (MaterialSelecionado is not null)
            IsConfirmacaoRemocaoAberta = true;
    }

    [RelayCommand]
    private void CancelarRemocao() => IsConfirmacaoRemocaoAberta = false;

    [RelayCommand]
    private async Task ConfirmarRemocaoAsync()
    {
        if (MaterialSelecionado is null)
            return;

        var resultado = await _estoqueService.DesativarAsync(MaterialSelecionado.Id);
        IsConfirmacaoRemocaoAberta = false;
        if (!resultado.Sucesso)
        {
            MensagemErro = resultado.Mensagem ?? "Não foi possível remover o material.";
            return;
        }

        MaterialSelecionado = null;
        await AtualizarAsync();
        MensagemSucesso = resultado.Mensagem;
    }

    partial void OnMaterialSelecionadoChanged(ItemEstoque? value)
    {
        OnPropertyChanged(nameof(LocalizacaoDestaque));
        OnPropertyChanged(nameof(PrecoSelecionado));
        OnPropertyChanged(nameof(TemMaterialSelecionado));
        AtualizarMatriz();
    }

    partial void OnIsCarregandoChanged(bool value) =>
        OnPropertyChanged(nameof(NenhumMaterialEncontrado));

    partial void OnIdMaterialEmEdicaoChanged(int value) =>
        OnPropertyChanged(nameof(TituloFormulario));

    partial void OnCategoriaChanged(string value) =>
        OnPropertyChanged(nameof(EhInsumo));

    private void LimparFormulario()
    {
        IdMaterialEmEdicao = 0;
        _categoriaDoCodigo = "";
        _tipoInsumoDoCodigo = "";
        Codigo = "";
        Nome = "";
        Descricao = "";
        Categoria = "";
        TipoInsumo = "";
        Quantidade = "0";
        PrecoUnitario = "0,00";
        Prateleira = "";
        Posicao = "";
        MensagemErroFormulario = null;
    }

    private void AtualizarMatriz()
    {
        Prateleiras.Clear();
        for (var letra = 'A'; letra <= 'G'; letra++)
        {
            var celulas = new ObservableCollection<EstoqueCelulaViewModel>();
            for (var posicao = 1; posicao <= 8; posicao++)
            {
                var material = _itensMatriz.FirstOrDefault(item =>
                    item.Prateleira == letra.ToString() && item.Posicao == posicao);
                celulas.Add(new EstoqueCelulaViewModel(
                    letra.ToString(), posicao, material, MaterialSelecionado?.Id));
            }

            Prateleiras.Add(new LinhaPrateleiraViewModel(letra.ToString(), celulas));
        }
    }
}

public sealed class LinhaPrateleiraViewModel(
    string prateleira,
    ObservableCollection<EstoqueCelulaViewModel> celulas)
{
    public string Prateleira { get; } = prateleira;
    public ObservableCollection<EstoqueCelulaViewModel> Celulas { get; } = celulas;
}

public sealed class EstoqueCelulaViewModel(
    string prateleira,
    int posicao,
    ItemEstoque? material,
    int? idDestaque)
{
    public string Coordenada { get; } = $"{prateleira}{posicao}";
    public ItemEstoque? Material { get; } = material;
    public bool Ocupada => Material is not null;
    public bool EmDestaque => Material?.Id == idDestaque;
    public string TextoNormal => EmDestaque ? "" : Material?.Codigo ?? "";
    public string TextoDestaque => EmDestaque ? Material?.Codigo ?? "" : "";
    public string Dica => Material is null
        ? $"{Coordenada} - livre"
        : $"{Coordenada} - {Material.Nome} ({Material.Codigo})";
}
