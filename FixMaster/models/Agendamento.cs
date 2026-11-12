using System;

namespace FixMaster.Models;

public sealed class Agendamento
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public DateTime DataHoraInicio { get; set; }
    public int DuracaoMinutos { get; set; }
    public DateTime DataHoraFim { get; set; }
    public string DescricaoServico { get; set; } = "";
    public StatusAgendamento Status { get; set; } = StatusAgendamento.Pendente;
    public Cliente? Cliente { get; set; }
}
