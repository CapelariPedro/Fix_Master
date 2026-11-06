# US05 — Localizar material por busca

| Campo | Valor |
|---|---|
| Feature | [F02](../features/) |
| Requisitos | RF03, RF04 |
| Origem | Pergunta 5 da entrevista |
| Status | A fazer |

## História

**Como** operador,
**quero** digitar nome, código ou tipo da peça e ver onde ela está guardada,
**para** ir direto à prateleira sem procurar uma a uma.

## Critérios de aceite

- [ ] A busca aceita termos parciais de nome, código e categoria.
- [ ] O resultado mostra a coordenada em destaque e em tamanho grande (ex.: M4).
- [ ] O item encontrado é destacado na matriz visual.
- [ ] Quando nada é encontrado, uma mensagem informa isso.

## Notas técnicas

- Detalhes de implementação ficam na feature F02 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [ ] Critérios de aceite atendidos
- [ ] Testes escritos e passando
- [ ] Interface segue o design system ([03-DESIGN-STARFIELD](../03-DESIGN-STARFIELD.md))
- [ ] Merge na `develop` via pull request
