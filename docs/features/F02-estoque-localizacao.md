# F02 — Estoque e localização física

| Campo | Valor |
|---|---|
| Caso de uso | UC02 — Gerenciar estoque e localização física |
| Requisitos | RF03, RF04 |
| Histórias | US04, US05 |
| Depende de | F06, F00 |
| Branch sugerida | `feature/f02-estoque-localizacao` |

## Objetivo

Controlar materiais e ferramentas e localizá-los por coordenada no depósito.

## Telas (estilo Starfield)

- `EstoqueView`: lista de itens + busca + **resultado de localização em fonte grande** (ex.: `M4`).
- `ShelfMatrix` (controle próprio): grade 24×8 (A–X × 1–8); células ocupadas preenchidas, livres apenas com contorno, item pesquisado em destaque com a cor de acento.
- `ItemEstoqueFormView`: nome, código, categoria, quantidade, prateleira, posição.

## Entidades envolvidas

- `ItemEstoque` (Id, Codigo, Nome, Descricao, Categoria, Quantidade, PrecoUnitario, Prateleira, Posicao, Ativo)

## Serviços e regras de negócio

- `IEstoqueService`: CRUD, `BuscarAsync(termo)`, `ObterMatrizAsync()`
- `LocalizacaoValidator`: prateleira ∈ [A..X], posição ∈ [1..8]; normaliza para maiúscula.

### Regras

- Índice único em (Prateleira, Posicao) quando ativo — um item por coordenada.
- Índice único em Codigo.
- `CHECK` no banco: Quantidade >= 0, Posicao entre 1 e 8, Prateleira entre 'A' e 'X'.
- A matriz visual lê sempre do banco (não de cache).

## Tarefas técnicas

- [ ] Criar `ItemEstoque` e configuração EF com índices e CHECKs
- [ ] Implementar `LocalizacaoValidator` + testes (A, X, Y, 0, 9, minúsculas)
- [ ] Implementar `EstoqueService` e busca
- [ ] Criar controle `ShelfMatrix`
- [ ] Tela de busca com destaque grande da coordenada

## Definição de pronto

- [ ] Todas as histórias da feature concluídas
- [ ] Regras de negócio cobertas por testes unitários
- [ ] Funciona offline
- [ ] Atividade relevante registrada no feed (quando aplicável)
