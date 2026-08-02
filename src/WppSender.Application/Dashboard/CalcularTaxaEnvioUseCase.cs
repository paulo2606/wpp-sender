using WppSender.Domain;

namespace WppSender.Application.Dashboard;

public record TaxaEnvioResumo(int Enviado, int Falhou, int Pendente);

public class CalcularTaxaEnvioUseCase
{
    private readonly IEnvioRepository _repositorio;

    public CalcularTaxaEnvioUseCase(IEnvioRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<TaxaEnvioResumo> ExecutarAsync()
    {
        var contagens = await _repositorio.ContarTodosPorStatusAsync();

        return new TaxaEnvioResumo(
            contagens.GetValueOrDefault(StatusEnvio.Enviado),
            contagens.GetValueOrDefault(StatusEnvio.Falhou),
            contagens.GetValueOrDefault(StatusEnvio.Pendente));
    }
}
