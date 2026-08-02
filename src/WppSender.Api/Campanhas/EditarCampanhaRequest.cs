namespace WppSender.Api.Campanhas;

public record EditarCampanhaRequest(string Nome, string Mensagem, DateTime? AgendadoPara, int IntervaloMinSegundos, int IntervaloMaxSegundos);
