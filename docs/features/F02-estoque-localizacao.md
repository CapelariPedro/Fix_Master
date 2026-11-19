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
- `ShelfMatrix` (controle próprio): grade 7×8 (A–G × 1–8); células ocupadas preenchidas, livres apenas com contorno, item pesquisado em destaque em azul claro.
- `ItemEstoqueFormView`: código automático pelo tipo selecionado, nome, descrição, tipo, quantidade, preço unitário, prateleira e posição.

## Entidades envolvidas

- `ItemEstoque` (Id, Codigo, Nome, Descricao, Categoria, Quantidade, PrecoUnitario, Prateleira, Posicao, Ativo)

## Serviços e regras de negócio

- `IEstoqueService`: CRUD, `BuscarAsync(termo)`, `ObterMatrizAsync()`
- `LocalizacaoValidator`: prateleira ∈ [A..G], posição ∈ [1..8]; normaliza para maiúscula.

### Regras

- Índice único em (Prateleira, Posicao) quando ativo — um item por coordenada.
- Índice único em Codigo.
- Código gerado como PREFIXO-NÚMERO aleatório de seis dígitos, com prefixo por tipo: Peças (PEC), Ferramentas (FER), Insumos (INS), Equipamentos (EQP) e Outros (OUT); a geração verifica unicidade no banco.
- `CHECK` no banco: Quantidade >= 0, Posicao entre 1 e 8, Prateleira entre 'A' e 'G'.
- A matriz visual lê sempre do banco (não de cache).
- A migração reduz a faixa de A–X para A–G e realoca materiais ativos de H–X para as primeiras posições livres; materiais inativos são movidos para A1. As localizações originais são preservadas para permitir rollback.

## Tarefas técnicas

- [x] Criar `ItemEstoque` e configuração EF com índices e CHECKs
- [x] Implementar `LocalizacaoValidator` + testes (A, G, H, X, Y, 0, 9, minúsculas)
- [x] Implementar `EstoqueService` e busca
- [x] Criar controle `ShelfMatrix`
- [x] Tela de busca com destaque grande da coordenada
- [x] Cadastro e edição em modal com código, nome, descrição, categoria, quantidade, preço unitário e localização; remoção lógica

## Definição de pronto

- [x] Todas as histórias US04 e US05 concluídas
- [x] Regras de negócio cobertas por testes unitários
- [x] Funciona offline, com persistência local em SQLite
- [x] Atividade no feed não aplicável: a aplicação ainda não possui feed de atividades
