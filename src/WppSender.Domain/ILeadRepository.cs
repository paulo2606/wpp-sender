namespace WppSender.Domain;

public interface ILeadRepository
{
    Task<Lead?> BuscarPorTelefoneNormalizadoAsync(string telefoneNormalizado);
    Task<Lead?> BuscarPorIdAsync(Guid id);
    Task AdicionarAsync(Lead lead);
    Task AtualizarAsync(Lead lead);
    Task<(IReadOnlyList<Lead> Itens, int Total)> ListarAsync(string? busca, int pagina, int tamanhoPagina, Guid? grupoId = null);
    Task<IReadOnlyList<Lead>> ListarAtivosPorGrupoAsync(Guid grupoId);
}
