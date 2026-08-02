using WppSender.Domain;

namespace WppSender.Application.Dashboard;

public record ContagemCampanhasPorStatus(int Rascunho, int Agendada, int EmAndamento, int Pausada, int Concluida);

public class ContarCampanhasPorStatusUseCase
{
    private readonly ICampanhaRepository _repositorio;

    public ContarCampanhasPorStatusUseCase(ICampanhaRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<ContagemCampanhasPorStatus> ExecutarAsync()
    {
        var contagens = await _repositorio.ContarPorStatusAsync();

        return new ContagemCampanhasPorStatus(
            contagens.GetValueOrDefault(StatusCampanha.Rascunho),
            contagens.GetValueOrDefault(StatusCampanha.Agendada),
            contagens.GetValueOrDefault(StatusCampanha.EmAndamento),
            contagens.GetValueOrDefault(StatusCampanha.Pausada),
            contagens.GetValueOrDefault(StatusCampanha.Concluida));
    }
}
