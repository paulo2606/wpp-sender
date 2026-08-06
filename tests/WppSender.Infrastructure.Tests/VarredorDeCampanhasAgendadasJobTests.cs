using WppSender.Application.Campanhas;
using WppSender.Domain;
using WppSender.Infrastructure.Jobs;
using Xunit;

namespace WppSender.Infrastructure.Tests;

public class VarredorDeCampanhasAgendadasJobTests
{
    private class FakeCampanhaRepository : ICampanhaRepository
    {
        public List<Campanha> Campanhas { get; } = new();
        public List<Campanha> Atualizadas { get; } = new();

        public Task<Campanha?> BuscarPorIdAsync(Guid id) =>
            Task.FromResult(Campanhas.FirstOrDefault(c => c.Id == id));

        public Task AdicionarAsync(Campanha campanha)
        {
            Campanhas.Add(campanha);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(Campanha campanha)
        {
            Atualizadas.Add(campanha);
            return Task.CompletedTask;
        }

        public Task RemoverAsync(Campanha campanha)
        {
            Campanhas.Remove(campanha);
            return Task.CompletedTask;
        }

        public Task<(IReadOnlyList<Campanha> Itens, int Total)> ListarAsync(StatusCampanha? status, int pagina, int tamanhoPagina) =>
            Task.FromResult<(IReadOnlyList<Campanha>, int)>((Campanhas, Campanhas.Count));

        public Task<IReadOnlyList<Campanha>> ListarAgendadasParaIniciarAsync(DateTime agora)
        {
            var vencidas = Campanhas
                .Where(c => c.Status == StatusCampanha.Agendada && c.AgendadoPara != null && c.AgendadoPara <= agora)
                .ToList();
            return Task.FromResult<IReadOnlyList<Campanha>>(vencidas);
        }

        public Task<IReadOnlyDictionary<StatusCampanha, int>> ContarPorStatusAsync()
        {
            var contagens = Campanhas.GroupBy(c => c.Status).ToDictionary(g => g.Key, g => g.Count());
            return Task.FromResult<IReadOnlyDictionary<StatusCampanha, int>>(contagens);
        }

        public Task<Campanha?> ObterProximaAgendadaAsync()
        {
            var resultado = Campanhas
                .Where(c => c.Status == StatusCampanha.Agendada && c.AgendadoPara != null)
                .OrderBy(c => c.AgendadoPara)
                .FirstOrDefault();
            return Task.FromResult(resultado);
        }
    }

    private class FakeSessaoWhatsAppRepository : ISessaoWhatsAppRepository
    {
        private SessaoWhatsApp _sessao = new(StatusSessaoWhatsApp.Desconectado);

        public Task<SessaoWhatsApp> ObterAsync() => Task.FromResult(_sessao);

        public Task AtualizarAsync(SessaoWhatsApp sessao)
        {
            _sessao = sessao;
            return Task.CompletedTask;
        }
    }

    private class FakeCampanhaJobScheduler : ICampanhaJobScheduler
    {
        public List<Guid> Agendamentos { get; } = new();

        public Task AgendarProximoEnvioAsync(Guid campanhaId, TimeSpan atraso)
        {
            Agendamentos.Add(campanhaId);
            return Task.CompletedTask;
        }
    }

    private class FakeRelogio : IRelogio
    {
        public DateTime Agora { get; set; } = new DateTime(2026, 8, 2, 12, 0, 0, DateTimeKind.Utc);

        public DateTime AgoraUtc() => Agora;
    }

    [Fact]
    public async Task DeveIniciarCampanhaEAgendarPrimeiroPasso_QuandoVencidaESessaoConectada()
    {
        var campanhaRepositorio = new FakeCampanhaRepository();
        var sessaoRepositorio = new FakeSessaoWhatsAppRepository();
        var scheduler = new FakeCampanhaJobScheduler();
        var relogio = new FakeRelogio();

        var campanha = new Campanha(Guid.NewGuid(), "Campanha", "Msg", Guid.NewGuid(), relogio.Agora.AddMinutes(-5));
        campanhaRepositorio.Campanhas.Add(campanha);
        await sessaoRepositorio.AtualizarAsync(new SessaoWhatsApp(StatusSessaoWhatsApp.Conectado));

        var job = new VarredorDeCampanhasAgendadasJob(campanhaRepositorio, sessaoRepositorio, scheduler, relogio);

        await job.ExecutarAsync();

        Assert.Equal(StatusCampanha.EmAndamento, campanha.Status);
        Assert.Single(scheduler.Agendamentos);
        Assert.Equal(campanha.Id, scheduler.Agendamentos[0]);
    }

    [Fact]
    public async Task DevePausarCampanhaENaoAgendar_QuandoVencidaESessaoNaoConectada()
    {
        var campanhaRepositorio = new FakeCampanhaRepository();
        var sessaoRepositorio = new FakeSessaoWhatsAppRepository();
        var scheduler = new FakeCampanhaJobScheduler();
        var relogio = new FakeRelogio();

        var campanha = new Campanha(Guid.NewGuid(), "Campanha", "Msg", Guid.NewGuid(), relogio.Agora.AddMinutes(-5));
        campanhaRepositorio.Campanhas.Add(campanha);
        await sessaoRepositorio.AtualizarAsync(new SessaoWhatsApp(StatusSessaoWhatsApp.Desconectado));

        var job = new VarredorDeCampanhasAgendadasJob(campanhaRepositorio, sessaoRepositorio, scheduler, relogio);

        await job.ExecutarAsync();

        Assert.Equal(StatusCampanha.Pausada, campanha.Status);
        Assert.Empty(scheduler.Agendamentos);
    }
}
