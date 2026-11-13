using System;

namespace FixMaster.Models;

public sealed class AtividadeRecente
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; }
    public string Tipo { get; set; } = "";
    public string Descricao { get; set; } = "";
    public int? ReferenciaId { get; set; }
}
