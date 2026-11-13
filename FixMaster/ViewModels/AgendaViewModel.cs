using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FixMaster.Infrastructure;
using FixMaster.Models;
using FixMaster.Services.Interfaces;

namespace FixMaster.ViewModels;

public sealed partial class AgendaViewModel : ViewModelBase
{
    private static readonly CultureInfo CulturaPtBr = CultureInfo.GetCultureInfo("pt-BR");
    private readonly IAgendaService _agendaService;
    private readonly IClienteService _clienteService;

    [ObservableProperty]
    private DateTime _dataSelecionada = DateTime.Today;

    [ObservableProperty]
    private DateTime _mesExibido = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    [ObservableProperty]
    private ClienteResumo? _clienteSelecionado;

    [ObservableProperty]
    private string _dataFormulario = "";

    [ObservableProperty]
    private string _horaInicioFormulario = "09:00";

    [ObservableProperty]
    private string _duracaoFormulario = "1";

    [ObservableProperty]
    private string _descricaoFormulario = "";

    [ObservableProperty]
    private string? _mensagemErro;

    [ObservableProperty]
    private bool _isCarregando;

    [ObservableProperty]
    private bool _isFormularioAberto;

    [ObservableProperty]
    private bool _isSalvando;

    [ObservableProperty]
    private int _idAgendamentoEmEdicao;

    public AgendaViewModel(IAgendaService agendaService, IClienteService clienteService)
    {
        _agendaService = agendaService;
        _clienteService = clienteService;
    }

    public ObservableCollection<ClienteResumo> Clientes { get; } = [];
    public ObservableCollection<AgendamentoResumo> Agendamentos { get; } = [];
    public ObservableCollection<DiaCalendario> DiasDoMes { get; } = [];
    public ObservableCollection<HorarioAgenda> Horarios { get; } = [];

    public string TituloMes => MesExibido.ToString("MMMM yyyy", CulturaPtBr).ToUpper(CulturaPtBr);
    public string DataSelecionadaDescricao =>
        DataSelecionada.ToString("dddd, dd 'de' MMMM 'de' yyyy", CulturaPtBr);
    public string TituloFormulario =>
        IdAgendamentoEmEdicao == 0 ? "NOVO ATENDIMENTO" : "EDITAR ATENDIMENTO";
    public bool IsEditando => IdAgendamentoEmEdicao != 0;
    public bool TemAgendamentos => Agendamentos.Count > 0;

    public async Task InicializarAsync() => await CarregarAsync();

    [RelayCommand]
    public async Task CarregarAsync()
    {
        IsCarregando = true;
        MensagemErro = null;

        var clientesTask = _clienteService.ListarAsync(null);
        var mesTask = _agendaService.ListarMesAsync(MesExibido.Year, MesExibido.Month);
        var diaTask = _agendaService.ListarDiaAsync(DataSelecionada);
        await Task.WhenAll(clientesTask, mesTask, diaTask);

        var clientes = await clientesTask;
        var mes = await mesTask;
        var dia = await diaTask;
        if (!clientes.Sucesso || clientes.Valor is null
            || !mes.Sucesso || mes.Valor is null
            || !dia.Sucesso || dia.Valor is null)
        {
            MensagemErro = clientes.Mensagem ?? mes.Mensagem ?? dia.Mensagem
                ?? "Não foi possível carregar a agenda.";
            IsCarregando = false;
            return;
        }

        Clientes.Clear();
        foreach (var cliente in clientes.Valor)
            Clientes.Add(cliente);

        Agendamentos.Clear();
        foreach (var agendamento in dia.Valor)
            Agendamentos.Add(agendamento);

        AtualizarCalendario(mes.Valor);
        AtualizarHorarios();
        OnPropertyChanged(nameof(TemAgendamentos));
        OnPropertyChanged(nameof(DataSelecionadaDescricao));
        OnPropertyChanged(nameof(TituloMes));
        IsCarregando = false;
    }

    [RelayCommand]
    private async Task MesAnteriorAsync()
    {
        MesExibido = MesExibido.AddMonths(-1);
        DataSelecionada = AjustarDiaNoMes(DataSelecionada.Day);
        await CarregarAsync();
    }

    [RelayCommand]
    private async Task ProximoMesAsync()
    {
        MesExibido = MesExibido.AddMonths(1);
        DataSelecionada = AjustarDiaNoMes(DataSelecionada.Day);
        await CarregarAsync();
    }

    [RelayCommand]
    private async Task SelecionarDiaAsync(DiaCalendario dia)
    {
        DataSelecionada = dia.Data;
        MesExibido = new DateTime(dia.Data.Year, dia.Data.Month, 1);
        await CarregarAsync();
    }

    [RelayCommand]
    private void NovoAgendamento()
    {
        IdAgendamentoEmEdicao = 0;
        ClienteSelecionado = Clientes.FirstOrDefault();
        DataFormulario = DataSelecionada.ToString("dd/MM/yyyy", CulturaPtBr);
        HoraInicioFormulario = "09:00";
        DuracaoFormulario = "1";
        DescricaoFormulario = "";
        MensagemErro = null;
        IsFormularioAberto = true;
    }

    [RelayCommand]
    private void EditarAgendamento(AgendamentoResumo agendamento)
    {
        if (agendamento.Status == StatusAgendamento.Cancelado)
            return;

        IdAgendamentoEmEdicao = agendamento.Id;
        ClienteSelecionado = Clientes.FirstOrDefault(cliente =>
            cliente.Id == agendamento.ClienteId);
        DataFormulario = agendamento.DataHoraInicio.ToString("dd/MM/yyyy", CulturaPtBr);
        HoraInicioFormulario = agendamento.DataHoraInicio.ToString("HH:mm");
        DuracaoFormulario = Math.Max(1, (agendamento.DuracaoMinutos + 59) / 60)
            .ToString(CulturaPtBr);
        DescricaoFormulario = agendamento.DescricaoServico;
        MensagemErro = null;
        IsFormularioAberto = true;
    }

    [RelayCommand]
    private void FecharFormulario()
    {
        IsFormularioAberto = false;
        MensagemErro = null;
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (ClienteSelecionado is null)
        {
            MensagemErro = "Selecione um cliente.";
            return;
        }

        if (!DateTime.TryParseExact(
                DataFormulario,
                "dd/MM/yyyy",
                CulturaPtBr,
                DateTimeStyles.None,
                out var data)
            || !TimeSpan.TryParseExact(
                HoraInicioFormulario,
                @"hh\:mm",
                CulturaPtBr,
                TimeSpanStyles.None,
                out var horario)
            || horario >= TimeSpan.FromDays(1))
        {
            MensagemErro = "Informe a data no formato dd/MM/aaaa e o horário no formato HH:mm.";
            return;
        }

        if (!int.TryParse(DuracaoFormulario, NumberStyles.None, CulturaPtBr, out var duracaoHoras)
            || duracaoHoras is < 1 or > 24)
        {
            MensagemErro = "Informe a duração em horas inteiras, entre 1 e 24.";
            return;
        }

        IsSalvando = true;
        MensagemErro = null;
        var resultado = await _agendaService.SalvarAsync(new Agendamento
        {
            Id = IdAgendamentoEmEdicao,
            ClienteId = ClienteSelecionado.Id,
            DataHoraInicio = DateTime.SpecifyKind(data.Date + horario, DateTimeKind.Unspecified),
            DuracaoMinutos = duracaoHoras * 60,
            DescricaoServico = DescricaoFormulario
        });
        IsSalvando = false;

        if (!resultado.Sucesso)
        {
            MensagemErro = resultado.Mensagem ?? "Não foi possível salvar o agendamento.";
            return;
        }

        if (!DateTime.TryParseExact(
                DataFormulario,
                "dd/MM/yyyy",
                CulturaPtBr,
                DateTimeStyles.None,
                out var dataSalva))
            throw new InvalidOperationException("A data do agendamento mudou durante o salvamento.");

        DataSelecionada = dataSalva.Date;
        MesExibido = new DateTime(DataSelecionada.Year, DataSelecionada.Month, 1);
        IsFormularioAberto = false;
        await CarregarAsync();
    }

    [RelayCommand]
    private async Task IniciarAsync(AgendamentoResumo agendamento) =>
        await AlterarStatusAsync(agendamento, StatusAgendamento.EmAndamento);

    [RelayCommand]
    private async Task ConcluirAsync(AgendamentoResumo agendamento) =>
        await AlterarStatusAsync(agendamento, StatusAgendamento.Concluido);

    [RelayCommand]
    private async Task CancelarAsync(AgendamentoResumo agendamento) =>
        await AlterarStatusAsync(agendamento, StatusAgendamento.Cancelado);

    private async Task AlterarStatusAsync(AgendamentoResumo agendamento, StatusAgendamento status)
    {
        MensagemErro = null;
        var resultado = await _agendaService.AlterarStatusAsync(agendamento.Id, status);
        if (!resultado.Sucesso)
        {
            MensagemErro = resultado.Mensagem ?? "Não foi possível alterar o status.";
            return;
        }

        await CarregarAsync();
    }

    private DateTime AjustarDiaNoMes(int dia)
    {
        var ultimoDia = DateTime.DaysInMonth(MesExibido.Year, MesExibido.Month);
        return new DateTime(MesExibido.Year, MesExibido.Month, Math.Min(dia, ultimoDia));
    }

    private void AtualizarCalendario(IReadOnlyList<DateTime> datasComAgendamento)
    {
        var inicioMes = new DateTime(MesExibido.Year, MesExibido.Month, 1);
        var deslocamento = ((int)inicioMes.DayOfWeek + 6) % 7;
        var inicioGrade = inicioMes.AddDays(-deslocamento);
        var datasOcupadas = datasComAgendamento.Select(data => data.Date).ToHashSet();

        DiasDoMes.Clear();
        for (var dia = 0; dia < 42; dia++)
        {
            var data = inicioGrade.AddDays(dia);
            DiasDoMes.Add(new DiaCalendario(
                data,
                data.Day.ToString(CulturaPtBr),
                data.Month == MesExibido.Month,
                datasOcupadas.Contains(data.Date),
                data.Date == DataSelecionada.Date));
        }
    }

    private void AtualizarHorarios()
    {
        Horarios.Clear();
        var inicioDia = DataSelecionada.Date;
        for (var hora = 0; hora < 24; hora++)
        {
            var inicioSlot = inicioDia.AddHours(hora);
            var fimSlot = inicioSlot.AddHours(1);
            var ocupados = Agendamentos
                .Where(agendamento =>
                    agendamento.Status != StatusAgendamento.Cancelado
                    && agendamento.DataHoraInicio < fimSlot
                    && agendamento.DataHoraFim > inicioSlot)
                .ToList();
            var iniciados = Agendamentos
                .Where(agendamento =>
                    agendamento.DataHoraInicio >= inicioSlot
                    && agendamento.DataHoraInicio < fimSlot)
                .ToList();
            var livre = ocupados.Count == 0;
            var textoOcupacao = !livre && iniciados.Count == 0
                ? $"Em atendimento até {ocupados.Max(item => item.DataHoraFim):HH:mm}"
                : null;

            Horarios.Add(new HorarioAgenda($"{hora:00}:00", livre, textoOcupacao, iniciados));
        }
    }
}
