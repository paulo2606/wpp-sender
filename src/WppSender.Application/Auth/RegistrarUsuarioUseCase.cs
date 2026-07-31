using WppSender.Domain;

namespace WppSender.Application.Auth;

public class RegistrarUsuarioUseCase
{
    private const string MensagemCadastroIndisponivel = "Cadastro indisponível";

    private readonly IUsuarioRepository _repositorio;
    private readonly IPasswordHasher _hasher;

    public RegistrarUsuarioUseCase(IUsuarioRepository repositorio, IPasswordHasher hasher)
    {
        _repositorio = repositorio;
        _hasher = hasher;
    }

    public async Task<RegistroResult> ExecutarAsync(string email, string senha)
    {
        if (await _repositorio.ExisteAlgumAsync())
        {
            return RegistroResult.Falha(MensagemCadastroIndisponivel);
        }

        var usuario = new Usuario(Guid.NewGuid(), email, _hasher.Hash(senha));
        await _repositorio.AdicionarAsync(usuario);

        return RegistroResult.ComSucesso();
    }
}
