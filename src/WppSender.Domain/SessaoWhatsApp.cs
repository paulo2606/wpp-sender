namespace WppSender.Domain;

public class SessaoWhatsApp
{
    public StatusSessaoWhatsApp Status { get; private set; }

    public SessaoWhatsApp(StatusSessaoWhatsApp status)
    {
        Status = status;
    }

    private SessaoWhatsApp() { }

    public void MarcarAguardandoQr() => Status = StatusSessaoWhatsApp.AguardandoQr;

    public void MarcarConectado() => Status = StatusSessaoWhatsApp.Conectado;

    public void MarcarDesconectado() => Status = StatusSessaoWhatsApp.Desconectado;
}
