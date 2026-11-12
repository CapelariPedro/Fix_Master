# US08 — Impedir conflitos de horário

| Campo | Valor |
|---|---|
| Feature | [F03](../features/) |
| Requisitos | RF07 |
| Origem | Pergunta 8 da entrevista |
| Status | Implementada |

## História

**Como** operador,
**quero** que o sistema confira conflitos antes de salvar um novo horário, considerando a duração,
**para** nunca marcar dois clientes na mesma hora.

## Critérios de aceite

- [x] Há conflito quando novoInício < fimExistente E novoFim > inícioExistente.
- [x] Somente agendamentos que bloqueiam a agenda entram na verificação (cancelados não bloqueiam).
- [x] Em caso de conflito, o agendamento não é gravado e o compromisso conflitante é exibido.
- [x] O operador pode escolher outro horário sem perder os dados já digitados.
- [x] Agendamentos consecutivos (término = início do próximo) são permitidos.
- [x] Ao editar um agendamento, ele não conflita consigo mesmo.

## Notas técnicas

- Detalhes de implementação ficam na feature F03 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [x] Critérios de aceite atendidos
- [x] Testes escritos e passando
- [x] Interface segue o design system Starfield aplicado no projeto
- [ ] Merge na `develop` via pull request
