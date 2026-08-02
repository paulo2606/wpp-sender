using WppSender.Domain;

namespace WppSender.Application.Campanhas;

public record CampanhaComProgresso(CampanhaResumo Campanha, ProgressoCampanha Progresso);

public class ObterCampanhaUseCase
{
    private readonly ICampanhaRepository _campanhaRepositorio;
    private readonly IEnvioRepository _envioRepositorio;

    public ObterCampanhaUseCase(ICampanhaRepository campanhaRepositorio, IEnvioRepository envioRepositorio)
    {
        _campanhaRepositorio = campanhaRepositorio;
        _envioRepositorio = envioRepositorio;
    }

    public async Task<CampanhaComProgresso?> ExecutarAsync(Guid id)
    {
        var campanha = await _campanhaRepositorio.BuscarPorIdAsync(id);
        if (campanha is null)
        {
            return null;
        }

        var contagens = await _envioRepositorio.ContarPorStatusAsync(id);
        var progresso = new ProgressoCampanha(
            contagens.GetValueOrDefault(StatusEnvio.Pendente),
            contagens.GetValueOrDefault(StatusEnvio.Enviado),
            contagens.GetValueOrDefault(StatusEnvio.Falhou));

        return new CampanhaComProgresso(CampanhaResumo.DeCampanha(campanha), progresso);
    }
}
