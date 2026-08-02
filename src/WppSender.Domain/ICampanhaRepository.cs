namespace WppSender.Domain;

public interface ICampanhaRepository
{
    Task<Campanha?> BuscarPorIdAsync(Guid id);
    Task AdicionarAsync(Campanha campanha);
    Task AtualizarAsync(Campanha campanha);
    Task RemoverAsync(Campanha campanha);
    Task<(IReadOnlyList<Campanha> Itens, int Total)> ListarAsync(StatusCampanha? status, int pagina, int tamanhoPagina);
    Task<IReadOnlyList<Campanha>> ListarAgendadasParaIniciarAsync(DateTime agora);
}
