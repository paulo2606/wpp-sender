namespace WppSender.Domain;

public class Envio
{
    public Guid Id { get; private set; }
    public Guid CampanhaId { get; private set; }
    public Guid LeadId { get; private set; }
    public StatusEnvio Status { get; private set; }
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

    public void MarcarComoEnviado(DateTime agora)
    {
        Status = StatusEnvio.Enviado;
        EnviadoEm = agora;
        AtualizadoEm = agora;
        Erro = null;
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
        EnviadoEm = null;
        Erro = null;
        AtualizadoEm = null;
    }
}
