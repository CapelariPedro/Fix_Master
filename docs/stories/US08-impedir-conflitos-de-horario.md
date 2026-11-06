# US08 — Impedir conflitos de horário

| Campo | Valor |
|---|---|
| Feature | [F03](../features/) |
| Requisitos | RF07 |
| Origem | Pergunta 8 da entrevista |
| Status | A fazer |

## História

**Como** operador,
**quero** que o sistema confira conflitos antes de salvar um novo horário, considerando a duração,
**para** nunca marcar dois clientes na mesma hora.

## Critérios de aceite

- [ ] Há conflito quando novoInício < fimExistente E novoFim > inícioExistente.
- [ ] Somente agendamentos que bloqueiam a agenda entram na verificação (cancelados não bloqueiam).
- [ ] Em caso de conflito, o agendamento não é gravado e o compromisso conflitante é exibido.
- [ ] O operador pode escolher outro horário sem perder os dados já digitados.
- [ ] Agendamentos consecutivos (término = início do próximo) são permitidos.
- [ ] Ao editar um agendamento, ele não conflita consigo mesmo.

## Notas técnicas

- Detalhes de implementação ficam na feature F03 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [ ] Critérios de aceite atendidos
- [ ] Testes escritos e passando
- [ ] Interface segue o design system ([03-DESIGN-STARFIELD](../03-DESIGN-STARFIELD.md))
- [ ] Merge na `develop` via pull request
