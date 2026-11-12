using System;
using System.Linq;
using System.Threading.Tasks;
using FixMaster.Data;
using FixMaster.Models;
using FixMaster.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FixMaster.Tests;

public sealed class ClienteServiceTests
{
    [Fact]
    public async Task SalvarAsync_CadastraAtualizaEConsultaCliente()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var cliente = CriarPessoaFisica();
        cliente.Nome = "João d'Ávila & Irmãos";
        cliente.Endereco = "Rua São João, nº 10 - Centro";
        cliente.Cep = "12345-678";

        var cadastro = await ambiente.Servico.SalvarAsync(cliente);
        Assert.True(cadastro.Sucesso, cadastro.Mensagem);
        Assert.True(cliente.Id > 0);

        await using (var db = await ambiente.Fabrica.CreateDbContextAsync())
        {
            var salvo = await db.Clientes.SingleAsync(item => item.Id == cliente.Id);
            Assert.Equal("João d'Ávila & Irmãos", salvo.Nome);
            Assert.Equal("Rua São João, nº 10 - Centro", salvo.Endereco);
            Assert.Equal("12345678", salvo.Cep);
        }

        var atualizacao = await ambiente.Servico.SalvarAsync(new Cliente
        {
            Id = cliente.Id,
            Nome = "Maria Oliveira",
            Documento = cliente.Documento,
            Cep = cliente.Cep,
            Telefone = cliente.Telefone,
            Endereco = cliente.Endereco
        });
        Assert.True(atualizacao.Sucesso, atualizacao.Mensagem);

        var consulta = await ambiente.Servico.ObterAsync(cliente.Id);
        Assert.True(consulta.Sucesso, consulta.Mensagem);
        Assert.Equal("Maria Oliveira", consulta.Valor?.Nome);
        Assert.Equal("12345-678", consulta.Valor?.CepFormatado);
    }

    [Fact]
    public async Task SalvarAsync_ValidaCamposETornaDocumentoUnico()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var primeiro = CriarPessoaFisica();
        Assert.True((await ambiente.Servico.SalvarAsync(primeiro)).Sucesso);

        var duplicado = CriarPessoaFisica();
        duplicado.Nome = "Outro cliente";
        var resultado = await ambiente.Servico.SalvarAsync(duplicado);

        Assert.False(resultado.Sucesso);
        Assert.Contains("Documento", resultado.Mensagem);

        var documentoInvalido = CriarPessoaFisica();
        documentoInvalido.Documento = "111.111.111-11";
        resultado = await ambiente.Servico.SalvarAsync(documentoInvalido);
        Assert.False(resultado.Sucesso);
        Assert.Contains("CPF", resultado.Mensagem);

        var cepInvalido = CriarPessoaFisica();
        cepInvalido.Cep = "1234-567";
        resultado = await ambiente.Servico.SalvarAsync(cepInvalido);
        Assert.False(resultado.Sucesso);
        Assert.Contains("CEP", resultado.Mensagem);

        var empresa = new Cliente
        {
            Nome = "Empresa de teste",
            Documento = "12.abc.345/01de-35",
            Telefone = "11 3000-0000",
            Endereco = "Avenida B, 20",
            EhEmpresa = true
        };
        Assert.True((await ambiente.Servico.SalvarAsync(empresa)).Sucesso);

        await using var db = await ambiente.Fabrica.CreateDbContextAsync();
        var empresaSalva = await db.Clientes.SingleAsync(item => item.Id == empresa.Id);
        Assert.Equal("12ABC34501DE35", empresaSalva.Documento);

        var cnpjDuplicado = new Cliente
        {
            Nome = "CNPJ duplicado",
            Documento = "12.ABC.345/01DE-35",
            Telefone = "11 3000-1111",
            Endereco = "Avenida C, 30",
            EhEmpresa = true
        };
        resultado = await ambiente.Servico.SalvarAsync(cnpjDuplicado);
        Assert.False(resultado.Sucesso);
    }

    [Fact]
    public async Task ListarAsync_BuscaPorNomeTelefoneEDocumento()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var cliente = CriarPessoaFisica();
        cliente.Cep = "12345-678";
        Assert.True((await ambiente.Servico.SalvarAsync(cliente)).Sucesso);

        var clienteComAcentos = new Cliente
        {
            Nome = "João & Irmãos Ltda.",
            Documento = "11.222.333/0001-81",
            Telefone = "11 98888-0000",
            Endereco = "Rua São João, nº 10 - Centro",
            Cep = "98765-432",
            EhEmpresa = true
        };
        Assert.True((await ambiente.Servico.SalvarAsync(clienteComAcentos)).Sucesso);

        var empresa = new Cliente
        {
            Nome = "Empresa ABC",
            Documento = "12.ABC.345/01DE-35",
            Telefone = "11 3000-0000",
            Endereco = "Avenida B, 20",
            EhEmpresa = true
        };
        Assert.True((await ambiente.Servico.SalvarAsync(empresa)).Sucesso);

        var porNome = await ambiente.Servico.ListarAsync("maria");
        var porTelefone = await ambiente.Servico.ListarAsync("99999");
        var porDocumento = await ambiente.Servico.ListarAsync("529.982");
        var porCnpjAlfanumerico = await ambiente.Servico.ListarAsync("abc345");
        var porCep = await ambiente.Servico.ListarAsync("12345-678");
        var porNomeComAcentoSemAcento = await ambiente.Servico.ListarAsync("joao");

        Assert.Single(porNome.Valor!);
        Assert.Single(porTelefone.Valor!);
        Assert.Single(porDocumento.Valor!);
        Assert.Single(porCnpjAlfanumerico.Valor!);
        Assert.Single(porCep.Valor!);
        Assert.Single(porNomeComAcentoSemAcento.Valor!);
        Assert.Equal("529.982.247-25", porDocumento.Valor![0].DocumentoFormatado);
    }

    [Fact]
    public async Task ExcluirOuInativarAsync_ExcluiSemVinculosEInativaComVinculos()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var clienteVinculado = CriarPessoaFisica();
        Assert.True((await ambiente.Servico.SalvarAsync(clienteVinculado)).Sucesso);

        await using (var db = await ambiente.Fabrica.CreateDbContextAsync())
        {
            await db.Database.ExecuteSqlRawAsync(
                "CREATE TABLE \"AgendamentoTeste\" (\"Id\" INTEGER PRIMARY KEY, \"ClienteId\" INTEGER NOT NULL REFERENCES \"Clientes\"(\"Id\"));");
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO \"AgendamentoTeste\" (\"Id\", \"ClienteId\") VALUES (1, {clienteVinculado.Id});");
        }

        var resultado = await ambiente.Servico.ExcluirOuInativarAsync(clienteVinculado.Id);
        Assert.True(resultado.Sucesso, resultado.Mensagem);
        var clientesAtivos = await ambiente.Servico.ListarAsync(null);
        Assert.DoesNotContain(clientesAtivos.Valor!, item => item.Id == clienteVinculado.Id);
        Assert.False((await ambiente.Servico.ObterAsync(clienteVinculado.Id)).Sucesso);

        var clienteSemVinculos = new Cliente
        {
            Nome = "Empresa sem vínculos",
            Documento = "11.222.333/0001-81",
            Telefone = "11 3000-0000",
            Endereco = "Avenida B, 20",
            EhEmpresa = true
        };
        Assert.True((await ambiente.Servico.SalvarAsync(clienteSemVinculos)).Sucesso);
        Assert.True((await ambiente.Servico.ExcluirOuInativarAsync(clienteSemVinculos.Id)).Sucesso);
        Assert.False((await ambiente.Servico.ObterAsync(clienteSemVinculos.Id)).Sucesso);
    }

    private static Cliente CriarPessoaFisica() =>
        new()
        {
            Nome = "Maria Silva",
            Documento = "529.982.247-25",
            Telefone = "(11) 99999-0000",
            Endereco = "Rua A, 10"
        };

    private sealed class AmbienteTeste : IAsyncDisposable
    {
        private readonly SqliteConnection _conexao;

        private AmbienteTeste(
            SqliteConnection conexao,
            IDbContextFactory<AppDbContext> fabrica,
            ClienteService servico)
        {
            _conexao = conexao;
            Fabrica = fabrica;
            Servico = servico;
        }

        public IDbContextFactory<AppDbContext> Fabrica { get; }
        public ClienteService Servico { get; }

        public static async Task<AmbienteTeste> CriarAsync()
        {
            var conexao = new SqliteConnection(
                $"Data Source=FixMasterTests_{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
            await conexao.OpenAsync();
            var opcoes = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(conexao)
                .Options;
            var fabrica = new FabricaContexto(opcoes);
            await using (var db = await fabrica.CreateDbContextAsync())
                await db.Database.MigrateAsync();

            return new AmbienteTeste(conexao, fabrica, new ClienteService(fabrica));
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
