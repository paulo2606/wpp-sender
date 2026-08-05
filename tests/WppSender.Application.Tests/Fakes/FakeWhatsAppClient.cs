using WppSender.Application.Campanhas;
using WppSender.Domain;

namespace WppSender.Application.Tests.Fakes;

public class FakeWhatsAppClient : IWhatsAppClient
{
    public StatusSessaoWhatsApp StatusSessao { get; set; } = StatusSessaoWhatsApp.Desconectado;
    public bool ProximoEnvioDeveFalhar { get; set; }
    public bool ProximoEnvioDeveLancarExcecao { get; set; }
    public string MotivoFalha { get; set; } = "Falha simulada";
    public List<(string Telefone, string Mensagem)> MensagensEnviadas { get; } = new();
    public Dictionary<string, StatusEntregaMensagem> StatusPorMensagemId { get; } = new();

    public Task<ResultadoEnvioMensagem> EnviarMensagemAsync(string telefone, string mensagem)
    {
        MensagensEnviadas.Add((telefone, mensagem));

        if (ProximoEnvioDeveLancarExcecao)
        {
            throw new InvalidOperationException(MotivoFalha);
        }

        if (ProximoEnvioDeveFalhar)
        {
            return Task.FromResult(new ResultadoEnvioMensagem(false, MotivoFalha));
        }

        return Task.FromResult(new ResultadoEnvioMensagem(true, null, Guid.NewGuid().ToString()));
    }

    public Task<string> IniciarSessaoAsync() => Task.FromResult("qr-code-base64-fake");

    public Task<StatusSessaoWhatsApp> ObterStatusSessaoAsync() => Task.FromResult(StatusSessao);

    public Task<IReadOnlyDictionary<string, StatusEntregaMensagem>> ObterStatusMensagensAsync(IReadOnlyCollection<string> mensagemIds)
    {
        IReadOnlyDictionary<string, StatusEntregaMensagem> resultado = mensagemIds
            .Where(StatusPorMensagemId.ContainsKey)
            .ToDictionary(id => id, id => StatusPorMensagemId[id]);

        return Task.FromResult(resultado);
    }
}
