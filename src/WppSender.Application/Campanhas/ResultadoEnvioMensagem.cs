namespace WppSender.Application.Campanhas;

public record ResultadoEnvioMensagem(bool Sucesso, string? MensagemErro, string? MensagemId = null);
