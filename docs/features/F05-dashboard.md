# F05 — Dashboard operacional

| Campo | Valor |
|---|---|
| Caso de uso | UC05 — Visualizar o dashboard operacional |
| Requisitos | RF10 |
| Histórias | US09 |
| Depende de | F00, F03, F04 (pode começar com dados de teste) |
| Branch sugerida | `feature/f05-dashboard` |

## Objetivo

Mostrar ao abrir o app o resumo do dia e as atividades recentes.

## Telas (estilo Starfield)

- `DashboardView`: grade de 4 cards (MARCADOS, PENDENTES, EM ANDAMENTO, CONCLUÍDOS) com número grande e rótulo em caixa alta.
- Agendamentos do dia em ordem crescente de horário e uma lista dos agendamentos pendentes ou em andamento sem OS vinculada.
- Painel ATIVIDADES RECENTES: lista vertical com horário, ícone simples e descrição, no estilo de log de missão.

## Entidades envolvidas

- `AtividadeRecente` (Id, DataHora, Tipo, Descricao, ReferenciaId) — nova entidade para o feed
- Leitura de `Agendamento` e `OrdemServico`

## Serviços e regras de negócio

- `IDashboardService`: `ObterIndicadoresDoDiaAsync()`, `ObterAtividadesRecentesAsync(n)`
- `IAtividadeService.RegistrarAsync(tipo, descricao, refId)` chamado pelos demais serviços.

### Regras

- Indicadores calculados dos dados persistidos, por status.
- Feed ordenado por DataHora decrescente, limitado (ex.: 20).
- Dashboard é a tela inicial após o login.

## Tarefas técnicas

- [X] Criar `AtividadeRecente` e configuração EF
- [X] Chamar o registro de atividade em Agenda, OS e Clientes
- [X] Implementar `DashboardService`
- [X] Criar `DashboardViewModel` e `DashboardView`

## Definição de pronto

- [X] Todas as histórias da feature concluídas
- [X] Regras de negócio cobertas por testes unitários
- [X] Funciona offline
