using WppSender.Domain;

namespace WppSender.Application.Dashboard;

public record ProximaCampanhaAgendada(Guid Id, string Nome, DateTime AgendadoPara);

public class ObterProximaCampanhaAgendadaUseCase
{
    private readonly ICampanhaRepository _repositorio;

    public ObterProximaCampanhaAgendadaUseCase(ICampanhaRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<ProximaCampanhaAgendada?> ExecutarAsync()
    {
        var campanha = await _repositorio.ObterProximaAgendadaAsync();

        return campanha is null
            ? null
            : new ProximaCampanhaAgendada(campanha.Id, campanha.Nome, campanha.AgendadoPara!.Value);
    }
}
