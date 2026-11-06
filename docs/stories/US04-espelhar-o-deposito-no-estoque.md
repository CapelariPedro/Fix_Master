# US04 — Espelhar o depósito no estoque

| Campo | Valor |
|---|---|
| Feature | [F02](../features/) |
| Requisitos | RF04 |
| Origem | Pergunta 4 da entrevista |
| Status | A fazer |

## História

**Como** operador,
**quero** que o estoque reproduza meu depósito (prateleiras A–X, posições 1–8),
**para** saber exatamente onde cada item está guardado.

## Critérios de aceite

- [ ] São aceitas as prateleiras de A a X (24) e as posições de 1 a 8; qualquer outra coordenada é rejeitada com mensagem clara.
- [ ] A coordenada é exibida no formato prateleira+posição (ex.: M4).
- [ ] Existe uma matriz visual 24×8 que reflete a localização persistida no banco.
- [ ] Regra de ocupação definida explicitamente: um item por coordenada (índice único) ou, se houver mais de um, a regra está documentada.

## Notas técnicas

- Detalhes de implementação ficam na feature F02 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [ ] Critérios de aceite atendidos
- [ ] Testes escritos e passando
- [ ] Interface segue o design system ([03-DESIGN-STARFIELD](../03-DESIGN-STARFIELD.md))
- [ ] Merge na `develop` via pull request
