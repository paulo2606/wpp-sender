namespace WppSender.Domain;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarPorEmailAsync(string email);
    Task<bool> ExisteAlgumAsync();
    Task AdicionarAsync(Usuario usuario);
    Task<bool> AdicionarSeForOPrimeiroAsync(Usuario usuario);
}
