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

public sealed class EstoqueServiceTests
{
    [Fact]
    public async Task SalvarAsync_ValidaESalvaMaterialNormalizandoPrateleira()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var material = CriarMaterial();
        material.Prateleira = "g";

        var resultado = await ambiente.Servico.SalvarAsync(material);

        Assert.True(resultado.Sucesso, resultado.Mensagem);
        Assert.True(material.Id > 0);
        var salvo = await ambiente.Servico.ObterAsync(material.Id);
        Assert.True(salvo.Sucesso, salvo.Mensagem);
        Assert.Equal("G", salvo.Valor!.Prateleira);
        Assert.Equal(4, salvo.Valor.Posicao);
        Assert.Equal(12.50m, salvo.Valor.PrecoUnitario);
    }

    [Fact]
    public async Task GerarCodigoAsync_UsaPrefixoDoTipoECriaCodigoUnico()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();

        var primeiro = await ambiente.Servico.GerarCodigoAsync("Ferramentas");
        Assert.True(primeiro.Sucesso, primeiro.Mensagem);
        Assert.Matches("^FER-[0-9]{6}$", primeiro.Valor);

        var material = CriarMaterial();
        material.Codigo = primeiro.Valor!;
        material.Categoria = "Ferramentas";
        Assert.True((await ambiente.Servico.SalvarAsync(material)).Sucesso);

        var segundo = await ambiente.Servico.GerarCodigoAsync("Ferramentas");
        Assert.True(segundo.Sucesso, segundo.Mensagem);
        Assert.NotEqual(primeiro.Valor, segundo.Valor);

        var eletrico = await ambiente.Servico.GerarCodigoAsync("Insumos", "Elétrico");
        Assert.True(eletrico.Sucesso, eletrico.Mensagem);
        Assert.Matches("^INS-E-[0-9]{6}$", eletrico.Valor);

        var hidraulico = await ambiente.Servico.GerarCodigoAsync("Insumos", "Hidráulico");
        Assert.True(hidraulico.Sucesso, hidraulico.Mensagem);
        Assert.Matches("^INS-H-[0-9]{6}$", hidraulico.Valor);

        var tipoInsumoAusente = await ambiente.Servico.GerarCodigoAsync("Insumos");
        Assert.False(tipoInsumoAusente.Sucesso);
    }

    [Fact]
    public async Task EstoqueViewModel_GeraCodigoAoSelecionarTipo()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var viewModel = new EstoqueViewModel(ambiente.Servico);

        viewModel.Categoria = "Peças";
        await viewModel.AtualizarCodigoPorCategoriaAsync(viewModel.Categoria);
        Assert.Matches("^PEC-[0-9]{6}$", viewModel.Codigo);

        viewModel.Categoria = "Insumos";
        await viewModel.AtualizarCodigoPorCategoriaAsync(viewModel.Categoria);
        Assert.True(viewModel.EhInsumo);
        Assert.Empty(viewModel.Codigo);

        viewModel.TipoInsumo = "Hidráulico";
        await viewModel.AtualizarCodigoPorCategoriaAsync(
            viewModel.Categoria, viewModel.TipoInsumo);
        Assert.Matches("^INS-H-[0-9]{6}$", viewModel.Codigo);
    }

    [Fact]
    public async Task SalvarAsync_RejeitaCodigoECoordenadaDuplicados()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var primeiro = CriarMaterial();
        Assert.True((await ambiente.Servico.SalvarAsync(primeiro)).Sucesso);

        var codigoDuplicado = CriarMaterial();
        codigoDuplicado.Prateleira = "A";
        codigoDuplicado.Posicao = 1;
        var resultado = await ambiente.Servico.SalvarAsync(codigoDuplicado);
        Assert.False(resultado.Sucesso);
        Assert.Contains("Código", resultado.Mensagem);

        var localDuplicado = CriarMaterial();
        localDuplicado.Codigo = "COD-002";
        resultado = await ambiente.Servico.SalvarAsync(localDuplicado);
        Assert.False(resultado.Sucesso);
        Assert.Contains("Localização G4", resultado.Mensagem);
    }

    [Fact]
    public async Task BuscarAsync_LocalizaPorTrechoDeNomeCodigoECategoria()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var material = CriarMaterial();
        Assert.True((await ambiente.Servico.SalvarAsync(material)).Sucesso);

        Assert.Single((await ambiente.Servico.BuscarAsync("paraf")).Valor!);
        Assert.Single((await ambiente.Servico.BuscarAsync("od-00")).Valor!);
        Assert.Single((await ambiente.Servico.BuscarAsync("peças")).Valor!);
        Assert.Single((await ambiente.Servico.BuscarAsync("pecas")).Valor!);
        Assert.Empty((await ambiente.Servico.BuscarAsync("sem resultado")).Valor!);
    }

    [Fact]
    public async Task ObterMatrizAsync_RefleteBancoEDesativacaoLiberaCoordenada()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var material = CriarMaterial();
        Assert.True((await ambiente.Servico.SalvarAsync(material)).Sucesso);

        var matriz = await ambiente.Servico.ObterMatrizAsync();
        Assert.True(matriz.Sucesso, matriz.Mensagem);
        var celula = Assert.Single(matriz.Valor!);
        Assert.Equal("G", celula.Prateleira);
        Assert.Equal(4, celula.Posicao);

        Assert.True((await ambiente.Servico.DesativarAsync(material.Id)).Sucesso);
        Assert.Empty((await ambiente.Servico.ObterMatrizAsync()).Valor!);

        var outroMaterial = CriarMaterial();
        outroMaterial.Codigo = "COD-002";
        Assert.True((await ambiente.Servico.SalvarAsync(outroMaterial)).Sucesso);
    }

    [Fact]
    public async Task SalvarAsync_RejeitaDadosObrigatoriosEValoresNegativos()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var material = CriarMaterial();
        material.Quantidade = -1;

        var resultado = await ambiente.Servico.SalvarAsync(material);

        Assert.False(resultado.Sucesso);
        Assert.Contains("Quantidade", resultado.Mensagem);

        material = CriarMaterial();
        material.Prateleira = "Y";
        resultado = await ambiente.Servico.SalvarAsync(material);
        Assert.False(resultado.Sucesso);
        Assert.Contains("Prateleira", resultado.Mensagem);

        material = CriarMaterial();
        material.Posicao = 9;
        resultado = await ambiente.Servico.SalvarAsync(material);
        Assert.False(resultado.Sucesso);
        Assert.Contains("Posição", resultado.Mensagem);
    }

    [Fact]
    public async Task EstoqueViewModel_CriaMatrizDeVinteEQuatroPorOito()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var material = CriarMaterial();
        Assert.True((await ambiente.Servico.SalvarAsync(material)).Sucesso);

        var viewModel = new EstoqueViewModel(ambiente.Servico);
        await viewModel.InicializarAsync();

        Assert.Equal(7, viewModel.Prateleiras.Count);
        Assert.All(viewModel.Prateleiras, linha => Assert.Equal(8, linha.Celulas.Count));
        var celula = viewModel.Prateleiras
            .Single(linha => linha.Prateleira == "G")
            .Celulas.Single(item => item.Coordenada == "G4");
        Assert.True(celula.Ocupada);
        Assert.True(celula.EmDestaque);
        Assert.Equal("G4", viewModel.LocalizacaoDestaque);
    }

    [Theory]
    [InlineData(-1, "A", 1)]
    [InlineData(0, "A", 9)]
    [InlineData(0, "Y", 1)]
    public async Task Banco_RejeitaValoresForaDasRegras(
        int quantidade,
        string prateleira,
        int posicao)
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        await using var db = await ambiente.Fabrica.CreateDbContextAsync();

        await Assert.ThrowsAsync<SqliteException>(() =>
            db.Database.ExecuteSqlInterpolatedAsync(
                $"""
                INSERT INTO "ItensEstoque"
                    ("Codigo", "Nome", "Descricao", "Categoria", "Quantidade",
                     "PrecoUnitario", "Prateleira", "Posicao", "Ativo")
                VALUES
                    ('INVALIDO', 'Material', '', 'Teste', {quantidade},
                     '1.00', {prateleira}, {posicao}, 1)
                """));
    }

    [Fact]
    public async Task MigracaoAteG_RealocaMateriaisForaDaFaixaEPodeReverter()
    {
        var conexao = new SqliteConnection(
            $"Data Source=FixMasterMigracaoEstoque_{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
        await conexao.OpenAsync();
        try
        {
            var opcoes = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(conexao)
                .Options;
            var fabrica = new FabricaContexto(opcoes);
            await using (var db = await fabrica.CreateDbContextAsync())
            {
                await db.Database.MigrateAsync("20261111134500_Estoque");
                await db.Database.ExecuteSqlRawAsync(
                    """
                    INSERT INTO "ItensEstoque"
                        ("Codigo", "Nome", "Descricao", "Categoria", "Quantidade",
                         "PrecoUnitario", "Prateleira", "Posicao", "Ativo")
                    VALUES
                        ('A-1', 'Existente 1', '', 'Peças', 1, '1.00', 'A', 1, 1),
                        ('A-2', 'Existente 2', '', 'Peças', 1, '1.00', 'A', 2, 1),
                        ('H-1', 'Material H', '', 'Peças', 3, '1.00', 'H', 1, 1),
                        ('X-1', 'Material X', '', 'Peças', 4, '1.00', 'X', 1, 1)
                    """);

                await db.Database.MigrateAsync();
                var realocados = await db.ItensEstoque
                    .OrderBy(item => item.Codigo)
                    .ToListAsync();
                Assert.Equal(("A", 3), (realocados[2].Prateleira, realocados[2].Posicao));
                Assert.Equal(("A", 4), (realocados[3].Prateleira, realocados[3].Posicao));
                Assert.Equal(2, await db.Database
                    .SqlQueryRaw<int>("SELECT COUNT(*) AS \"Value\" FROM \"__EstoquePrateleirasOriginais\"")
                    .SingleAsync());

                await db.Database.MigrateAsync("20261111134500_Estoque");
                Assert.Equal(
                    "20261111134500_Estoque",
                    await db.Database.SqlQueryRaw<string>(
                            "SELECT \"MigrationId\" AS \"Value\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\" DESC LIMIT 1")
                        .SingleAsync());
                db.ChangeTracker.Clear();
                var revertidos = await db.ItensEstoque
                    .OrderBy(item => item.Codigo)
                    .ToListAsync();
                Assert.Equal(("H", 1), (revertidos[2].Prateleira, revertidos[2].Posicao));
                Assert.Equal(("X", 1), (revertidos[3].Prateleira, revertidos[3].Posicao));
            }
        }
        finally
        {
            await conexao.DisposeAsync();
        }
    }

    private static ItemEstoque CriarMaterial() =>
        new()
        {
            Codigo = "COD-001",
            Nome = "Parafuso sextavado",
            Descricao = "Aço galvanizado",
            Categoria = "Peças",
            Quantidade = 12,
            PrecoUnitario = 12.50m,
            Prateleira = "G",
            Posicao = 4
        };

    private sealed class AmbienteTeste : IAsyncDisposable
    {
        private readonly SqliteConnection _conexao;

        private AmbienteTeste(
            SqliteConnection conexao,
            FabricaContexto fabrica,
            EstoqueService servico)
        {
            _conexao = conexao;
            Fabrica = fabrica;
            Servico = servico;
        }

        public FabricaContexto Fabrica { get; }
        public EstoqueService Servico { get; }

        public static async Task<AmbienteTeste> CriarAsync()
        {
            var conexao = new SqliteConnection(
                $"Data Source=FixMasterEstoqueTests_{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
            await conexao.OpenAsync();
            var opcoes = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(conexao)
                .Options;
            var fabrica = new FabricaContexto(opcoes);
            await using (var db = await fabrica.CreateDbContextAsync())
                await db.Database.MigrateAsync();

            return new AmbienteTeste(conexao, fabrica, new EstoqueService(fabrica));
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
