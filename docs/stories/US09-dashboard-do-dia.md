# US09 — Dashboard do dia

| Campo | Valor |
|---|---|
| Feature | [F05](../features/) |
| Requisitos | RF10 |
| Origem | Pergunta 9 da entrevista |
| Status | A fazer |

## História

**Como** operador,
**quero** ver logo ao abrir o programa como está meu dia,
**para** acompanhar o andamento sem navegar por vários módulos.

## Critérios de aceite

- [ ] Cards mostram: total de serviços marcados, pendentes, em andamento e concluídos do dia.
- [ ] Uma lista (feed) mostra as atividades recentes, da mais recente para a mais antiga.
- [ ] Os números vêm dos dados persistidos, respeitando os status definidos.
- [ ] O dashboard reflete a conclusão de uma OS ao ser reaberto/atualizado.

## Notas técnicas

- Detalhes de implementação ficam na feature F05 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [ ] Critérios de aceite atendidos
- [ ] Testes escritos e passando
- [ ] Interface segue o design system ([03-DESIGN-STARFIELD](../03-DESIGN-STARFIELD.md))
- [ ] Merge na `develop` via pull request
