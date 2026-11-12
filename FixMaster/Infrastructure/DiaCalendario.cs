using System;

namespace FixMaster.Infrastructure;

public sealed record DiaCalendario(
    DateTime Data,
    string Numero,
    bool DoMesExibido,
    bool TemAgendamentos,
    bool Selecionado)
{
    public double Opacidade => DoMesExibido ? 1 : 0.35;
}
