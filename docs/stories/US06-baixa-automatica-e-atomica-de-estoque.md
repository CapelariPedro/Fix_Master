# US06 — Baixa automática e atômica de estoque

| Campo | Valor |
|---|---|
| Feature | [F04](../features/) |
| Requisitos | RF09 |
| Origem | Pergunta 6 da entrevista |
| Status | A fazer |

## História

**Como** operador,
**quero** que ao concluir um serviço o estoque seja descontado automaticamente,
**para** não anotar manualmente e nunca ter serviço concluído com estoque errado.

## Critérios de aceite

- [ ] Ao concluir a OS, cada ItemOS desconta sua quantidade do ItemEstoque correspondente.
- [ ] Conclusão da OS, baixa de estoque e atualização de status ocorrem em uma única transação: ou tudo acontece, ou nada.
- [ ] Se algum item não tiver estoque suficiente, nada é alterado e o sistema informa qual item falhou.
- [ ] Uma segunda solicitação de conclusão não desconta novamente as quantidades.
- [ ] A quantidade em estoque nunca fica negativa.

## Notas técnicas

- Detalhes de implementação ficam na feature F04 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [ ] Critérios de aceite atendidos
- [ ] Testes escritos e passando
- [ ] Interface segue o design system ([03-DESIGN-STARFIELD](../03-DESIGN-STARFIELD.md))
- [ ] Merge na `develop` via pull request
