using WppSender.Domain;

namespace WppSender.Application.Campanhas;

public class ListarEnviosFalhosUseCase
{
    private readonly IEnvioRepository _envioRepositorio;
    private readonly ILeadRepository _leadRepositorio;

    public ListarEnviosFalhosUseCase(IEnvioRepository envioRepositorio, ILeadRepository leadRepositorio)
    {
        _envioRepositorio = envioRepositorio;
        _leadRepositorio = leadRepositorio;
    }

    public async Task<IReadOnlyList<EnvioFalhoResumo>> ExecutarAsync(Guid campanhaId)
    {
        var falhos = await _envioRepositorio.ListarFalhosAsync(campanhaId);
        var resultado = new List<EnvioFalhoResumo>();

        foreach (var envio in falhos)
        {
            var lead = await _leadRepositorio.BuscarPorIdAsync(envio.LeadId);
            resultado.Add(new EnvioFalhoResumo(envio.Id, envio.LeadId, lead?.Nome, envio.Erro));
        }

        return resultado;
    }
}
