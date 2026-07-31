using WppSender.Domain;

namespace WppSender.Application.Tests.Fakes;

public class FakeUsuarioRepository : IUsuarioRepository
{
    private readonly List<Usuario> _usuarios = new();

    public Task<Usuario?> BuscarPorEmailAsync(string email)
        => Task.FromResult(_usuarios.FirstOrDefault(u => u.Email == email));

    public Task<bool> ExisteAlgumAsync()
        => Task.FromResult(_usuarios.Count > 0);

    public Task AdicionarAsync(Usuario usuario)
    {
        _usuarios.Add(usuario);
        return Task.CompletedTask;
    }
}
