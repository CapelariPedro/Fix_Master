# US07 — Calendário mensal e grade diária com status

| Campo | Valor |
|---|---|
| Feature | [F03](../features/) |
| Requisitos | RF05, RF06, RF07 |
| Origem | Pergunta 7 da entrevista |
| Status | A fazer |

## História

**Como** operador,
**quero** um calendário do mês e uma grade com as horas do dia,
**para** ver o movimento do mês e o que falta fazer, o que estou fazendo e o que já terminei.

## Critérios de aceite

- [ ] É possível navegar entre os meses.
- [ ] Dias com atendimentos são indicados no calendário.
- [ ] Selecionar uma data atualiza a grade diária.
- [ ] Os status Pendente, Em Andamento e Concluído são distinguíveis por cor e por texto (não apenas por cor).

## Notas técnicas

- Detalhes de implementação ficam na feature F03 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [ ] Critérios de aceite atendidos
- [ ] Testes escritos e passando
- [ ] Interface segue o design system ([03-DESIGN-STARFIELD](../03-DESIGN-STARFIELD.md))
- [ ] Merge na `develop` via pull request
