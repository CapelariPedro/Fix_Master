using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FixMaster.Infrastructure;
using FixMaster.Models;
using FixMaster.Services.Interfaces;

namespace FixMaster.ViewModels;

public sealed partial class DashboardViewModel : ViewModelBase
{
    private readonly IDashboardService _dashboardService;
    private readonly Func<DateTime, Task> _abrirAgenda;
    private readonly Func<AgendamentoResumo, Task> _abrirOrdemServico;
    private readonly Func<AtividadeRecente, Task> _abrirAtividade;
    private readonly Func<int, Task> _abrirOrdemExistente;

    [ObservableProperty]
    private DashboardIndicadores _indicadores = DashboardIndicadores.Vazio;

    [ObservableProperty]
    private bool _isCarregando;

    [ObservableProperty]
    private string? _mensagemErro;

    public DashboardViewModel(
        IDashboardService dashboardService,
        Func<DateTime, Task> abrirAgenda,
        Func<AgendamentoResumo, Task> abrirOrdemServico,
        Func<AtividadeRecente, Task> abrirAtividade,
        Func<int, Task> abrirOrdemExistente)
    {
        _dashboardService = dashboardService;
        _abrirAgenda = abrirAgenda;
        _abrirOrdemServico = abrirOrdemServico;
        _abrirAtividade = abrirAtividade;
        _abrirOrdemExistente = abrirOrdemExistente;
    }

    public ObservableCollection<AtividadeRecente> AtividadesRecentes { get; } = [];
    public ObservableCollection<AgendamentoResumo> AgendamentosDoDia { get; } = [];
    public ObservableCollection<AgendamentoResumo> OrdensParaAbrir { get; } = [];
    public ObservableCollection<OrdemServicoResumo> OrdensParaConcluir { get; } = [];
    public ObservableCollection<OrdemServicoResumo> OrdensConcluidasHoje { get; } = [];

    public bool NenhumAgendamentoParaHoje =>
        !IsCarregando && AgendamentosDoDia.Count == 0;
    public bool NenhumaOrdemParaAbrir =>
        !IsCarregando && OrdensParaAbrir.Count == 0;
    public bool NenhumaOrdemParaConcluir =>
        !IsCarregando && OrdensParaConcluir.Count == 0;
    public bool NenhumaOrdemConcluidaHoje =>
        !IsCarregando && OrdensConcluidasHoje.Count == 0;
    public bool NenhumaAtividadeRecente =>
        !IsCarregando && AtividadesRecentes.Count == 0;

    public async Task InicializarAsync() => await CarregarAsync();

    partial void OnIsCarregandoChanged(bool value)
    {
        OnPropertyChanged(nameof(NenhumAgendamentoParaHoje));
        OnPropertyChanged(nameof(NenhumaOrdemParaAbrir));
        OnPropertyChanged(nameof(NenhumaOrdemParaConcluir));
        OnPropertyChanged(nameof(NenhumaOrdemConcluidaHoje));
        OnPropertyChanged(nameof(NenhumaAtividadeRecente));
    }

    [RelayCommand]
    private Task AbrirAgendaAsync() => _abrirAgenda(DateTime.Today);

    [RelayCommand]
    private Task AbrirAgendamentoAsync(AgendamentoResumo agendamento) =>
        _abrirAgenda(agendamento.DataHoraInicio.Date);

    [RelayCommand]
    private Task AbrirOrdemServicoAsync(AgendamentoResumo agendamento) =>
        _abrirOrdemServico(agendamento);

    [RelayCommand]
    private Task AbrirOrdemExistenteAsync(OrdemServicoResumo ordem) =>
        _abrirOrdemExistente(ordem.Id);

    [RelayCommand]
    private Task AbrirAtividadeAsync(AtividadeRecente atividade) =>
        _abrirAtividade(atividade);

    [RelayCommand]
    public async Task CarregarAsync()
    {
        IsCarregando = true;
        MensagemErro = null;

        try
        {
            var indicadoresTask = _dashboardService.ObterIndicadoresDoDiaAsync();
            var agendamentosTask = _dashboardService.ObterAgendamentosDoDiaAsync();
            var atividadesTask = _dashboardService.ObterAtividadesRecentesAsync(20);
            var paraConcluirTask = _dashboardService.ObterOrdensParaConcluirAsync();
            var concluidasTask = _dashboardService.ObterOrdensConcluidasDoDiaAsync();
            await Task.WhenAll(
                indicadoresTask,
                agendamentosTask,
                atividadesTask,
                paraConcluirTask,
                concluidasTask);

            var indicadores = await indicadoresTask;
            var agendamentos = await agendamentosTask;
            var atividades = await atividadesTask;
            var paraConcluir = await paraConcluirTask;
            var concluidas = await concluidasTask;

            if (!indicadores.Sucesso || indicadores.Valor is null)
            {
                MensagemErro = indicadores.Mensagem ?? "Não foi possível carregar os indicadores do dia.";
                return;
            }

            if (!agendamentos.Sucesso || agendamentos.Valor is null)
            {
                MensagemErro = agendamentos.Mensagem ?? "Não foi possível carregar os agendamentos do dia.";
                return;
            }

            if (!atividades.Sucesso || atividades.Valor is null)
            {
                MensagemErro = atividades.Mensagem ?? "Não foi possível carregar as atividades recentes.";
                return;
            }

            if (!paraConcluir.Sucesso || paraConcluir.Valor is null)
            {
                MensagemErro = paraConcluir.Mensagem ?? "Não foi possível carregar as OS a concluir.";
                return;
            }

            if (!concluidas.Sucesso || concluidas.Valor is null)
            {
                MensagemErro = concluidas.Mensagem ?? "Não foi possível carregar as OS concluídas hoje.";
                return;
            }

            Indicadores = indicadores.Valor;
            AgendamentosDoDia.Clear();
            OrdensParaAbrir.Clear();
            foreach (var agendamento in agendamentos.Valor)
            {
                AgendamentosDoDia.Add(agendamento);
                if (!agendamento.PossuiOrdemServico
                    && agendamento.Status is StatusAgendamento.Pendente or StatusAgendamento.EmAndamento)
                {
                    OrdensParaAbrir.Add(agendamento);
                }
            }

            OrdensParaConcluir.Clear();
            foreach (var ordem in paraConcluir.Valor)
                OrdensParaConcluir.Add(ordem);

            OrdensConcluidasHoje.Clear();
            foreach (var ordem in concluidas.Valor)
                OrdensConcluidasHoje.Add(ordem);

            OnPropertyChanged(nameof(NenhumAgendamentoParaHoje));
            OnPropertyChanged(nameof(NenhumaOrdemParaAbrir));
            OnPropertyChanged(nameof(NenhumaOrdemParaConcluir));
            OnPropertyChanged(nameof(NenhumaOrdemConcluidaHoje));

            AtividadesRecentes.Clear();
            foreach (var atividade in atividades.Valor)
                AtividadesRecentes.Add(atividade);
        }
        finally
        {
            IsCarregando = false;
        }
    }
}