# US05 — Localizar material por busca

| Campo | Valor |
|---|---|
| Feature | [F02](../features/) |
| Requisitos | RF03, RF04 |
| Origem | Pergunta 5 da entrevista |
| Status | Implementada localmente (merge pendente) |

## História

**Como** operador,
**quero** digitar nome, código ou tipo da peça e ver onde ela está guardada,
**para** ir direto à prateleira sem procurar uma a uma.

## Critérios de aceite

- [x] A busca aceita termos parciais de nome, código e categoria.
- [x] O resultado mostra a coordenada em destaque e em tamanho grande (ex.: M4).
- [x] O item encontrado é destacado na matriz visual.
- [x] Quando nada é encontrado, uma mensagem informa isso.

## Notas técnicas

- Detalhes de implementação ficam na feature F02 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [x] Critérios de aceite atendidos
- [x] Testes escritos e passando
- [x] Interface segue o design system Starfield
- [ ] Merge na `develop` via pull request (pendente)
- [ ] Merge na `develop` via pull request
