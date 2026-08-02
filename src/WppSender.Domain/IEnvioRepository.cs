namespace WppSender.Domain;

public interface IEnvioRepository
{
    Task AdicionarVariosAsync(IReadOnlyList<Envio> envios);
    Task AtualizarAsync(Envio envio);
    Task<Envio?> BuscarProximoPendenteAsync(Guid campanhaId);
    Task<IReadOnlyDictionary<StatusEnvio, int>> ContarPorStatusAsync(Guid campanhaId);
    Task<IReadOnlyList<Envio>> ListarFalhosAsync(Guid campanhaId);
}
