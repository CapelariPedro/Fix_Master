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
- `OrdemServicoFormView`: seleção do agendamento, relatório técnico, mão de obra, adição de materiais (busca no estoque + quantidade).
- Botão **CONCLUIR OS** em destaque; confirmação antes de executar.

## Entidades envolvidas

- `OrdemServico` (Id, AgendamentoId único, RelatorioTecnico, MaoDeObra, Status, DataEmissao, DataConclusao)
- `ItemOS` (Id, OrdemServicoId, ItemEstoqueId, QuantidadeUtilizada, ValorUnitario)
- `ItemEstoque`, `Agendamento` (atualizados na conclusão)

## Serviços e regras de negócio

- `IOrdemServicoService`: `CriarAsync`, `AtualizarAsync`, `ObterAsync`, `ConcluirAsync(osId)`
- Valor total = MaoDeObra + Σ (QuantidadeUtilizada × ValorUnitario).

### Regras

- Conclusão em **uma transação**: verificar status → verificar estoque → baixar → atualizar status da OS e do agendamento → registrar atividade.
- Idempotência: OS já concluída não baixa estoque novamente.
- Estoque insuficiente: nenhuma alteração; mensagem indica o item.
- `ValorUnitario` é copiado do cadastro no momento da inclusão do item.
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

- [ ] Criar `OrdemServico`, `ItemOS`, enum `StatusOS` e configuração EF (índice único em AgendamentoId)
- [ ] Criar tipo `Resultado` (Ok/Falha com mensagem)
- [ ] Implementar `OrdemServicoService` (criar, atualizar, calcular total)
- [ ] Implementar `ConcluirAsync` + testes (sucesso, estoque insuficiente com rollback, dupla conclusão)
- [ ] Telas de lista, formulário e conclusão

## Definição de pronto

- [ ] Todas as histórias da feature concluídas
- [ ] Teste de rollback comprova que nada muda quando há falha
- [ ] Funciona offline
- [ ] Atividade registrada no feed
