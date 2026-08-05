namespace WppSender.Domain;

public interface IEnvioRepository
{
    Task AdicionarVariosAsync(IReadOnlyList<Envio> envios);
    Task AtualizarAsync(Envio envio);
    Task<Envio?> BuscarProximoPendenteAsync(Guid campanhaId);
    Task<IReadOnlyDictionary<StatusEnvio, int>> ContarPorStatusAsync(Guid campanhaId);
    Task<IReadOnlyList<Envio>> ListarFalhosAsync(Guid campanhaId);
    Task<IReadOnlyDictionary<StatusEnvio, int>> ContarTodosPorStatusAsync();

    // Envios despachados (Status = Enviado) cuja campanha ainda está sendo acompanhada
    // (EmAndamento ou Pausada) — usado pelo job de confirmação de entrega. Campanhas
    // Canceladas ficam de fora de propósito: envios já despachados delas não são mais
    // rastreados (congelados no estado em que estavam no momento do cancelamento).
    Task<IReadOnlyList<Envio>> ListarAguardandoConfirmacaoAsync();
}
