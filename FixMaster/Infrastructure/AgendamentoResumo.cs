using System;
using FixMaster.Models;

namespace FixMaster.Infrastructure;

public sealed record AgendamentoResumo(
    int Id,
    int ClienteId,
    string ClienteNome,
    DateTime DataHoraInicio,
    DateTime DataHoraFim,
    int DuracaoMinutos,
    string DescricaoServico,
    StatusAgendamento Status,
    bool PossuiOrdemServico = false)
{
    public string Horario =>
        $"{DataHoraInicio:HH:mm}–{DataHoraFim:HH:mm}";

    public string StatusDescricao => Status switch
    {
        StatusAgendamento.Pendente => "PENDENTE",
        StatusAgendamento.EmAndamento => "EM ANDAMENTO",
        StatusAgendamento.Concluido => "CONCLUÍDO",
        StatusAgendamento.Cancelado => "CANCELADO",
        _ => "DESCONHECIDO"
    };

    public bool IsPendente => Status == StatusAgendamento.Pendente;
    public bool IsEmAndamento => Status == StatusAgendamento.EmAndamento;
    public bool IsConcluido => Status == StatusAgendamento.Concluido;
    public bool IsCancelado => Status == StatusAgendamento.Cancelado;
    public bool IsEditavel => Status != StatusAgendamento.Cancelado && !PossuiOrdemServico;
    public bool IsCancelavel =>
        Status is StatusAgendamento.Pendente or StatusAgendamento.EmAndamento
        && !PossuiOrdemServico;
    public bool IsConcluivel =>
        Status == StatusAgendamento.EmAndamento && !PossuiOrdemServico;
}
