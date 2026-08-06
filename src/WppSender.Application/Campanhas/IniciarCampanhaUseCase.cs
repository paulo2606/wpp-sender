using WppSender.Domain;
using WppSender.Application.Shared;

namespace WppSender.Application.Campanhas;

public class IniciarCampanhaUseCase
{
    private const string MensagemNaoEncontrada = "Campanha não encontrada";
    private const string MensagemStatusInvalido = "Campanha só pode ser iniciada a partir de rascunho ou agendada";
    private const string MensagemSessaoDesconectada = "É necessário conectar a sessão do WhatsApp antes de iniciar a campanha";

    private readonly ICampanhaRepository _campanhaRepositorio;
    private readonly ISessaoWhatsAppRepository _sessaoRepositorio;
    private readonly ICampanhaJobScheduler _jobScheduler;

    public IniciarCampanhaUseCase(ICampanhaRepository campanhaRepositorio, ISessaoWhatsAppRepository sessaoRepositorio, ICampanhaJobScheduler jobScheduler)
    {
        _campanhaRepositorio = campanhaRepositorio;
        _sessaoRepositorio = sessaoRepositorio;
        _jobScheduler = jobScheduler;
    }

    public async Task<Resultado<IniciarCampanhaErro>> ExecutarAsync(Guid campanhaId)
    {
        var campanha = await _campanhaRepositorio.BuscarPorIdAsync(campanhaId);
        if (campanha is null)
        {
            return Resultado<IniciarCampanhaErro>.Falha(MensagemNaoEncontrada, IniciarCampanhaErro.NaoEncontrada);
        }

        var sessao = await _sessaoRepositorio.ObterAsync();
        if (sessao.Status != StatusSessaoWhatsApp.Conectado)
        {
            return Resultado<IniciarCampanhaErro>.Falha(MensagemSessaoDesconectada, IniciarCampanhaErro.SessaoDesconectada);
        }

        try
        {
            campanha.Iniciar();
        }
        catch (InvalidOperationException)
        {
            return Resultado<IniciarCampanhaErro>.Falha(MensagemStatusInvalido, IniciarCampanhaErro.StatusInvalido);
        }

        await _campanhaRepositorio.AtualizarAsync(campanha);
        await _jobScheduler.AgendarProximoEnvioAsync(campanha.Id, TimeSpan.Zero);

        return Resultado<IniciarCampanhaErro>.ComSucesso();
    }
}
