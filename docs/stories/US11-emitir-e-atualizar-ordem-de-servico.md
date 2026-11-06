# US11 — Emitir e atualizar ordem de serviço

| Campo | Valor |
|---|---|
| Feature | [F04](../features/) |
| Requisitos | RF08 |
| Origem | Documento (UC04 e seção 2.5) |
| Status | A fazer |

## História

**Como** operador,
**quero** emitir uma ordem de serviço a partir de um agendamento, registrando mão de obra e materiais,
**para** calcular o custo do atendimento e manter o histórico financeiro.

## Critérios de aceite

- [ ] Cada agendamento gera no máximo uma OS (índice único em AgendamentoId).
- [ ] A OS registra relatório técnico, mão de obra e materiais com quantidades.
- [ ] Cada ItemOS grava o valor unitário vigente no momento do atendimento, sem ser alterado por mudanças posteriores no cadastro.
- [ ] Valor total = mão de obra + Σ (quantidade × valor unitário).
- [ ] A OS pode ser consultada e atualizada enquanto não estiver concluída.

## Notas técnicas

- Detalhes de implementação ficam na feature F04 (pasta `docs/features`).
- Cada critério deve virar ao menos um teste (automatizado quando for regra de negócio; manual quando for de interface).

## Definição de pronto

- [ ] Critérios de aceite atendidos
- [ ] Testes escritos e passando
- [ ] Interface segue o design system ([03-DESIGN-STARFIELD](../03-DESIGN-STARFIELD.md))
- [ ] Merge na `develop` via pull request
