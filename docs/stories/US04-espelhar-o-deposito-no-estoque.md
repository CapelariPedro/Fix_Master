# US04 — Espelhar o depósito no estoque

| Campo | Valor |
|---|---|
| Feature | [F02](../features/) |
| Requisitos | RF04 |
| Origem | Pergunta 4 da entrevista |
| Status | Implementada localmente (merge pendente) |

## História

**Como** operador,
**quero** que o estoque reproduza meu depósito (prateleiras A–G, posições 1–8),
**para** saber exatamente onde cada item está guardado.

## Critérios de aceite

- [x] São aceitas as prateleiras de A a G (7) e as posições de 1 a 8; qualquer outra coordenada é rejeitada com mensagem clara.
- [x] A coordenada é exibida no formato prateleira+posição (ex.: M4).
- [x] Existe uma matriz visual 7×8 que reflete a localização persistida no banco.
- [x] Regra de ocupação definida explicitamente: um item ativo por coordenada, garantido por índice único parcial.

## Notas técnicas

- Detalhes de implementação ficam na feature F02 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [x] Critérios de aceite atendidos
- [x] Testes escritos e passando
- [x] Interface segue o design system Starfield
- [ ] Merge na `develop` via pull request (pendente)
- [ ] Merge na `develop` via pull request
