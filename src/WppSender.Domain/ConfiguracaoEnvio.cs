namespace WppSender.Domain;

public class ConfiguracaoEnvio
{
    public int LimiteDiarioEnvios { get; private set; }
    public int EnviosRealizadosHoje { get; private set; }
    public DateOnly DataReferencia { get; private set; }

    public ConfiguracaoEnvio(int limiteDiarioEnvios, int enviosRealizadosHoje, DateOnly dataReferencia)
    {
        LimiteDiarioEnvios = limiteDiarioEnvios;
        EnviosRealizadosHoje = enviosRealizadosHoje;
        DataReferencia = dataReferencia;
    }

    private ConfiguracaoEnvio() { }

    public bool TentarRegistrarEnvio(DateOnly hoje)
    {
        if (DataReferencia != hoje)
        {
            DataReferencia = hoje;
            EnviosRealizadosHoje = 0;
        }

        if (EnviosRealizadosHoje >= LimiteDiarioEnvios)
        {
            return false;
        }

        EnviosRealizadosHoje++;
        return true;
    }
}
