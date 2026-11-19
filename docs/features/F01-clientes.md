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

- Painel de clientes integrado à tela principal: busca no topo, lista à esquerda com seleção destacada e detalhes à direita; disponível para operadores e administradores.
- Formulário em modal com alternância entre pessoa física e empresa, seguindo os campos e estilos Starfield já usados no sistema.
- Administradores alternam entre o painel de clientes e a lista de usuários sem perder as funções administrativas existentes.

## Entidades envolvidas

- `Cliente` (Id, Nome, Documento CPF/CNPJ, Telefone, CEP opcional, Endereco, EhEmpresa, Ativo)

## Serviços e regras de negócio

- `IClienteService`: `ListarAsync(filtro)`, `ObterAsync(id)`, `SalvarAsync(cliente)`, `ExcluirOuInativarAsync(id)`
- `DocumentoValidator`: valida CPF e CNPJ (dígitos verificadores).

### Regras

- Documento único (índice único no banco + verificação no serviço).
- Validação antes da persistência (tipo de pessoa e documento).
- CPF é armazenado somente com dígitos; CNPJ pode conter letras e números nas primeiras 12 posições e mantém dois dígitos verificadores numéricos. Ambos são exibidos com máscara e normalizados em maiúsculas.
- O dígito verificador do CNPJ usa módulo 11 e os valores ASCII menos 48 para letras e números, conforme o formato alfanumérico da Receita Federal.
- CEP é opcional, aceita oito dígitos, recebe máscara `00000-000` durante a digitação e é preenchido manualmente, sem consulta externa. Nome e endereço preservam acentos e caracteres especiais.
- A busca inclui CEP quando o filtro contém apenas números, espaços, pontos ou hífens, evitando interpretar trechos numéricos de documentos alfanuméricos como CEP.
- Exclusão: cliente sem vínculos é removido; se uma chave estrangeira impedir a remoção por haver agendamento/OS, o cliente é inativado (`Ativo = false`) e deixa de aparecer na lista ativa.

## Tarefas técnicas

- [x] Criar `Cliente` e configuração EF
- [x] Implementar `DocumentoValidator` + testes (CPF/CNPJ válidos e inválidos)
- [x] Implementar `ClienteService`
- [x] Criar view model e telas integradas ao painel
- [x] Busca por nome/telefone/documento
- [x] CEP opcional com máscara e preenchimento manual

## Definição de pronto

- [x] História US02 concluída
- [x] Regras de negócio cobertas por testes automatizados
- [x] Funciona offline (persistência SQLite local)
- [ ] Atividade relevante registrada no feed (quando aplicável)
