using WppSender.Domain;

namespace WppSender.Application.Campanhas;

public class AtualizarStatusEntregaUseCase
{
    private const string MotivoErroReportado = "Falha reportada pelo serviço de WhatsApp";
    private const string MotivoTimeout = "Sem confirmação de entrega após timeout";

    private readonly IEnvioRepository _envioRepositorio;
    private readonly ICampanhaRepository _campanhaRepositorio;
    private readonly IWhatsAppClient _whatsAppClient;
    private readonly IRelogio _relogio;
    private readonly int _timeoutConfirmacaoMinutos;

    public AtualizarStatusEntregaUseCase(
        IEnvioRepository envioRepositorio,
        ICampanhaRepository campanhaRepositorio,
        IWhatsAppClient whatsAppClient,
        IRelogio relogio,
        int timeoutConfirmacaoMinutos)
    {
        _envioRepositorio = envioRepositorio;
        _campanhaRepositorio = campanhaRepositorio;
        _whatsAppClient = whatsAppClient;
        _relogio = relogio;
        _timeoutConfirmacaoMinutos = timeoutConfirmacaoMinutos;
    }

    public async Task ExecutarAsync()
    {
        var aguardando = await _envioRepositorio.ListarAguardandoConfirmacaoAsync();
        if (aguardando.Count == 0)
        {
            return;
        }

        var idsComMensagem = aguardando
            .Where(e => !string.IsNullOrEmpty(e.WhatsAppMensagemId))
            .Select(e => e.WhatsAppMensagemId!)
            .ToList();

        var statusPorId = idsComMensagem.Count > 0
            ? await _whatsAppClient.ObterStatusMensagensAsync(idsComMensagem)
            : new Dictionary<string, StatusEntregaMensagem>();

        var agora = _relogio.AgoraUtc();
        var campanhasAfetadas = new HashSet<Guid>();

        foreach (var envio in aguardando)
        {
            campanhasAfetadas.Add(envio.CampanhaId);

            var temStatusConclusivo = envio.WhatsAppMensagemId is not null
                && statusPorId.TryGetValue(envio.WhatsAppMensagemId, out var status)
                && status != StatusEntregaMensagem.Pendente;

            if (!temStatusConclusivo)
            {
                await AplicarTimeoutSeVencidoAsync(envio, agora);
                continue;
            }

            switch (statusPorId[envio.WhatsAppMensagemId!])
            {
                case StatusEntregaMensagem.Entregue:
                    envio.MarcarComoEntregue(agora);
                    break;
                case StatusEntregaMensagem.Lido:
                    envio.MarcarComoLido(agora);
                    break;
                case StatusEntregaMensagem.Erro:
                    envio.MarcarComoFalhouEntrega(MotivoErroReportado, agora);
                    break;
            }

            await _envioRepositorio.AtualizarAsync(envio);
        }

        foreach (var campanhaId in campanhasAfetadas)
        {
            await TentarConcluirAsync(campanhaId);
        }
    }

    private async Task AplicarTimeoutSeVencidoAsync(Envio envio, DateTime agora)
    {
        if (envio.EnviadoEm is null || agora - envio.EnviadoEm.Value < TimeSpan.FromMinutes(_timeoutConfirmacaoMinutos))
        {
            return;
        }

        envio.MarcarComoFalhouEntrega(MotivoTimeout, agora);
        await _envioRepositorio.AtualizarAsync(envio);
    }

    private async Task TentarConcluirAsync(Guid campanhaId)
    {
        var campanha = await _campanhaRepositorio.BuscarPorIdAsync(campanhaId);
        if (campanha is null || campanha.Status != StatusCampanha.EmAndamento)
        {
            return;
        }

        var contagens = await _envioRepositorio.ContarPorStatusAsync(campanhaId);
        var pendente = contagens.GetValueOrDefault(StatusEnvio.Pendente);
        var enviado = contagens.GetValueOrDefault(StatusEnvio.Enviado);
        if (pendente > 0 || enviado > 0)
        {
            return;
        }

        var comFalhas = contagens.GetValueOrDefault(StatusEnvio.Falhou) > 0
            || contagens.GetValueOrDefault(StatusEnvio.FalhouEntrega) > 0;
        campanha.Concluir(comFalhas);
        await _campanhaRepositorio.AtualizarAsync(campanha);
    }
}
