using System;
using System.Collections.Generic;

namespace FixMaster.Models;

public sealed class OrdemServico
{
    public int Id { get; set; }
    public int AgendamentoId { get; set; }
    public string RelatorioTecnico { get; set; } = "";
    public decimal MaoDeObra { get; set; }
    public StatusOS Status { get; set; } = StatusOS.Aberta;
    public DateTime DataEmissao { get; set; }
    public DateTime? DataConclusao { get; set; }
    public Agendamento? Agendamento { get; set; }
    public List<ItemOS> Itens { get; set; } = [];
}
