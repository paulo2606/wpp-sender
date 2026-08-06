namespace WppSender.Domain;

public class Campanha
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Mensagem { get; private set; }
    public Guid GrupoId { get; private set; }
    public StatusCampanha Status { get; private set; }
    public DateTime? AgendadoPara { get; private set; }
    public int IntervaloMinSegundos { get; private set; }
    public int IntervaloMaxSegundos { get; private set; }

    public Campanha(Guid id, string nome, string mensagem, Guid grupoId, DateTime? agendadoPara, int intervaloMinSegundos = 30, int intervaloMaxSegundos = 90)
    {
        ValidarCamposObrigatorios(nome, mensagem);
        ValidarIntervalo(intervaloMinSegundos, intervaloMaxSegundos);

        Id = id;
        Nome = nome;
        Mensagem = mensagem;
        GrupoId = grupoId;
        AgendadoPara = agendadoPara;
        IntervaloMinSegundos = intervaloMinSegundos;
        IntervaloMaxSegundos = intervaloMaxSegundos;
        Status = agendadoPara is null ? StatusCampanha.Rascunho : StatusCampanha.Agendada;
    }

    private Campanha()
    {
        Nome = null!;
        Mensagem = null!;
    }

    public bool PodeEditar() => Status is StatusCampanha.Rascunho or StatusCampanha.Agendada;

    public bool PodeExcluir() => Status is StatusCampanha.Rascunho or StatusCampanha.Agendada;

    public void AtualizarDados(string nome, string mensagem, DateTime? agendadoPara, int intervaloMinSegundos, int intervaloMaxSegundos)
    {
        ValidarCamposObrigatorios(nome, mensagem);
        ValidarIntervalo(intervaloMinSegundos, intervaloMaxSegundos);

        Nome = nome;
        Mensagem = mensagem;
        AgendadoPara = agendadoPara;
        IntervaloMinSegundos = intervaloMinSegundos;
        IntervaloMaxSegundos = intervaloMaxSegundos;
        Status = agendadoPara is null ? StatusCampanha.Rascunho : StatusCampanha.Agendada;
    }

    public void Iniciar()
    {
        if (Status is not (StatusCampanha.Rascunho or StatusCampanha.Agendada))
        {
            throw new InvalidOperationException("Campanha só pode ser iniciada a partir de rascunho ou agendada");
        }

        Status = StatusCampanha.EmAndamento;
    }

    public void Pausar()
    {
        if (Status != StatusCampanha.EmAndamento)
        {
            throw new InvalidOperationException("Campanha só pode ser pausada quando está em andamento");
        }

        Status = StatusCampanha.Pausada;
    }

    public void Retomar()
    {
        if (Status != StatusCampanha.Pausada)
        {
            throw new InvalidOperationException("Campanha só pode ser retomada quando está pausada");
        }

        Status = StatusCampanha.EmAndamento;
    }

    public void Concluir(bool comFalhas)
    {
        if (Status != StatusCampanha.EmAndamento)
        {
            throw new InvalidOperationException("Campanha só pode ser concluída quando está em andamento");
        }

        Status = comFalhas ? StatusCampanha.ConcluidaComFalhas : StatusCampanha.Concluida;
    }

    public void Cancelar()
    {
        if (Status != StatusCampanha.Pausada)
        {
            throw new InvalidOperationException("Campanha só pode ser cancelada quando está pausada");
        }

        Status = StatusCampanha.Cancelada;
    }

    public void ReabrirParaReenvio()
    {
        if (Status is StatusCampanha.Concluida or StatusCampanha.ConcluidaComFalhas)
        {
            Status = StatusCampanha.EmAndamento;
        }
    }

    public static void ValidarCamposObrigatorios(string nome, string mensagem)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome é obrigatório", nameof(nome));
        }

        if (string.IsNullOrWhiteSpace(mensagem))
        {
            throw new ArgumentException("Mensagem é obrigatória", nameof(mensagem));
        }
    }

    public static void ValidarIntervalo(int intervaloMinSegundos, int intervaloMaxSegundos)
    {
        if (intervaloMinSegundos < 0)
        {
            throw new ArgumentException("Intervalo mínimo não pode ser negativo", nameof(intervaloMinSegundos));
        }

        if (intervaloMaxSegundos < intervaloMinSegundos)
        {
            throw new ArgumentException("Intervalo máximo não pode ser menor que o mínimo", nameof(intervaloMaxSegundos));
        }
    }
}
