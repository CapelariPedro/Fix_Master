# F00 — Login local

| Campo | Valor |
|---|---|
| Caso de uso | UC00 — Realizar Login Local |
| Requisitos | RF01, RNF06 |
| Histórias | US01 |
| Depende de | F06 (fundação) |
| Branch sugerida | `feature/f00-login-local` |

## Objetivo

Controlar o acesso ao sistema com credenciais armazenadas de forma segura no banco local.

## Telas (estilo Starfield)

- `LoginView`: painel central translúcido sobre fundo escuro, campos USUÁRIO e SENHA, botão ENTRAR (cor de destaque) e área de mensagem de erro.
- Atalhos exibidos na barra inferior (ex.: [ENTER] ENTRAR).

## Entidades envolvidas

- `Usuario` (Id, Username, SenhaHash)

## Serviços e regras de negócio

- `IAuthService.AutenticarAsync(usuario, senha) : bool`
- `PasswordHasher`: PBKDF2 com sal aleatório (`Rfc2898DeriveBytes.Pbkdf2`, já embutido no .NET) — armazenar sal + hash no campo SenhaHash.
- `DbSeeder`: no primeiro uso (tabela Usuarios vazia), exibir tela de criação do operador, evitando senha padrão no repositório.

### Regras

- Mensagem de erro única para usuário inexistente e senha errada.
- Comparação de hash em tempo constante (`CryptographicOperations.FixedTimeEquals`).
- Após autenticar, `MainViewModel.CurrentScreen` troca de `LoginViewModel` para `ShellViewModel`.

## Tarefas técnicas

- [X] Criar `Usuario` e configuração EF (índice único em Username)
- [X] Implementar `PasswordHasher` + testes
- [X] Implementar `AuthService`
- [X] Criar `LoginViewModel` e `LoginView`
- [X] Fluxo de primeiro acesso (criar operador)
- [X] Troca de tela Login → Shell

## DSefinição de pronto

- [X] Todas as histórias da feature concluídas
- [X] Regras de negócio cobertas por testes unitários
- [X] Funciona offline
- [X] Atividade relevante registrada no feed (quando aplicável)
