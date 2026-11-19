using System.Collections.Generic;

namespace FixMaster.Infrastructure;

public sealed record HorarioAgenda(
    string Hora,
    bool Livre,
    string? TextoOcupacao,
    IReadOnlyList<AgendamentoResumo> Agendamentos)
{
    public bool Ocupado => !Livre;
    public bool TemAgendamentos => Agendamentos.Count > 0;
    public bool MostrarLinha => Ocupado || TemAgendamentos;
    public string TextoDisponibilidade => Livre ? "LIVRE" : "OCUPADO";
    public bool TemTextoOcupacao => !string.IsNullOrEmpty(TextoOcupacao);
}
