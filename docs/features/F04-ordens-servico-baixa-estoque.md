# F04 — Ordens de serviço e baixa de estoque

| Campo | Valor |
|---|---|
| Caso de uso | UC04 — Emitir e concluir ordens de serviço |
| Requisitos | RF08, RF09, RNF06 |
| Histórias | US06, US11 |
| Depende de | F01, F02, F03 |
| Branch sugerida | `feature/f04-ordens-servico` |

## Objetivo

Registrar serviços executados, calcular custos e, ao concluir, baixar o estoque e atualizar o status de forma atômica.

## Telas (estilo Starfield)

- `OrdensServicoView`: lista de OS + painel de detalhes (relatório técnico, mão de obra, materiais, total).
- Formulário da OS: seleção de um agendamento não cancelado e ainda sem OS, relatório técnico, mão de obra e adição/remoção de materiais (busca no estoque + quantidade inteira).
- Botão **CONCLUIR OS** em destaque; confirmação antes de executar.

## Entidades envolvidas

- `OrdemServico` (Id, AgendamentoId único, RelatorioTecnico, MaoDeObra em reais, Status, DataEmissao, DataConclusao)
- `ItemOS` (Id, OrdemServicoId, ItemEstoqueId, QuantidadeUtilizada inteira, ValorUnitario em reais)
- `ItemEstoque`, `Agendamento` (atualizados na conclusão)
- `AtividadeRecente` (registro local da conclusão para o feed operacional)

As relações são protegidas por chaves estrangeiras: a OS referencia um agendamento (um-para-um), cada item referencia uma OS e um material. A exclusão de materiais vinculados é restrita; os itens são removidos em cascata quando a OS é removida.

## Serviços e regras de negócio

- `IOrdemServicoService`: `CriarAsync`, `AtualizarAsync`, `ObterAsync`, `ConcluirAsync(osId)`
- Valor total = MaoDeObra + Σ (QuantidadeUtilizada × ValorUnitario).

### Regras

- Conclusão em **uma transação**: verificar status → verificar estoque → baixar → atualizar status da OS e do agendamento → registrar atividade.
- Idempotência: OS já concluída não baixa estoque novamente.
- Estoque insuficiente: nenhuma alteração; mensagem indica o item.
- Com uma OS vinculada, o agendamento não pode ser editado, cancelado ou concluído diretamente pela agenda; ele pode ser iniciado e a conclusão é feita pela OS.
- `ValorUnitario` é copiado do cadastro no momento da inclusão do item e permanece imutável para materiais já vinculados, inclusive quando a OS é atualizada.
- Valores monetários são `decimal`, armazenados no SQLite como texto para evitar perda de precisão; a interface aceita e apresenta reais com centavos.
- A duração do agendamento é informada em horas inteiras (1–24). O código de agenda converte para minutos somente internamente para manter os cálculos de horário e preservar durações já gravadas.
- Atenção ao tipo `decimal`: o provedor SQLite do EF Core não agrega decimais no servidor. Opção segura: guardar valores em centavos (`long`) com um `ValueConverter`, ou calcular totais em memória.

### Esboço

```csharp
public async Task<Resultado> ConcluirAsync(int osId)
{
    await using var db = await _factory.CreateDbContextAsync();
    await using var tx = await db.Database.BeginTransactionAsync();

    var os = await db.OrdensServico
        .Include(o => o.Agendamento)
        .Include(o => o.Itens).ThenInclude(i => i.ItemEstoque)
        .FirstAsync(o => o.Id == osId);

    if (os.Status == StatusOS.Concluida)
        return Resultado.Falha("Esta OS já foi concluída.");

    foreach (var item in os.Itens)
    {
        if (item.ItemEstoque.Quantidade < item.QuantidadeUtilizada)
            return Resultado.Falha($"Estoque insuficiente: {item.ItemEstoque.Nome}."); // rollback ao sair do using
        item.ItemEstoque.Quantidade -= item.QuantidadeUtilizada;
    }

    os.Status = StatusOS.Concluida;
    os.DataConclusao = DateTime.Now;
    os.Agendamento.Status = StatusAgendamento.Concluido;
    db.Atividades.Add(AtividadeRecente.OsConcluida(os));

    await db.SaveChangesAsync();
    await tx.CommitAsync();
    return Resultado.Ok();
}
```

## Tarefas técnicas

- [x] Criar `OrdemServico`, `ItemOS`, enum `StatusOS` e configuração EF (índice único em AgendamentoId)
- [x] Criar migration SQLite para OS, itens e atividades, incluindo índices e relações
- [x] Implementar `OrdemServicoService` (listar, consultar, criar, atualizar e calcular total)
- [x] Implementar `ConcluirAsync` + testes (sucesso, estoque insuficiente com rollback, dupla conclusão)
- [x] Telas de lista, formulário e conclusão integradas à navegação principal

## Definição de pronto

- [x] Todas as histórias da feature concluídas
- [x] Teste de estoque insuficiente comprova que estoque e status não mudam quando há falha
- [x] Funciona offline
- [x] Atividade de conclusão persistida localmente
