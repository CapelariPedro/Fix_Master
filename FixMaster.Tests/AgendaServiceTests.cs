using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FixMaster.Data;
using FixMaster.Models;
using FixMaster.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FixMaster.Tests;

public sealed class AgendaServiceTests
{
    private static readonly DateTime DiaTeste = new(2026, 11, 12);

    [Fact]
    public async Task SalvarAsync_RejeitaSobreposicaoExibeConflitoEPermiteHorarioConsecutivo()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var cliente = await ambiente.CriarClienteAsync();
        var existente = CriarAgendamento(cliente.Id, DiaTeste.AddHours(9), 60);

        var cadastro = await ambiente.Servico.SalvarAsync(existente);
        Assert.True(cadastro.Sucesso, cadastro.Mensagem);
        Assert.Equal(DiaTeste.AddHours(10), existente.DataHoraFim);

        var sobreposto = CriarAgendamento(cliente.Id, DiaTeste.AddHours(9).AddMinutes(30), 30);
        var resultado = await ambiente.Servico.SalvarAsync(sobreposto);

        Assert.False(resultado.Sucesso);
        Assert.Contains("Conflito de horário", resultado.Mensagem);
        Assert.Contains("Cliente de teste", resultado.Mensagem);
        Assert.Equal(0, sobreposto.Id);

        var consecutivo = CriarAgendamento(cliente.Id, DiaTeste.AddHours(10), 30);
        resultado = await ambiente.Servico.SalvarAsync(consecutivo);
        Assert.True(resultado.Sucesso, resultado.Mensagem);

        var lista = await ambiente.Servico.ListarDiaAsync(DiaTeste);
        Assert.True(lista.Sucesso, lista.Mensagem);
        Assert.Equal(2, lista.Valor!.Count);
        Assert.Equal(existente.Id, lista.Valor[0].Id);
        Assert.Equal(consecutivo.Id, lista.Valor[1].Id);
    }

    [Theory]
    [InlineData(-30, 60)]
    [InlineData(15, 10)]
    [InlineData(30, 60)]
    public async Task TemConflitoAsync_DetectaSobreposicoesParciaisEContidas(
        int deslocamentoMinutos,
        int duracaoMinutos)
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var cliente = await ambiente.CriarClienteAsync();
        var existente = CriarAgendamento(cliente.Id, DiaTeste.AddHours(9), 60);
        Assert.True((await ambiente.Servico.SalvarAsync(existente)).Sucesso);

        var conflito = await ambiente.Servico.TemConflitoAsync(
            DiaTeste.AddHours(9).AddMinutes(deslocamentoMinutos),
            duracaoMinutos);

        Assert.True(conflito.Sucesso, conflito.Mensagem);
        Assert.True(conflito.Valor);
    }

    [Fact]
    public async Task SalvarAsync_EditarIgnoraRegistroAtualECancelaSemBloquearHorario()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var cliente = await ambiente.CriarClienteAsync();
        var agendamento = CriarAgendamento(cliente.Id, DiaTeste.AddHours(9), 60);
        Assert.True((await ambiente.Servico.SalvarAsync(agendamento)).Sucesso);

        agendamento.DataHoraInicio = DiaTeste.AddHours(9).AddMinutes(15);
        agendamento.DuracaoMinutos = 30;
        var resultado = await ambiente.Servico.SalvarAsync(agendamento);
        Assert.True(resultado.Sucesso, resultado.Mensagem);

        resultado = await ambiente.Servico.AlterarStatusAsync(
            agendamento.Id, StatusAgendamento.Cancelado);
        Assert.True(resultado.Sucesso, resultado.Mensagem);

        var novo = CriarAgendamento(cliente.Id, DiaTeste.AddHours(9).AddMinutes(15), 30);
        resultado = await ambiente.Servico.SalvarAsync(novo);
        Assert.True(resultado.Sucesso, resultado.Mensagem);

        var busca = await ambiente.Servico.BuscarConflitoAsync(
            novo.DataHoraInicio, novo.DuracaoMinutos);
        Assert.True(busca.Sucesso, busca.Mensagem);
        Assert.Equal(novo.Id, busca.Valor?.Id);
    }

    [Fact]
    public async Task AlterarStatusAsync_NaoReativaHorarioQueAgoraTemConflito()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var cliente = await ambiente.CriarClienteAsync();
        var cancelado = CriarAgendamento(cliente.Id, DiaTeste.AddHours(9), 60);
        Assert.True((await ambiente.Servico.SalvarAsync(cancelado)).Sucesso);
        Assert.True((await ambiente.Servico.AlterarStatusAsync(
            cancelado.Id, StatusAgendamento.Cancelado)).Sucesso);

        var ativo = CriarAgendamento(cliente.Id, DiaTeste.AddHours(9), 60);
        Assert.True((await ambiente.Servico.SalvarAsync(ativo)).Sucesso);

        var resultado = await ambiente.Servico.AlterarStatusAsync(
            cancelado.Id, StatusAgendamento.Pendente);

        Assert.False(resultado.Sucesso);
        Assert.Contains("conflito", resultado.Mensagem, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SalvarAsync_ValidaDuracaoDescricaoEStatusInicial()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var cliente = await ambiente.CriarClienteAsync();
        var agendamento = CriarAgendamento(cliente.Id, DiaTeste.AddHours(9), 0);

        var resultado = await ambiente.Servico.SalvarAsync(agendamento);
        Assert.False(resultado.Sucesso);

        agendamento.DuracaoMinutos = 60;
        agendamento.DescricaoServico = "   ";
        resultado = await ambiente.Servico.SalvarAsync(agendamento);
        Assert.False(resultado.Sucesso);

        agendamento.DescricaoServico = "Revisão";
        resultado = await ambiente.Servico.SalvarAsync(agendamento);
        Assert.True(resultado.Sucesso, resultado.Mensagem);
        Assert.Equal(StatusAgendamento.Pendente, agendamento.Status);
    }

    [Fact]
    public async Task ListarMesAsync_RetornaDiasOcupadosSemCancelamentos()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var cliente = await ambiente.CriarClienteAsync();
        var primeiroDia = CriarAgendamento(cliente.Id, DiaTeste.AddHours(9), 60);
        var segundoDia = CriarAgendamento(cliente.Id, DiaTeste.AddDays(2).AddHours(9), 60);
        var cancelado = CriarAgendamento(cliente.Id, DiaTeste.AddDays(3).AddHours(9), 60);

        Assert.True((await ambiente.Servico.SalvarAsync(primeiroDia)).Sucesso);
        Assert.True((await ambiente.Servico.SalvarAsync(segundoDia)).Sucesso);
        Assert.True((await ambiente.Servico.SalvarAsync(cancelado)).Sucesso);
        Assert.True((await ambiente.Servico.AlterarStatusAsync(
            cancelado.Id, StatusAgendamento.Cancelado)).Sucesso);

        var resultado = await ambiente.Servico.ListarMesAsync(DiaTeste.Year, DiaTeste.Month);

        Assert.True(resultado.Sucesso, resultado.Mensagem);
        Assert.Equal(
            new[] { DiaTeste.Date, DiaTeste.AddDays(2).Date },
            resultado.Valor!.ToArray());
    }

    [Fact]
    public async Task ListarDiaAsync_ComportaOitoAgendamentosOrdenados()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var cliente = await ambiente.CriarClienteAsync();

        for (var hora = 8; hora < 16; hora++)
        {
            var agendamento = CriarAgendamento(cliente.Id, DiaTeste.AddHours(hora), 60);
            var resultado = await ambiente.Servico.SalvarAsync(agendamento);
            Assert.True(resultado.Sucesso, resultado.Mensagem);
        }

        var lista = await ambiente.Servico.ListarDiaAsync(DiaTeste);

        Assert.True(lista.Sucesso, lista.Mensagem);
        Assert.Equal(8, lista.Valor!.Count);
        Assert.Equal(
            Enumerable.Range(8, 8).Select(hora => DiaTeste.AddHours(hora)),
            lista.Valor.Select(item => item.DataHoraInicio));
    }

    [Fact]
    public async Task ExcluirClienteComAgendamento_InativaClienteEPreservaVinculo()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var cliente = await ambiente.CriarClienteAsync();
        var agendamento = CriarAgendamento(cliente.Id, DiaTeste.AddHours(9), 60);
        Assert.True((await ambiente.Servico.SalvarAsync(agendamento)).Sucesso);

        var resultado = await ambiente.Clientes.ExcluirOuInativarAsync(cliente.Id);

        Assert.True(resultado.Sucesso, resultado.Mensagem);
        Assert.DoesNotContain(
            (await ambiente.Clientes.ListarAsync(null)).Valor!,
            item => item.Id == cliente.Id);
        var atendimentos = await ambiente.Servico.ListarDiaAsync(DiaTeste);
        Assert.True(atendimentos.Sucesso, atendimentos.Mensagem);
        Assert.Contains(atendimentos.Valor!, item => item.Id == agendamento.Id);
    }

    private static Agendamento CriarAgendamento(int clienteId, DateTime inicio, int duracao) =>
        new()
        {
            ClienteId = clienteId,
            DataHoraInicio = inicio,
            DuracaoMinutos = duracao,
            DescricaoServico = "Revisão preventiva"
        };

    private sealed class AmbienteTeste : IAsyncDisposable
    {
        private readonly SqliteConnection _conexao;

        private AmbienteTeste(SqliteConnection conexao, FabricaContexto fabrica)
        {
            _conexao = conexao;
            Fabrica = fabrica;
            Servico = new AgendaService(fabrica);
            Clientes = new ClienteService(fabrica);
        }

        private FabricaContexto Fabrica { get; }
        public AgendaService Servico { get; }
        public ClienteService Clientes { get; }

        public static async Task<AmbienteTeste> CriarAsync()
        {
            var conexao = new SqliteConnection(
                $"Data Source=FixMasterAgendaTests_{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
            await conexao.OpenAsync();
            var opcoes = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(conexao)
                .Options;
            var fabrica = new FabricaContexto(opcoes);
            await using (var db = await fabrica.CreateDbContextAsync())
                await db.Database.MigrateAsync();

            return new AmbienteTeste(conexao, fabrica);
        }

        public async Task<Cliente> CriarClienteAsync()
        {
            var cliente = new Cliente
            {
                Nome = "Cliente de teste",
                Documento = "529.982.247-25",
                Telefone = "11 98888-0000",
                Endereco = "Rua de teste, 10"
            };
            var resultado = await Clientes.SalvarAsync(cliente);
            Assert.True(resultado.Sucesso, resultado.Mensagem);
            return cliente;
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
