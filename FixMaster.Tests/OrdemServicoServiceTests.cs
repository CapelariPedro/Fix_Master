using System;
using System.Linq;
using System.Threading.Tasks;
using FixMaster.Data;
using FixMaster.Models;
using FixMaster.Services;
using FixMaster.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FixMaster.Tests;

public sealed class OrdemServicoServiceTests
{
    [Fact]
    public async Task Migration_DeveContinuarAposMigrationDeOSJaAplicada()
    {
        await using var conexao = new SqliteConnection(
            $"Data Source=FixMasterOsMigrationTests_{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
        await conexao.OpenAsync();
        var opcoes = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conexao)
            .Options;
        var fabrica = new FabricaContexto(opcoes);

        await using (var db = await fabrica.CreateDbContextAsync())
            await db.Database.MigrateAsync("20261112160000_OrdensServico");

        await using (var db = await fabrica.CreateDbContextAsync())
            await db.Database.MigrateAsync();

        await using var verificacao = await fabrica.CreateDbContextAsync();
        Assert.True(await verificacao.Database.CanConnectAsync());
        var migrations = await verificacao.Database.GetAppliedMigrationsAsync();
        Assert.Contains("20261112160000_OrdensServico", migrations);
        Assert.Contains("20261112210000_AtividadesRecentes", migrations);
        Assert.Equal(0, await verificacao.OrdensServico.CountAsync());
        Assert.Equal(0, await verificacao.AtividadesRecentes.CountAsync());
    }

    [Fact]
    public async Task CriarAsync_ExigeAgendamentoUnicoERegistraPrecoDoMaterial()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var (cliente, agendamento) = await ambiente.CriarAgendamentoAsync();
        var material = await ambiente.CriarMaterialAsync("Bateria", 12, 49.90m);
        var ordem = CriarOrdem(agendamento.Id, material.Id, 2);

        var criada = await ambiente.Ordens.CriarAsync(ordem);

        Assert.True(criada.Sucesso, criada.Mensagem);
        Assert.True(criada.Valor > 0);
        var carregada = await ambiente.Ordens.ObterAsync(criada.Valor);
        Assert.True(carregada.Sucesso, carregada.Mensagem);
        Assert.Equal(49.90m, Assert.Single(carregada.Valor!.Itens).ValorUnitario);
        Assert.Equal(149.80m, OrdemServicoService.CalcularTotal(carregada.Valor));

        material.PrecoUnitario = 59.90m;
        Assert.True((await ambiente.Estoque.SalvarAsync(material)).Sucesso);

        var outra = await ambiente.CriarAgendamentoParaClienteAsync(cliente.Id, DateTime.Today.AddDays(1));
        var duplicada = await ambiente.Ordens.CriarAsync(CriarOrdem(agendamento.Id, material.Id, 1));
        Assert.False(duplicada.Sucesso);
        Assert.Contains("Já existe", duplicada.Mensagem);

        var atualizada = await ambiente.Ordens.ObterAsync(criada.Valor);
        Assert.Equal(49.90m, Assert.Single(atualizada.Valor!.Itens).ValorUnitario);
        Assert.NotEqual(agendamento.Id, outra.Id);
    }

    [Fact]
    public async Task ModeloOrdemServico_StatusDeveSerInformadoPelaAplicacao()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        await using var db = await ambiente.Fabrica.CreateDbContextAsync();

        var propriedade = db.Model.FindEntityType(typeof(OrdemServico))!
            .FindProperty(nameof(OrdemServico.Status));

        Assert.Equal(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never, propriedade!.ValueGenerated);
    }

    [Fact]
    public async Task OrdemServicoViewModel_CriaOSSelecionandoAgendamentoSemMateriais()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var (_, agendamento) = await ambiente.CriarAgendamentoAsync();
        var viewModel = new OrdemServicoViewModel(ambiente.Ordens, ambiente.Estoque);

        await viewModel.NovaCommand.ExecuteAsync(null);

        Assert.Equal(agendamento.Id, viewModel.AgendamentoSelecionado?.Id);
        Assert.True(viewModel.PodeSalvar);
        viewModel.RelatorioTecnico = "Atendimento realizado";
        viewModel.MaoDeObraTexto = "125,50";

        await viewModel.SalvarCommand.ExecuteAsync(null);

        var ordem = Assert.Single((await ambiente.Ordens.ListarAsync()).Valor!);
        Assert.Equal(agendamento.Id, ordem.AgendamentoId);
        Assert.Equal(125.50m, ordem.MaoDeObra);
        Assert.Empty(ordem.Itens);
        Assert.False(viewModel.IsFormularioAberto);
        Assert.Contains("criada com sucesso", viewModel.Mensagem);
    }

    [Fact]
    public async Task ConcluirAsync_BaixaEstoqueAtualizaAgendamentoERegistraAtividadeUmaVez()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var (_, agendamento) = await ambiente.CriarAgendamentoAsync();
        var material = await ambiente.CriarMaterialAsync("Filtro", 7, 12.50m);
        var criada = await ambiente.Ordens.CriarAsync(CriarOrdem(agendamento.Id, material.Id, 3));
        Assert.True(criada.Sucesso, criada.Mensagem);

        var resultado = await ambiente.Ordens.ConcluirAsync(criada.Valor);

        Assert.True(resultado.Sucesso, resultado.Mensagem);
        Assert.Equal(4, (await ambiente.Estoque.ObterAsync(material.Id)).Valor!.Quantidade);
        Assert.Equal(StatusOS.Concluida, (await ambiente.Ordens.ObterAsync(criada.Valor)).Valor!.Status);
        Assert.Equal(
            StatusAgendamento.Concluido,
            (await ambiente.Agenda.ListarDiaAsync(agendamento.DataHoraInicio)).Valor!.Single().Status);
        await using var db = await ambiente.Fabrica.CreateDbContextAsync();
        Assert.Equal(1, await db.AtividadesRecentes.CountAsync());

        resultado = await ambiente.Ordens.ConcluirAsync(criada.Valor);
        Assert.False(resultado.Sucesso);
        Assert.Contains("já foi concluída", resultado.Mensagem);
        Assert.Equal(4, (await ambiente.Estoque.ObterAsync(material.Id)).Valor!.Quantidade);
        Assert.Equal(1, await db.AtividadesRecentes.CountAsync());
    }

    [Fact]
    public async Task ConcluirAsync_EstoqueInsuficienteNaoAlteraEstoqueNemStatus()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var (_, agendamento) = await ambiente.CriarAgendamentoAsync();
        var material = await ambiente.CriarMaterialAsync("Correia", 1, 25m);
        var criada = await ambiente.Ordens.CriarAsync(CriarOrdem(agendamento.Id, material.Id, 2));
        Assert.True(criada.Sucesso, criada.Mensagem);

        var resultado = await ambiente.Ordens.ConcluirAsync(criada.Valor);

        Assert.False(resultado.Sucesso);
        Assert.Contains("Correia", resultado.Mensagem);
        Assert.Equal(1, (await ambiente.Estoque.ObterAsync(material.Id)).Valor!.Quantidade);
        Assert.Equal(StatusOS.Aberta, (await ambiente.Ordens.ObterAsync(criada.Valor)).Valor!.Status);
        await using var db = await ambiente.Fabrica.CreateDbContextAsync();
        Assert.Equal(StatusAgendamento.Pendente, await db.Agendamentos
            .Where(item => item.Id == agendamento.Id)
            .Select(item => item.Status)
            .SingleAsync());
        Assert.Empty(await db.AtividadesRecentes.ToListAsync());
    }

    [Fact]
    public async Task AtualizarAsync_PreservaPrecoDoMaterialJaVinculado()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var (_, agendamento) = await ambiente.CriarAgendamentoAsync();
        var material = await ambiente.CriarMaterialAsync("Conector", 10, 4.25m);
        var criada = await ambiente.Ordens.CriarAsync(CriarOrdem(agendamento.Id, material.Id, 1));
        Assert.True(criada.Sucesso, criada.Mensagem);

        material.PrecoUnitario = 8.75m;
        Assert.True((await ambiente.Estoque.SalvarAsync(material)).Sucesso);
        var carregada = await ambiente.Ordens.ObterAsync(criada.Valor);
        var ordem = carregada.Valor!;
        ordem.MaoDeObra = 100m;
        ordem.Itens.Single().QuantidadeUtilizada = 3;

        var atualizacao = await ambiente.Ordens.AtualizarAsync(ordem);

        Assert.True(atualizacao.Sucesso, atualizacao.Mensagem);
        var salva = await ambiente.Ordens.ObterAsync(criada.Valor);
        Assert.Equal(4.25m, Assert.Single(salva.Valor!.Itens).ValorUnitario);
        Assert.Equal(112.75m, OrdemServicoService.CalcularTotal(salva.Valor));
    }

    [Fact]
    public async Task AgendamentoComOS_PermiteIniciarMasProtegeEdicaoCancelamentoEConclusaoDireta()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var (_, agendamento) = await ambiente.CriarAgendamentoAsync();
        var criada = await ambiente.Ordens.CriarAsync(new OrdemServico
        {
            AgendamentoId = agendamento.Id,
            RelatorioTecnico = "Diagnóstico inicial",
            MaoDeObra = 0m
        });
        Assert.True(criada.Sucesso, criada.Mensagem);

        var cancelamento = await ambiente.Agenda.AlterarStatusAsync(
            agendamento.Id, StatusAgendamento.Cancelado);
        Assert.False(cancelamento.Sucesso);

        var conclusaoDireta = await ambiente.Agenda.AlterarStatusAsync(
            agendamento.Id, StatusAgendamento.Concluido);
        Assert.False(conclusaoDireta.Sucesso);

        var inicio = await ambiente.Agenda.AlterarStatusAsync(
            agendamento.Id, StatusAgendamento.EmAndamento);
        Assert.True(inicio.Sucesso, inicio.Mensagem);

        agendamento.DescricaoServico = "Tentativa de alteração";
        var edicao = await ambiente.Agenda.SalvarAsync(agendamento);
        Assert.False(edicao.Sucesso);
        Assert.Contains("possui uma OS", edicao.Mensagem);

        var resumo = Assert.Single(
            (await ambiente.Agenda.ListarDiaAsync(agendamento.DataHoraInicio)).Valor!);
        Assert.True(resumo.PossuiOrdemServico);
        Assert.False(resumo.IsEditavel);
        Assert.False(resumo.IsCancelavel);
        Assert.False(resumo.IsConcluivel);
    }

    private static OrdemServico CriarOrdem(int agendamentoId, int materialId, int quantidade) =>
        new()
        {
            AgendamentoId = agendamentoId,
            RelatorioTecnico = "Diagnóstico e serviço executado",
            MaoDeObra = 50m,
            Itens =
            [
                new ItemOS
                {
                    ItemEstoqueId = materialId,
                    QuantidadeUtilizada = quantidade
                }
            ]
        };

    private sealed class AmbienteTeste : IAsyncDisposable
    {
        private readonly SqliteConnection _conexao;

        private AmbienteTeste(SqliteConnection conexao, FabricaContexto fabrica)
        {
            _conexao = conexao;
            Fabrica = fabrica;
            Agenda = new AgendaService(fabrica);
            Estoque = new EstoqueService(fabrica);
            Ordens = new OrdemServicoService(fabrica);
            Clientes = new ClienteService(fabrica);
        }

        public FabricaContexto Fabrica { get; }
        public AgendaService Agenda { get; }
        public EstoqueService Estoque { get; }
        public OrdemServicoService Ordens { get; }
        private ClienteService Clientes { get; }

        public static async Task<AmbienteTeste> CriarAsync()
        {
            var conexao = new SqliteConnection(
                $"Data Source=FixMasterOrdemServicoTests_{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
            await conexao.OpenAsync();
            var opcoes = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(conexao)
                .Options;
            var fabrica = new FabricaContexto(opcoes);
            await using (var db = await fabrica.CreateDbContextAsync())
                await db.Database.MigrateAsync();
            return new AmbienteTeste(conexao, fabrica);
        }

        public async Task<(Cliente Cliente, Agendamento Agendamento)> CriarAgendamentoAsync()
        {
            var cliente = new Cliente
            {
                Nome = "Cliente OS",
                Documento = "529.982.247-25",
                Telefone = "11 98888-0000",
                Endereco = "Rua de teste, 10"
            };
            var salvamentoCliente = await Clientes.SalvarAsync(cliente);
            Assert.True(salvamentoCliente.Sucesso, salvamentoCliente.Mensagem);
            var agendamento = await CriarAgendamentoParaClienteAsync(cliente.Id, DateTime.Today);
            return (cliente, agendamento);
        }

        public async Task<Agendamento> CriarAgendamentoParaClienteAsync(int clienteId, DateTime dia)
        {
            var agendamento = new Agendamento
            {
                ClienteId = clienteId,
                DataHoraInicio = dia.Date.AddHours(9),
                DuracaoMinutos = 60,
                DescricaoServico = "Revisão técnica"
            };
            var resultado = await Agenda.SalvarAsync(agendamento);
            Assert.True(resultado.Sucesso, resultado.Mensagem);
            return agendamento;
        }

        public async Task<ItemEstoque> CriarMaterialAsync(string nome, int quantidade, decimal preco)
        {
            var material = new ItemEstoque
            {
                Codigo = $"COD-{Guid.NewGuid():N}"[..10],
                Nome = nome,
                Descricao = "",
                Categoria = "Peças",
                Quantidade = quantidade,
                PrecoUnitario = preco,
                Prateleira = "A",
                Posicao = 1
            };
            var resultado = await Estoque.SalvarAsync(material);
            Assert.True(resultado.Sucesso, resultado.Mensagem);
            return material;
        }

        public ValueTask DisposeAsync() => _conexao.DisposeAsync();
    }

    private sealed class FabricaContexto(DbContextOptions<AppDbContext> opcoes)
        : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(opcoes);

        public Task<AppDbContext> CreateDbContextAsync(
            System.Threading.CancellationToken cancellationToken = default) =>
            Task.FromResult(new AppDbContext(opcoes));
    }
}
