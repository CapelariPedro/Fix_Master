# US01 — Login local

| Campo | Valor |
|---|---|
| Feature | [F00](../features/) |
| Requisitos | RF01 |
| Origem | Pergunta 1 da entrevista |
| Status | A fazer |

## História

**Como** operador (prestador de serviço),
**quero** informar usuário e senha ao abrir o programa,
**para** impedir que outras pessoas acessem meus dados.

## Critérios de aceite

- [X] Ao abrir o app, a tela de login aparece antes de qualquer módulo.
- [X] Com credenciais válidas, o dashboard é aberto.
- [X] Com usuário inexistente ou senha incorreta, é exibida uma mensagem genérica (sem indicar qual campo errou) e o operador permanece no login.
- [X] A senha é armazenada somente como hash, nunca em texto puro.
- [X] Nenhum módulo pode ser acessado sem autenticação.

## Notas técnicas

- Detalhes de implementação ficam na feature F00 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [X] Critérios de aceite atendidos
- [X] Testes escritos e passando
- [X] Interface segue o design system ([03-DESIGN-STARFIELD](../03-DESIGN-STARFIELD.md))
- [x] Merge na `develop` via pull request
