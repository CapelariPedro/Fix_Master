# US03 — Ver a carga de atendimentos do dia

| Campo | Valor |
|---|---|
| Feature | [F03](../features/) |
| Requisitos | RF06, RF07 |
| Origem | Pergunta 3 da entrevista |
| Status | Implementada |

## História

**Como** operador,
**quero** ver todos os atendimentos do dia (de 3 a 8) em ordem de horário,
**para** saber por onde começar e não me embolar durante o dia.

## Critérios de aceite

- [x] A grade diária lista os compromissos do dia selecionado ordenados por horário.
- [x] Cada compromisso mostra início–término, cliente, descrição do serviço e status.
- [x] A grade comporta pelo menos 8 compromissos no mesmo dia com boa legibilidade.
- [x] Horários livres são visualmente distintos dos ocupados.

## Notas técnicas

- Detalhes de implementação ficam na feature F03 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [x] Critérios de aceite atendidos
- [x] Testes escritos e passando
- [x] Interface segue o design system Starfield aplicado no projeto
- [ ] Merge na `develop` via pull request
