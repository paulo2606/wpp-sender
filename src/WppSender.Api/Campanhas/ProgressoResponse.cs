namespace WppSender.Api.Campanhas;

public record ProgressoResponse(int Pendente, int Enviado, int Entregue, int Lido, int Falhou, int FalhouEntrega);
