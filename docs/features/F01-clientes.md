# F01 — Gestão de clientes

| Campo | Valor |
|---|---|
| Caso de uso | UC01 — Gerenciar clientes |
| Requisitos | RF02 |
| Histórias | US02 |
| Depende de | F06, F00 |
| Branch sugerida | `feature/f01-clientes` |

## Objetivo

Cadastrar, consultar, atualizar e excluir clientes pessoa física e jurídica.

## Telas (estilo Starfield)

- `ClientesView`: lista à esquerda (linha selecionada em destaque claro, estilo menu do jogo) e painel de detalhes à direita; campo de busca no topo.
- `ClienteFormView`: formulário com alternância PESSOA FÍSICA / EMPRESA.

## Entidades envolvidas

- `Cliente` (Id, Nome, Documento CPF/CNPJ, Telefone, Endereco, EhEmpresa, Ativo)

## Serviços e regras de negócio

- `IClienteService`: `ListarAsync(filtro)`, `ObterAsync(id)`, `SalvarAsync(cliente)`, `ExcluirOuInativarAsync(id)`
- `DocumentoValidator`: valida CPF e CNPJ (dígitos verificadores).

### Regras

- Documento único (índice único no banco + verificação no serviço).
- Validação antes da persistência (tipo de pessoa e documento).
- Exclusão: se houver agendamento/OS, inativar (Ativo = false) ou bloquear com mensagem.

## Tarefas técnicas

- [ ] Criar `Cliente` e configuração EF
- [ ] Implementar `DocumentoValidator` + testes (CPF/CNPJ válidos e inválidos)
- [ ] Implementar `ClienteService`
- [ ] Criar `ClientesViewModel` e telas
- [ ] Busca por nome/telefone/documento

## Definição de pronto

- [ ] Todas as histórias da feature concluídas
- [ ] Regras de negócio cobertas por testes unitários
- [ ] Funciona offline
- [ ] Atividade relevante registrada no feed (quando aplicável)
