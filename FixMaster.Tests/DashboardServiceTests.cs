using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FixMaster.Data;
using FixMaster.Infrastructure;
using FixMaster.Models;
using FixMaster.Services;
using FixMaster.Services.Interfaces;
using FixMaster.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FixMaster.Tests;

public sealed class DashboardServiceTests
{
    [Fact]
    public async Task ObterIndicadoresDoDiaAsync_ContaAgendamentosPendentesEmAndamentoEConcluidos()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var cliente = await ambiente.CriarClienteAsync();
        var hoje = DateTime.Today;

        Assert.True((await ambiente.Servico.SalvarAsync(CriarAgendamento(cliente.Id, hoje.AddHours(9), 60))).Sucesso);
        Assert.True((await ambiente.Servico.SalvarAsync(CriarAgendamento(cliente.Id, hoje.AddHours(10), 60))).Sucesso);
        Assert.True((await ambiente.Servico.SalvarAsync(CriarAgendamento(cliente.Id, hoje.AddDays(1).AddHours(9), 60))).Sucesso);

        var agendamentoEmAndamento = CriarAgendamento(cliente.Id, hoje.AddHours(11), 60);
        agendamentoEmAndamento.Status = StatusAgendamento.EmAndamento;
        Assert.True((await ambiente.Servico.SalvarAsync(agendamentoEmAndamento)).Sucesso);

        var agendamentoConcluido = CriarAgendamento(cliente.Id, hoje.AddHours(12), 60);
        agendamentoConcluido.Status = StatusAgendamento.Concluido;
        Assert.True((await ambiente.Servico.SalvarAsync(agendamentoConcluido)).Sucesso);

        var cancelado = CriarAgendamento(cliente.Id, hoje.AddHours(13), 60);
        Assert.True((await ambiente.Servico.SalvarAsync(cancelado)).Sucesso);
        Assert.True((await ambiente.Servico.AlterarStatusAsync(cancelado.Id, StatusAgendamento.Cancelado)).Sucesso);

        var resultado = await ambiente.Dashboard.ObterIndicadoresDoDiaAsync();

        Assert.True(resultado.Sucesso, resultado.Mensagem);
        Assert.Equal(4, resultado.Valor!.Marcados);
        Assert.Equal(2, resultado.Valor.Pendentes);
        Assert.Equal(1, resultado.Valor.EmAndamento);
        Assert.Equal(1, resultado.Valor.Concluidos);
    }

    [Fact]
    public async Task ObterAtividadesRecentesAsync_ListaEmOrdemDecrescenteDeDataHora()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        await ambiente.Atividades.RegistrarAsync("LOG", "Primeira", 1);
        await ambiente.Atividades.RegistrarAsync("OS", "Segunda", 2);

        var resultado = await ambiente.Dashboard.ObterAtividadesRecentesAsync(10);

        Assert.True(resultado.Sucesso, resultado.Mensagem);
        Assert.Equal(2, resultado.Valor!.Count);
        Assert.Equal("OS", resultado.Valor.First().Tipo);
        Assert.Equal("Primeira", resultado.Valor.Last().Descricao);
    }

    [Fact]
    public async Task ObterAgendamentosDoDiaAsync_ListaEmOrdemCrescenteEIndicaOsVinculadas()
    {
        await using var ambiente = await AmbienteTeste.CriarAsync();
        var cliente = await ambiente.CriarClienteAsync();
        var hoje = DateTime.Today;
        var primeiro = CriarAgendamento(cliente.Id, hoje.AddHours(8), 60);
        var comOrdem = CriarAgendamento(cliente.Id, hoje.AddHours(10), 60);
        var terceiro = CriarAgendamento(cliente.Id, hoje.AddHours(12), 60);
        var cancelado = CriarAgendamento(cliente.Id, hoje.AddHours(14), 60);

        Assert.True((await ambiente.Servico.SalvarAsync(primeiro)).Sucesso);
        Assert.True((await ambiente.Servico.SalvarAsync(comOrdem)).Sucesso);
        Assert.True((await ambiente.Servico.SalvarAsync(terceiro)).Sucesso);
        Assert.True((await ambiente.Servico.SalvarAsync(cancelado)).Sucesso);
        Assert.True((await ambiente.Servico.AlterarStatusAsync(
            cancelado.Id, StatusAgendamento.Cancelado)).Sucesso);
        Assert.True((await ambiente.Ordens.CriarAsync(new OrdemServico
        {
            AgendamentoId = comOrdem.Id,
            RelatorioTecnico = "Inspeção inicial"
        })).Sucesso);
        Assert.True((await ambiente.Servico.SalvarAsync(
            CriarAgendamento(cliente.Id, hoje.AddDays(1).AddHours(9), 60))).Sucesso);

        var resultado = await ambiente.Dashboard.ObterAgendamentosDoDiaAsync();

        Assert.True(resultado.Sucesso, resultado.Mensagem);
        Assert.Equal(4, resultado.Valor!.Count);
        Assert.Equal(
            new[] { primeiro.Id, comOrdem.Id, terceiro.Id, cancelado.Id },
            resultado.Valor.Select(agendamento => agendamento.Id));
        Assert.True(resultado.Valor[1].PossuiOrdemServico);
        Assert.False(resultado.Valor[0].PossuiOrdemServico);
        Assert.Equal(StatusAgendamento.Cancelado, resultado.Valor[3].Status);
    }

    [Fact]
    public async Task CarregarDashboardAsync_ListaParaAbrirSomenteAgendamentosAtivosSemOs()
    {
        var inicio = DateTime.Today.AddHours(9);
        var agendamentos = new[]
        {
            CriarResumo(1, inicio, StatusAgendamento.Pendente),
            CriarResumo(2, inicio.AddHours(1), StatusAgendamento.EmAndamento),
            CriarResumo(3, inicio.AddHours(2), StatusAgendamento.Concluido),
            CriarResumo(4, inicio.AddHours(3), StatusAgendamento.Cancelado),
            CriarResumo(5, inicio.AddHours(4), StatusAgendamento.Pendente, possuiOrdemServico: true)
        };
        var viewModel = new DashboardViewModel(
            new DashboardFalso(agendamentos),
            _ => Task.CompletedTask,
            _ => Task.CompletedTask,
            _ => Task.CompletedTask);

        await viewModel.CarregarAsync();

        Assert.Equal(5, viewModel.AgendamentosDoDia.Count);
        Assert.Equal(new[] { 1, 2 }, viewModel.OrdensParaAbrir.Select(item => item.Id));
    }

    private static AgendamentoResumo CriarResumo(
        int id,
        DateTime inicio,
        StatusAgendamento status,
        bool possuiOrdemServico = false) =>
        new(id, 1, "Cliente", inicio, inicio.AddHours(1), 60, "Serviço", status, possuiOrdemServico);

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
            Dashboard = new DashboardService(fabrica);
            Atividades = new AtividadeService(fabrica);
            Servico = new AgendaService(fabrica);
            Ordens = new OrdemServicoService(fabrica);
            Clientes = new ClienteService(fabrica);
        }

        private FabricaContexto Fabrica { get; }
        public DashboardService Dashboard { get; }
        public AtividadeService Atividades { get; }
        public AgendaService Servico { get; }
        public OrdemServicoService Ordens { get; }
        public ClienteService Clientes { get; }

        public static async Task<AmbienteTeste> CriarAsync()
        {
            var conexao = new SqliteConnection(
                $"Data Source=FixMasterDashboardTests_{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
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

    private sealed class DashboardFalso(IReadOnlyList<AgendamentoResumo> agendamentos)
        : IDashboardService
    {
        public Task<Resultado<DashboardIndicadores>> ObterIndicadoresDoDiaAsync() =>
            Task.FromResult(Resultado<DashboardIndicadores>.Ok(DashboardIndicadores.Vazio));

        public Task<Resultado<IReadOnlyList<AgendamentoResumo>>> ObterAgendamentosDoDiaAsync() =>
            Task.FromResult(Resultado<IReadOnlyList<AgendamentoResumo>>.Ok(agendamentos));

        public Task<Resultado<IReadOnlyList<AtividadeRecente>>> ObterAtividadesRecentesAsync(
            int quantidade = 20) =>
            Task.FromResult(Resultado<IReadOnlyList<AtividadeRecente>>.Ok(
                Array.Empty<AtividadeRecente>()));
    }
}
