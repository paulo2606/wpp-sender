namespace WppSender.Domain;

public interface IGrupoRepository
{
    Task<Grupo?> BuscarPorIdAsync(Guid id);
    Task AdicionarAsync(Grupo grupo);
    Task AtualizarAsync(Grupo grupo);
    Task RemoverAsync(Grupo grupo);
    Task<(IReadOnlyList<(Grupo Grupo, int QuantidadeLeads)> Itens, int Total)> ListarComContagemAsync(int pagina, int tamanhoPagina);
    Task<IReadOnlyList<Grupo>> ListarTodosAsync();
}
