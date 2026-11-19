using System;

namespace FixMaster.Infrastructure;

public sealed record OrdemServicoResumo(
    int Id,
    int AgendamentoId,
    string ClienteNome,
    string DescricaoServico,
    DateTime DataHoraAtendimento,
    decimal Total);