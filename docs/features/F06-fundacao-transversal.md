# F06 — Fundação e requisitos transversais

| Campo | Valor |
|---|---|
| Caso de uso | Todos (base técnica) |
| Requisitos | RNF01 a RNF10 |
| Histórias | US10 |
| Depende de | — |
| Branch sugerida | `feature/f06-fundacao-transversal` |

## Objetivo

Preparar a base técnica sobre a qual as demais features são construídas.

## Telas (estilo Starfield)

- `MainWindow` como casca: hospeda `LoginView` e, após autenticar, `ShellView` (menu lateral + área de conteúdo + barra inferior de atalhos).
- Tema Starfield (`Themes/`) aplicado globalmente.

## Entidades envolvidas

- `AppDbContext` com todos os `DbSet`
- Todas as entidades (criadas gradualmente por feature)

## Serviços e regras de negócio

- Injeção de dependência (`Microsoft.Extensions.DependencyInjection`)
- `IDbContextFactory<AppDbContext>` registrado — contextos curtos, um por operação
- Serviço de navegação (`INavigationService`) — troca de página no Shell
- Caminho do banco: pasta de dados do usuário (`Environment.SpecialFolder.LocalApplicationData`/FixMaster/fixmaster.db3)
- Migrations aplicadas na inicialização (`Database.Migrate()`)

### Regras

- Views sem lógica de negócio; ViewModels não acessam `DbContext` diretamente (RNF01).
- Sem chamadas de rede; fontes e ícones embutidos (RNF09).
- Chaves, restrições e transações no banco (RNF06).
- Meta de desempenho < 1 s para operações comuns, medida em teste documentado (RNF04).

## Tarefas técnicas

- [ ] Instalar pacotes NuGet (ver `00-COMO-INICIAR.md`)
- [ ] Criar `AppDbContext` e a migração `Inicial`
- [ ] Configurar DI e `IDbContextFactory`
- [ ] Importar `Themes/Colors.axaml` e `StarfieldStyles.axaml` no `App.axaml`
- [ ] Criar `ShellView` com menu lateral e navegação
- [ ] Criar projeto `FixMaster.Tests` (xUnit)
- [ ] Atualizar `README.md` com requisitos e instruções de execução
- [ ] Configurar GitHub: board, issues a partir destes arquivos, acesso do tutor

## Definição de pronto

- [ ] Todas as histórias da feature concluídas
- [ ] Regras de negócio cobertas por testes unitários
- [ ] Funciona offline
- [ ] Atividade relevante registrada no feed (quando aplicável)
