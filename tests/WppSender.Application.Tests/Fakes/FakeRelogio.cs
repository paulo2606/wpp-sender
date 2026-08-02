using WppSender.Domain;

namespace WppSender.Application.Tests.Fakes;

public class FakeRelogio : IRelogio
{
    public DateTime Agora { get; set; } = new(2026, 8, 2, 10, 0, 0, DateTimeKind.Utc);

    public DateTime AgoraUtc() => Agora;
}
