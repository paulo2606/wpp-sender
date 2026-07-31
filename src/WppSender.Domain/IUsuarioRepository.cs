namespace WppSender.Domain;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarPorEmailAsync(string email);
    Task AdicionarAsync(Usuario usuario);
}
