using WppSender.Domain;

namespace WppSender.Infrastructure.Tempo;

public class RelogioSistema : IRelogio
{
    public DateTime AgoraUtc() => DateTime.UtcNow;
}
