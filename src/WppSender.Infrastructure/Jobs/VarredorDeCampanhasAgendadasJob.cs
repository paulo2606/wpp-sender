using WppSender.Application.Campanhas;
using WppSender.Domain;

namespace WppSender.Infrastructure.Jobs;

public class VarredorDeCampanhasAgendadasJob
{
    private readonly ICampanhaRepository _campanhaRepositorio;
    private readonly ISessaoWhatsAppRepository _sessaoRepositorio;
    private readonly ICampanhaJobScheduler _jobScheduler;
    private readonly IRelogio _relogio;

    public VarredorDeCampanhasAgendadasJob(
        ICampanhaRepository campanhaRepositorio,
        ISessaoWhatsAppRepository sessaoRepositorio,
        ICampanhaJobScheduler jobScheduler,
        IRelogio relogio)
    {
        _campanhaRepositorio = campanhaRepositorio;
        _sessaoRepositorio = sessaoRepositorio;
        _jobScheduler = jobScheduler;
        _relogio = relogio;
    }

    public async Task ExecutarAsync()
    {
        var agora = _relogio.AgoraUtc();
        var vencidas = await _campanhaRepositorio.ListarAgendadasParaIniciarAsync(agora);
        if (vencidas.Count == 0)
        {
            return;
        }

        var sessao = await _sessaoRepositorio.ObterAsync();

        foreach (var campanha in vencidas)
        {
            if (sessao.Status != StatusSessaoWhatsApp.Conectado)
            {
                // Decisão explícita: sem sessão conectada no horário agendado, pausa
                // e reporta erro em vez de esperar silenciosamente pelo próximo ciclo.
                campanha.Iniciar();
                campanha.Pausar();
                await _campanhaRepositorio.AtualizarAsync(campanha);
                continue;
            }

            campanha.Iniciar();
            await _campanhaRepositorio.AtualizarAsync(campanha);
            await _jobScheduler.AgendarProximoEnvioAsync(campanha.Id, TimeSpan.Zero);
        }
    }
}
