# US06 — Baixa automática e atômica de estoque

| Campo | Valor |
|---|---|
| Feature | [F04](../features/) |
| Requisitos | RF09 |
| Origem | Pergunta 6 da entrevista |
| Status | Implementada |

## História

**Como** operador,
**quero** que ao concluir um serviço o estoque seja descontado automaticamente,
**para** não anotar manualmente e nunca ter serviço concluído com estoque errado.

## Critérios de aceite

- [x] Ao concluir a OS, cada ItemOS desconta sua quantidade do ItemEstoque correspondente.
- [x] Conclusão da OS, baixa de estoque e atualização de status ocorrem em uma única transação: ou tudo acontece, ou nada.
- [x] Se algum item não tiver estoque suficiente, nada é alterado e o sistema informa qual item falhou.
- [x] Uma segunda solicitação de conclusão não desconta novamente as quantidades.
- [x] A quantidade em estoque nunca fica negativa.

## Notas técnicas

- Detalhes de implementação ficam na feature F04 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [x] Critérios de aceite atendidos
- [x] Testes escritos e passando
- [x] Interface segue o design system ([03-DESIGN-STARFIELD](../03-DESIGN-STARFIELD.md))
- [ ] Merge na `develop` via pull request
