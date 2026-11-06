# US02 — Cadastrar clientes PF e PJ

| Campo | Valor |
|---|---|
| Feature | [F01](../features/) |
| Requisitos | RF02 |
| Origem | Pergunta 2 da entrevista |
| Status | A fazer |

## História

**Como** operador,
**quero** cadastrar pessoas físicas e empresas com nome, telefone, endereço e documento,
**para** encontrar o cliente depois com facilidade.

## Critérios de aceite

- [ ] É possível cadastrar cliente pessoa física (CPF) e pessoa jurídica (CNPJ).
- [ ] O documento é validado e deve ser único.
- [ ] É possível consultar por nome, telefone ou documento.
- [ ] É possível atualizar os dados de um cliente.
- [ ] Cliente sem vínculos pode ser excluído; com agendamento/OS vinculado, a exclusão é bloqueada ou substituída por inativação (soft delete).
- [ ] Mensagens de validação são claras e indicam o campo com problema (RNF08).

## Notas técnicas

- Detalhes de implementação ficam na feature F01 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [ ] Critérios de aceite atendidos
- [ ] Testes escritos e passando
- [ ] Interface segue o design system ([03-DESIGN-STARFIELD](../03-DESIGN-STARFIELD.md))
- [ ] Merge na `develop` via pull request
