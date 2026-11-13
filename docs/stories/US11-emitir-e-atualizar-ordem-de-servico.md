# US11 — Emitir e atualizar ordem de serviço

| Campo | Valor |
|---|---|
| Feature | [F04](../features/) |
| Requisitos | RF08 |
| Origem | Documento (UC04 e seção 2.5) |
| Status | Implementada |

## História

**Como** operador,
**quero** emitir uma ordem de serviço a partir de um agendamento, registrando mão de obra e materiais,
**para** calcular o custo do atendimento e manter o histórico financeiro.

## Critérios de aceite

- [x] Cada agendamento gera no máximo uma OS (índice único em AgendamentoId).
- [x] A OS registra relatório técnico, mão de obra e materiais com quantidades.
- [x] Cada ItemOS grava o valor unitário vigente no momento da inclusão, sem ser alterado por mudanças posteriores no cadastro.
- [x] Valor total = mão de obra + Σ (quantidade × valor unitário).
- [x] A OS pode ser consultada e atualizada enquanto não estiver concluída.

## Notas técnicas

- Detalhes de implementação ficam na feature F04 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [x] Critérios de aceite atendidos
- [x] Testes escritos e passando
- [x] Interface segue o design system ([03-DESIGN-STARFIELD](../03-DESIGN-STARFIELD.md))
- [ ] Merge na `develop` via pull request
