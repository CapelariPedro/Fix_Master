using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FixMaster.Infrastructure;
using FixMaster.Models;
using FixMaster.Services;
using FixMaster.Services.Interfaces;

namespace FixMaster.ViewModels;

public sealed partial class OrdemServicoViewModel : ViewModelBase
{
    private static readonly CultureInfo CulturaPtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IOrdemServicoService _ordemServicoService;
    private readonly IEstoqueService _estoqueService;

    public OrdemServicoViewModel(
        IOrdemServicoService ordemServicoService,
        IEstoqueService estoqueService)
    {
        _ordemServicoService = ordemServicoService;
        _estoqueService = estoqueService;
    }

    public ObservableCollection<OrdemServico> Ordens { get; } = [];
    public ObservableCollection<AgendamentoResumo> AgendamentosDisponiveis { get; } = [];
    public ObservableCollection<ItemEstoque> MateriaisDisponiveis { get; } = [];
    public ObservableCollection<ItemOS> ItensFormulario { get; } = [];

    [ObservableProperty]
    private OrdemServico? _selecionada;

    [ObservableProperty]
    private AgendamentoResumo? _agendamentoSelecionado;

    [ObservableProperty]
    private ItemEstoque? _materialSelecionado;

    [ObservableProperty]
    private bool _isFormularioAberto;

    [ObservableProperty]
    private bool _isConfirmacaoAberta;

    [ObservableProperty]
    private bool _isEditando;

    [ObservableProperty]
    private bool _isListaMateriaisAberta;

    [ObservableProperty]
    private string _relatorioTecnico = "";

    [ObservableProperty]
    private string _maoDeObraTexto = "";

    [ObservableProperty]
    private string _filtroMaterial = "";

    [ObservableProperty]
    private string _quantidadeMaterialTexto = "1";

    [ObservableProperty]
    private string? _mensagem;

    public decimal TotalSelecionado =>
        Selecionada is null ? 0m : OrdemServicoService.CalcularTotal(Selecionada);

    public decimal TotalFormulario =>
        decimal.TryParse(
            MaoDeObraTexto,
            NumberStyles.Currency,
            CulturaPtBr,
            out var maoDeObra)
            ? maoDeObra + ItensFormulario.Sum(item =>
                item.QuantidadeUtilizada * item.ValorUnitario)
            : ItensFormulario.Sum(item => item.QuantidadeUtilizada * item.ValorUnitario);

    public bool PodeConcluir =>
        Selecionada?.Status == StatusOS.Aberta && !IsFormularioAberto;

    public bool PodeEditar =>
        Selecionada?.Status == StatusOS.Aberta && !IsFormularioAberto;

    public bool PodeSalvar => AgendamentoSelecionado is not null;
    public bool TemSelecionada => Selecionada is not null;
    public bool TemOrdens => Ordens.Count > 0;
    public bool TemAgendamentosDisponiveis => AgendamentosDisponiveis.Count > 0;
    public bool TemMateriaisDisponiveis => MateriaisDisponiveis.Count > 0;

    partial void OnSelecionadaChanged(OrdemServico? value)
    {
        OnPropertyChanged(nameof(TotalSelecionado));
        OnPropertyChanged(nameof(PodeConcluir));
        OnPropertyChanged(nameof(PodeEditar));
        OnPropertyChanged(nameof(TemSelecionada));
    }

    partial void OnIsFormularioAbertoChanged(bool value)
    {
        OnPropertyChanged(nameof(PodeConcluir));
        OnPropertyChanged(nameof(PodeEditar));
    }

    partial void OnAgendamentoSelecionadoChanged(AgendamentoResumo? value) =>
        OnPropertyChanged(nameof(PodeSalvar));

    partial void OnFiltroMaterialChanged(string value) =>
        IsListaMateriaisAberta = false;

    partial void OnMaterialSelecionadoChanged(ItemEstoque? value)
    {
        if (value is not null)
            IsListaMateriaisAberta = false;
    }

    partial void OnMaoDeObraTextoChanged(string value) =>
        OnPropertyChanged(nameof(TotalFormulario));

    [RelayCommand]
    public async Task CarregarAsync()
    {
        var idSelecionado = Selecionada?.Id;
        var resultado = await _ordemServicoService.ListarAsync();
        if (!resultado.Sucesso || resultado.Valor is null)
        {
            Mensagem = resultado.Mensagem ?? "Não foi possível carregar as ordens de serviço.";
            return;
        }

        Ordens.Clear();
        foreach (var ordem in resultado.Valor)
            Ordens.Add(ordem);

        OnPropertyChanged(nameof(TemOrdens));
        Selecionada = Ordens.FirstOrDefault(ordem => ordem.Id == idSelecionado)
            ?? Ordens.FirstOrDefault();
        OnPropertyChanged(nameof(TotalSelecionado));
        Mensagem = null;
    }

    [RelayCommand]
    private async Task NovaAsync()
    {
        LimparFormulario();
        IsEditando = false;
        Mensagem = null;
        var resultado = await _ordemServicoService.ListarAgendamentosDisponiveisAsync();
        if (!resultado.Sucesso || resultado.Valor is null)
        {
            Mensagem = resultado.Mensagem ?? "Não foi possível carregar os agendamentos.";
            return;
        }

        AgendamentosDisponiveis.Clear();
        foreach (var agendamento in resultado.Valor)
            AgendamentosDisponiveis.Add(agendamento);
        OnPropertyChanged(nameof(TemAgendamentosDisponiveis));
        AgendamentoSelecionado = AgendamentosDisponiveis.FirstOrDefault();
        IsFormularioAberto = true;
        await CarregarMateriaisAsync(false);
    }

    [RelayCommand]
    private void Selecionar(OrdemServico ordem)
    {
        Selecionada = ordem;
        IsConfirmacaoAberta = false;
    }

    [RelayCommand]
    private async Task EditarAsync()
    {
        if (Selecionada is null || Selecionada.Status != StatusOS.Aberta)
            return;

        IsEditando = true;
        AgendamentosDisponiveis.Clear();
        if (Selecionada.Agendamento is { } agendamento)
        {
            AgendamentoSelecionado = new AgendamentoResumo(
                agendamento.Id,
                agendamento.ClienteId,
                agendamento.Cliente?.Nome ?? "Cliente",
                agendamento.DataHoraInicio,
                agendamento.DataHoraFim,
                agendamento.DuracaoMinutos,
                agendamento.DescricaoServico,
                agendamento.Status);
            AgendamentosDisponiveis.Add(AgendamentoSelecionado);
        }

        RelatorioTecnico = Selecionada.RelatorioTecnico;
        MaoDeObraTexto = Selecionada.MaoDeObra.ToString("N2", CulturaPtBr);
        ItensFormulario.Clear();
        foreach (var item in Selecionada.Itens)
        {
            ItensFormulario.Add(new ItemOS
            {
                OrdemServicoId = Selecionada.Id,
                ItemEstoqueId = item.ItemEstoqueId,
                ItemEstoque = item.ItemEstoque,
                QuantidadeUtilizada = item.QuantidadeUtilizada,
                ValorUnitario = item.ValorUnitario
            });
        }

        OnPropertyChanged(nameof(TotalFormulario));
        OnPropertyChanged(nameof(TemAgendamentosDisponiveis));
        Mensagem = null;
        IsFormularioAberto = true;
        await CarregarMateriaisAsync(false);
    }

    [RelayCommand]
    private Task BuscarMateriaisAsync() => CarregarMateriaisAsync(true);

    private async Task CarregarMateriaisAsync(bool abrirLista)
    {
        IsListaMateriaisAberta = false;
        var resultado = await _estoqueService.BuscarAsync(FiltroMaterial);
        if (!resultado.Sucesso || resultado.Valor is null)
        {
            Mensagem = resultado.Mensagem ?? "Não foi possível buscar os materiais.";
            return;
        }

        MateriaisDisponiveis.Clear();
        foreach (var material in resultado.Valor)
            MateriaisDisponiveis.Add(material);
        OnPropertyChanged(nameof(TemMateriaisDisponiveis));

        if (MaterialSelecionado is not null
            && MateriaisDisponiveis.All(item => item.Id != MaterialSelecionado.Id))
        {
            MaterialSelecionado = null;
        }

        IsListaMateriaisAberta = abrirLista;
    }

    [RelayCommand]
    private void AdicionarMaterial()
    {
        if (MaterialSelecionado is null)
        {
            Mensagem = "Selecione um material ativo do estoque.";
            return;
        }

        if (!int.TryParse(QuantidadeMaterialTexto, NumberStyles.None, CulturaPtBr, out var quantidade)
            || quantidade <= 0)
        {
            Mensagem = "Informe uma quantidade inteira maior que zero.";
            return;
        }

        var itemExistente = ItensFormulario.FirstOrDefault(
            item => item.ItemEstoqueId == MaterialSelecionado.Id);
        if (itemExistente is not null)
        {
            itemExistente.QuantidadeUtilizada += quantidade;
        }
        else
        {
            ItensFormulario.Add(new ItemOS
            {
                OrdemServicoId = Selecionada?.Id ?? 0,
                ItemEstoqueId = MaterialSelecionado.Id,
                ItemEstoque = MaterialSelecionado,
                QuantidadeUtilizada = quantidade,
                ValorUnitario = MaterialSelecionado.PrecoUnitario
            });
        }

        QuantidadeMaterialTexto = "1";
        OnPropertyChanged(nameof(TotalFormulario));
        Mensagem = $"{MaterialSelecionado.Nome} adicionado à ordem.";
    }

    [RelayCommand]
    private void RemoverMaterial(ItemOS item)
    {
        if (item is null)
            return;

        ItensFormulario.Remove(item);
        OnPropertyChanged(nameof(TotalFormulario));
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (AgendamentoSelecionado is null)
        {
            Mensagem = "Selecione um agendamento sem ordem de serviço.";
            return;
        }

        if (!decimal.TryParse(
                MaoDeObraTexto,
                NumberStyles.Currency,
                CulturaPtBr,
                out var maoDeObra)
            && !decimal.TryParse(
                MaoDeObraTexto,
                NumberStyles.Currency,
                CultureInfo.InvariantCulture,
                out maoDeObra))
        {
            Mensagem = "Informe um valor válido para a mão de obra (R$).";
            return;
        }

        if (maoDeObra is < 0 or > 99999999.99m)
        {
            Mensagem = "A mão de obra deve estar entre R$ 0,00 e R$ 99.999.999,99.";
            return;
        }

        var ordem = new OrdemServico
        {
            Id = IsEditando ? Selecionada?.Id ?? 0 : 0,
            AgendamentoId = AgendamentoSelecionado.Id,
            RelatorioTecnico = RelatorioTecnico,
            MaoDeObra = decimal.Round(maoDeObra, 2),
            Itens = ItensFormulario.Select(item => new ItemOS
            {
                ItemEstoqueId = item.ItemEstoqueId,
                QuantidadeUtilizada = item.QuantidadeUtilizada,
                ValorUnitario = item.ValorUnitario
            }).ToList()
        };

        var estavaEditando = IsEditando;
        int id;
        if (estavaEditando)
        {
            ordem.Id = Selecionada?.Id ?? 0;
            var resultadoAtualizacao = await _ordemServicoService.AtualizarAsync(ordem);
            if (!resultadoAtualizacao.Sucesso)
            {
                Mensagem = resultadoAtualizacao.Mensagem
                    ?? "Não foi possível atualizar a ordem de serviço.";
                return;
            }

            id = ordem.Id;
        }
        else
        {
            var resultadoCriacao = await _ordemServicoService.CriarAsync(ordem);
            if (!resultadoCriacao.Sucesso || resultadoCriacao.Valor <= 0)
            {
                Mensagem = resultadoCriacao.Mensagem
                    ?? "Não foi possível criar a ordem de serviço.";
                return;
            }

            id = resultadoCriacao.Valor;
        }

        IsFormularioAberto = false;
        IsEditando = false;
        await CarregarAsync();
        if (id > 0)
            Selecionada = Ordens.FirstOrDefault(item => item.Id == id) ?? Selecionada;
        if (Ordens.Any(item => item.Id == id))
        {
            Mensagem = estavaEditando
                ? $"OS #{id} atualizada com sucesso."
                : $"OS #{id} criada com sucesso.";
        }
    }

    [RelayCommand]
    private void CancelarFormulario()
    {
        IsFormularioAberto = false;
        IsEditando = false;
        LimparFormulario();
        Mensagem = null;
    }

    [RelayCommand]
    private void SolicitarConclusao()
    {
        if (Selecionada?.Status == StatusOS.Aberta)
        {
            IsConfirmacaoAberta = true;
            Mensagem = null;
        }
    }

    [RelayCommand]
    private void CancelarConclusao() => IsConfirmacaoAberta = false;

    [RelayCommand]
    private async Task ConfirmarConclusaoAsync()
    {
        if (Selecionada is null)
            return;

        var id = Selecionada.Id;
        var resultado = await _ordemServicoService.ConcluirAsync(id);
        if (!resultado.Sucesso)
        {
            Mensagem = resultado.Mensagem ?? "Não foi possível concluir a ordem de serviço.";
            IsConfirmacaoAberta = false;
            return;
        }

        IsConfirmacaoAberta = false;
        await CarregarAsync();
        Selecionada = Ordens.FirstOrDefault(ordem => ordem.Id == id) ?? Selecionada;
        Mensagem = resultado.Mensagem;
    }

    private void LimparFormulario()
    {
        AgendamentoSelecionado = null;
        MaterialSelecionado = null;
        IsListaMateriaisAberta = false;
        RelatorioTecnico = "";
        MaoDeObraTexto = "0,00";
        FiltroMaterial = "";
        QuantidadeMaterialTexto = "1";
        ItensFormulario.Clear();
        OnPropertyChanged(nameof(TotalFormulario));
    }
}
