using WppSender.Domain;

namespace WppSender.Application.Dashboard;

public class ContarLeadsRecentesUseCase
{
    private readonly ILeadRepository _repositorio;

    public ContarLeadsRecentesUseCase(ILeadRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<int> ExecutarAsync(int dias = 7)
    {
        var desde = DateTime.UtcNow.AddDays(-dias);

        return await _repositorio.ContarAtivosCriadosDesdeAsync(desde);
    }
}
