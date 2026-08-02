using WppSender.Domain;

namespace WppSender.Application.Campanhas;

public record CampanhaResumo(
    Guid Id,
    string Nome,
    string Mensagem,
    Guid GrupoId,
    StatusCampanha Status,
    DateTime? AgendadoPara,
    int IntervaloMinSegundos,
    int IntervaloMaxSegundos)
{
    public static CampanhaResumo DeCampanha(Campanha campanha) => new(
        campanha.Id,
        campanha.Nome,
        campanha.Mensagem,
        campanha.GrupoId,
        campanha.Status,
        campanha.AgendadoPara,
        campanha.IntervaloMinSegundos,
        campanha.IntervaloMaxSegundos);
}
