# US02 — Cadastrar clientes PF e PJ

| Campo | Valor |
|---|---|
| Feature | [F01](../features/) |
| Requisitos | RF02 |
| Origem | Pergunta 2 da entrevista |
| Status | Implementada |

## História

**Como** operador,
**quero** cadastrar pessoas físicas e empresas com nome, telefone, endereço, CEP opcional e documento,
**para** encontrar o cliente depois com facilidade.

## Critérios de aceite

- [x] É possível cadastrar cliente pessoa física (CPF) e pessoa jurídica (CNPJ).
- [x] O documento é validado e deve ser único.
- [x] É possível consultar por nome, telefone ou documento.
- [x] CEP é opcional, recebe máscara automática e o endereço é informado manualmente.
- [x] É possível atualizar os dados de um cliente.
- [x] Cliente sem vínculos pode ser excluído; com agendamento/OS vinculado, a exclusão é substituída por inativação (soft delete).
- [x] Mensagens de validação são claras e indicam o campo com problema (RNF08).

## Notas técnicas

- Detalhes de implementação ficam na feature F01 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [x] Critérios de aceite atendidos
- [x] Testes escritos e passando
- [x] Interface segue o aspecto Starfield atual
- [ ] Merge na `develop` via pull request
