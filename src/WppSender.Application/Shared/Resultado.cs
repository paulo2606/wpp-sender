namespace WppSender.Application.Shared;

public class Resultado
{
    public bool Sucesso { get; }
    public string? MensagemErro { get; }

    private Resultado(bool sucesso, string? mensagemErro)
    {
        Sucesso = sucesso;
        MensagemErro = mensagemErro;
    }

    public static Resultado ComSucesso() => new(true, null);

    public static Resultado Falha(string mensagem) => new(false, mensagem);
}

public class Resultado<TErro>
    where TErro : struct
{
    public bool Sucesso { get; }
    public string? MensagemErro { get; }
    public TErro? Erro { get; }

    private Resultado(bool sucesso, string? mensagemErro, TErro? erro)
    {
        Sucesso = sucesso;
        MensagemErro = mensagemErro;
        Erro = erro;
    }

    public static Resultado<TErro> ComSucesso() => new(true, null, null);

    public static Resultado<TErro> Falha(string mensagem, TErro? erro = null) => new(false, mensagem, erro);
}

public class ResultadoComValor<TValor>
{
    public bool Sucesso { get; }
    public TValor? Valor { get; }
    public string? MensagemErro { get; }

    private ResultadoComValor(bool sucesso, TValor? valor, string? mensagemErro)
    {
        Sucesso = sucesso;
        Valor = valor;
        MensagemErro = mensagemErro;
    }

    public static ResultadoComValor<TValor> ComSucesso(TValor valor) => new(true, valor, null);

    public static ResultadoComValor<TValor> Falha(string mensagem) => new(false, default, mensagem);
}
