namespace WppSender.Domain;

public class Envio
{
    public Guid Id { get; private set; }
    public Guid CampanhaId { get; private set; }
    public Guid LeadId { get; private set; }
    public StatusEnvio Status { get; private set; }
    public string? WhatsAppMensagemId { get; private set; }
    public DateTime? EnviadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }
    public string? Erro { get; private set; }

    public Envio(Guid id, Guid campanhaId, Guid leadId)
    {
        Id = id;
        CampanhaId = campanhaId;
        LeadId = leadId;
        Status = StatusEnvio.Pendente;
    }

    private Envio() { }

    public void MarcarComoEnviado(DateTime agora, string? whatsAppMensagemId)
    {
        Status = StatusEnvio.Enviado;
        WhatsAppMensagemId = whatsAppMensagemId;
        EnviadoEm = agora;
        AtualizadoEm = agora;
        Erro = null;
    }

    public void MarcarComoEntregue(DateTime agora)
    {
        ExigirStatusEnviado(nameof(MarcarComoEntregue));
        Status = StatusEnvio.Entregue;
        AtualizadoEm = agora;
    }

    public void MarcarComoLido(DateTime agora)
    {
        if (Status is not (StatusEnvio.Enviado or StatusEnvio.Entregue))
        {
            throw new InvalidOperationException("Envio só pode ser marcado como lido a partir de enviado ou entregue");
        }

        Status = StatusEnvio.Lido;
        AtualizadoEm = agora;
    }

    public void MarcarComoFalhouEntrega(string motivo, DateTime agora)
    {
        ExigirStatusEnviado(nameof(MarcarComoFalhouEntrega));
        Status = StatusEnvio.FalhouEntrega;
        Erro = motivo;
        AtualizadoEm = agora;
    }

    public void MarcarComoFalhou(string motivo)
    {
        Status = StatusEnvio.Falhou;
        Erro = motivo;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Resetar()
    {
        Status = StatusEnvio.Pendente;
        WhatsAppMensagemId = null;
        EnviadoEm = null;
        Erro = null;
        AtualizadoEm = null;
    }

    private void ExigirStatusEnviado(string operacao)
    {
        if (Status != StatusEnvio.Enviado)
        {
            throw new InvalidOperationException($"{operacao} só é permitido a partir do status Enviado");
        }
    }
}
