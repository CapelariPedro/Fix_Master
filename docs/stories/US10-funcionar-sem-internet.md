# US10 — Funcionar sem internet

| Campo | Valor |
|---|---|
| Feature | [F06](../features/) |
| Requisitos | RNF03, RNF09 |
| Origem | Pergunta 10 da entrevista |
| Status | A fazer |

## História

**Como** operador,
**quero** que o sistema funcione sem internet,
**para** trabalhar no depósito e em qualquer lugar sem depender de conexão.

## Critérios de aceite

- [ ] Nenhuma funcionalidade essencial realiza chamadas de rede.
- [ ] Os dados ficam no arquivo local fixmaster.db3.
- [ ] Fontes, ícones e imagens são embutidos no aplicativo (sem CDN).
- [ ] Teste manual com a rede desligada cobre cadastro, agenda, OS e estoque.

## Notas técnicas

- Detalhes de implementação ficam na feature F06 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [ ] Critérios de aceite atendidos
- [ ] Testes escritos e passando
- [ ] Interface segue o design system ([03-DESIGN-STARFIELD](../03-DESIGN-STARFIELD.md))
- [ ] Merge na `develop` via pull request
