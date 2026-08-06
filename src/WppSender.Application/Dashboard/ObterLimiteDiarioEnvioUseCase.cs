using WppSender.Domain;

namespace WppSender.Application.Dashboard;

public record LimiteDiarioEnvio(int LimiteDiario, int EnviosRealizadosHoje, int PercentualUsado);

public class ObterLimiteDiarioEnvioUseCase
{
    private readonly IConfiguracaoEnvioRepository _repositorio;
    private readonly IRelogio _relogio;

    public ObterLimiteDiarioEnvioUseCase(IConfiguracaoEnvioRepository repositorio, IRelogio relogio)
    {
        _repositorio = repositorio;
        _relogio = relogio;
    }

    public async Task<LimiteDiarioEnvio> ExecutarAsync()
    {
        var config = await _repositorio.ObterAsync();
        var hoje = DateOnly.FromDateTime(_relogio.AgoraUtc());

        var enviosHoje = config.DataReferencia == hoje ? config.EnviosRealizadosHoje : 0;
        var percentual = config.LimiteDiarioEnvios > 0
            ? (int)Math.Round(enviosHoje * 100.0 / config.LimiteDiarioEnvios)
            : 0;

        return new LimiteDiarioEnvio(config.LimiteDiarioEnvios, enviosHoje, percentual);
    }
}
