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

- [ ] Ao abrir o app, a tela de login aparece antes de qualquer módulo.
- [ ] Com credenciais válidas, o dashboard é aberto.
- [ ] Com usuário inexistente ou senha incorreta, é exibida uma mensagem genérica (sem indicar qual campo errou) e o operador permanece no login.
- [ ] A senha é armazenada somente como hash, nunca em texto puro.
- [ ] Nenhum módulo pode ser acessado sem autenticação.

## Notas técnicas

- Detalhes de implementação ficam na feature F00 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [ ] Critérios de aceite atendidos
- [ ] Testes escritos e passando
- [ ] Interface segue o design system ([03-DESIGN-STARFIELD](../03-DESIGN-STARFIELD.md))
- [ ] Merge na `develop` via pull request
