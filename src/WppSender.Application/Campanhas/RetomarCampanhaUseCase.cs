using WppSender.Domain;
using WppSender.Application.Shared;

namespace WppSender.Application.Campanhas;

public class RetomarCampanhaUseCase
{
    private const string MensagemNaoEncontrada = "Campanha não encontrada";
    private const string MensagemStatusInvalido = "Campanha só pode ser retomada quando está pausada";
    private const string MensagemSessaoDesconectada = "É necessário conectar a sessão do WhatsApp antes de retomar a campanha";

    private readonly ICampanhaRepository _campanhaRepositorio;
    private readonly ISessaoWhatsAppRepository _sessaoRepositorio;
    private readonly ICampanhaJobScheduler _jobScheduler;

    public RetomarCampanhaUseCase(ICampanhaRepository campanhaRepositorio, ISessaoWhatsAppRepository sessaoRepositorio, ICampanhaJobScheduler jobScheduler)
    {
        _campanhaRepositorio = campanhaRepositorio;
        _sessaoRepositorio = sessaoRepositorio;
        _jobScheduler = jobScheduler;
    }

    public async Task<Resultado<RetomarCampanhaErro>> ExecutarAsync(Guid campanhaId)
    {
        var campanha = await _campanhaRepositorio.BuscarPorIdAsync(campanhaId);
        if (campanha is null)
        {
            return Resultado<RetomarCampanhaErro>.Falha(MensagemNaoEncontrada, RetomarCampanhaErro.NaoEncontrada);
        }

        var sessao = await _sessaoRepositorio.ObterAsync();
        if (sessao.Status != StatusSessaoWhatsApp.Conectado)
        {
            return Resultado<RetomarCampanhaErro>.Falha(MensagemSessaoDesconectada, RetomarCampanhaErro.SessaoDesconectada);
        }

        try
        {
            campanha.Retomar();
        }
        catch (InvalidOperationException)
        {
            return Resultado<RetomarCampanhaErro>.Falha(MensagemStatusInvalido, RetomarCampanhaErro.StatusInvalido);
        }

        await _campanhaRepositorio.AtualizarAsync(campanha);
        await _jobScheduler.AgendarProximoEnvioAsync(campanha.Id, TimeSpan.Zero);

        return Resultado<RetomarCampanhaErro>.ComSucesso();
    }
}
