namespace WppSender.Api.Campanhas;

public record CriarCampanhaRequest(string Nome, string Mensagem, Guid GrupoId, DateTime? AgendadoPara, int IntervaloMinSegundos = 30, int IntervaloMaxSegundos = 90);
