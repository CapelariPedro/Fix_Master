# US07 — Calendário mensal e grade diária com status

| Campo | Valor |
|---|---|
| Feature | [F03](../features/) |
| Requisitos | RF05, RF06, RF07 |
| Origem | Pergunta 7 da entrevista |
| Status | Implementada |

## História

**Como** operador,
**quero** um calendário do mês e uma grade com as horas do dia,
**para** ver o movimento do mês e o que falta fazer, o que estou fazendo e o que já terminei.

## Critérios de aceite

- [x] É possível navegar entre os meses.
- [x] Dias com atendimentos são indicados no calendário.
- [x] Selecionar uma data atualiza a grade diária.
- [x] Os status Pendente, Em Andamento e Concluído são distinguíveis por cor e por texto (não apenas por cor).

## Notas técnicas

- Detalhes de implementação ficam na feature F03 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [x] Critérios de aceite atendidos
- [x] Testes escritos e passando
- [x] Interface segue o design system Starfield aplicado no projeto
- [ ] Merge na `develop` via pull request
