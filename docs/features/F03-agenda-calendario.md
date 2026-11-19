# F03 — Agenda e calendário

| Campo | Valor |
|---|---|
| Caso de uso | UC03 — Gerenciar agenda e calendário |
| Requisitos | RF05, RF06, RF07 |
| Histórias | US03, US07, US08 |
| Depende de | F06, F00, F01 |
| Branch sugerida | `feature/f03-agenda-calendario` |

## Objetivo

Organizar compromissos em calendário mensal e grade diária, sem permitir sobreposição de horários.

## Telas (estilo Starfield)

- `AgendaView`: calendário mensal à esquerda, grade diária à direita (linhas por hora), barra superior com mês/ano e navegação.
- `AgendamentoFormView`: cliente, descrição, data, hora de início e duração informada em horas inteiras (1–24).
- Selo de status: Pendente (âmbar), Em Andamento (azul), Concluído (verde), sempre acompanhado de texto.

## Entidades envolvidas

- `Agendamento` (Id, ClienteId, DataHoraInicio, duração interna em minutos, DataHoraFim, DescricaoServico, Status)

## Serviços e regras de negócio

- `IAgendaService`: `ListarDiaAsync(data)`, `ListarMesAsync(ano, mes)` (dias com atendimento), `SalvarAsync(agendamento)`, `AlterarStatusAsync(id, status)`, `TemConflitoAsync(inicio, duracao, ignorarId?)`

### Regras

- Conflito: `novo.Inicio < existente.Fim && novo.Fim > existente.Inicio`, ignorando cancelados.
- O término é calculado e **gravado** (`DataHoraFim`) para que a consulta seja traduzida pelo EF Core no SQLite.
- Usar `DateTime` (e não `DateTimeOffset`) pelo mesmo motivo.
- Status inicial: Pendente.
- A interface informa duração em horas inteiras; o serviço converte para minutos para calcular horários e compatibilizar registros já existentes.

### Esboço da verificação de conflito

```csharp
public async Task<Agendamento?> BuscarConflitoAsync(DateTime inicio, int duracaoMin, int? ignorarId = null)
{
    var fim = inicio.AddMinutes(duracaoMin);
    await using var db = await _factory.CreateDbContextAsync();
    return await db.Agendamentos
        .Where(a => a.Status != StatusAgendamento.Cancelado
                 && a.Id != ignorarId
                 && a.DataHoraInicio < fim
                 && a.DataHoraFim > inicio)
        .OrderBy(a => a.DataHoraInicio)
        .FirstOrDefaultAsync();
}
```

## Tarefas técnicas

- [x] Criar `Agendamento` e enum `StatusAgendamento` (Pendente, EmAndamento, Concluido, Cancelado)
- [x] Implementar `AgendaService` + testes (sobreposição, encostado, contido, cancelado, edição do próprio registro)
- [x] Criar calendário mensal (controle `Calendar` do Avalonia ou controle próprio)
- [x] Criar grade diária
- [x] Formulário com mensagem de conflito apontando o compromisso existente

## Definição de pronto

- [x] Todas as histórias da feature concluídas
- [x] Regras de negócio cobertas por testes unitários
- [x] Funciona offline
- [ ] Atividade relevante registrada no feed
