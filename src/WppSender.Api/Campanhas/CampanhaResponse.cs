using WppSender.Domain;

namespace WppSender.Api.Campanhas;

public record CampanhaResponse(
    Guid Id,
    string Nome,
    string Mensagem,
    Guid GrupoId,
    StatusCampanha Status,
    DateTime? AgendadoPara,
    int IntervaloMinSegundos,
    int IntervaloMaxSegundos);
