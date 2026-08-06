using WppSender.Domain;

namespace WppSender.Application.Campanhas;

public class ProcessarProximoEnvioUseCase
{
    private readonly ICampanhaRepository _campanhaRepositorio;
    private readonly IEnvioRepository _envioRepositorio;
    private readonly ILeadRepository _leadRepositorio;
    private readonly IConfiguracaoEnvioRepository _configRepositorio;
    private readonly ISessaoWhatsAppRepository _sessaoRepositorio;
    private readonly IWhatsAppClient _whatsAppClient;
    private readonly ICampanhaJobScheduler _jobScheduler;
    private readonly IRelogio _relogio;

    public ProcessarProximoEnvioUseCase(
        ICampanhaRepository campanhaRepositorio,
        IEnvioRepository envioRepositorio,
        ILeadRepository leadRepositorio,
        IConfiguracaoEnvioRepository configRepositorio,
        ISessaoWhatsAppRepository sessaoRepositorio,
        IWhatsAppClient whatsAppClient,
        ICampanhaJobScheduler jobScheduler,
        IRelogio relogio)
    {
        _campanhaRepositorio = campanhaRepositorio;
        _envioRepositorio = envioRepositorio;
        _leadRepositorio = leadRepositorio;
        _configRepositorio = configRepositorio;
        _sessaoRepositorio = sessaoRepositorio;
        _whatsAppClient = whatsAppClient;
        _jobScheduler = jobScheduler;
        _relogio = relogio;
    }

    public async Task ExecutarAsync(Guid campanhaId)
    {
        var campanha = await _campanhaRepositorio.BuscarPorIdAsync(campanhaId);
        if (campanha is null || campanha.Status != StatusCampanha.EmAndamento)
        {
            return;
        }

        var sessao = await _sessaoRepositorio.ObterAsync();
        if (sessao.Status != StatusSessaoWhatsApp.Conectado)
        {
            campanha.Pausar();
            await _campanhaRepositorio.AtualizarAsync(campanha);
            return;
        }

        var envio = await _envioRepositorio.BuscarProximoPendenteAsync(campanhaId);
        if (envio is null)
        {
            var contagensAtuais = await _envioRepositorio.ContarPorStatusAsync(campanhaId);
            campanha.Concluir(TemFalha(contagensAtuais));
            await _campanhaRepositorio.AtualizarAsync(campanha);
            return;
        }

        var hoje = DateOnly.FromDateTime(_relogio.AgoraUtc());
        var podeEnviar = await _configRepositorio.TentarRegistrarEnvioAsync(hoje);
        if (!podeEnviar)
        {
            campanha.Pausar();
            await _campanhaRepositorio.AtualizarAsync(campanha);
            return;
        }

        var lead = await _leadRepositorio.BuscarPorIdAsync(envio.LeadId);
        var mensagemFinal = campanha.Mensagem.Replace("{{nome}}", lead?.Nome ?? string.Empty);

        ResultadoEnvioMensagem resultadoEnvio;
        try
        {
            resultadoEnvio = await _whatsAppClient.EnviarMensagemAsync(lead?.TelefoneNormalizado ?? string.Empty, mensagemFinal);
        }
        catch (Exception ex)
        {

            resultadoEnvio = new ResultadoEnvioMensagem(false, ex.Message);
        }

        if (resultadoEnvio.Sucesso)
        {
            envio.MarcarComoEnviado(_relogio.AgoraUtc(), resultadoEnvio.MensagemId);
        }
        else
        {
            envio.MarcarComoFalhou(resultadoEnvio.MensagemErro ?? "Falha desconhecida");
        }

        await _envioRepositorio.AtualizarAsync(envio);

        var aindaTemPendente = await _envioRepositorio.BuscarProximoPendenteAsync(campanhaId);
        if (aindaTemPendente is null)
        {

            var contagens = await _envioRepositorio.ContarPorStatusAsync(campanhaId);
            if (contagens.GetValueOrDefault(StatusEnvio.Enviado) == 0)
            {
                campanha.Concluir(TemFalha(contagens));
                await _campanhaRepositorio.AtualizarAsync(campanha);
            }

            return;
        }

        var atrasoSegundos = Random.Shared.Next(campanha.IntervaloMinSegundos, campanha.IntervaloMaxSegundos + 1);
        await _jobScheduler.AgendarProximoEnvioAsync(campanhaId, TimeSpan.FromSeconds(atrasoSegundos));
    }

    private static bool TemFalha(IReadOnlyDictionary<StatusEnvio, int> contagens) =>
        contagens.GetValueOrDefault(StatusEnvio.Falhou) > 0 || contagens.GetValueOrDefault(StatusEnvio.FalhouEntrega) > 0;
}
