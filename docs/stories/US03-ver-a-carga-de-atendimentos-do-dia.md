# US03 — Ver a carga de atendimentos do dia

| Campo | Valor |
|---|---|
| Feature | [F03](../features/) |
| Requisitos | RF06, RF07 |
| Origem | Pergunta 3 da entrevista |
| Status | A fazer |

## História

**Como** operador,
**quero** ver todos os atendimentos do dia (de 3 a 8) em ordem de horário,
**para** saber por onde começar e não me embolar durante o dia.

## Critérios de aceite

- [ ] A grade diária lista os compromissos do dia selecionado ordenados por horário.
- [ ] Cada compromisso mostra início–término, cliente, descrição do serviço e status.
- [ ] A grade comporta pelo menos 8 compromissos no mesmo dia com boa legibilidade.
- [ ] Horários livres são visualmente distintos dos ocupados.

## Notas técnicas

- Detalhes de implementação ficam na feature F03 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [ ] Critérios de aceite atendidos
- [ ] Testes escritos e passando
- [ ] Interface segue o design system ([03-DESIGN-STARFIELD](../03-DESIGN-STARFIELD.md))
- [ ] Merge na `develop` via pull request
