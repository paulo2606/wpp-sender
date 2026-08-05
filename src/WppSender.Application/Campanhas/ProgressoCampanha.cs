namespace WppSender.Application.Campanhas;

public record ProgressoCampanha(int Pendente, int Enviado, int Entregue, int Lido, int Falhou, int FalhouEntrega);
