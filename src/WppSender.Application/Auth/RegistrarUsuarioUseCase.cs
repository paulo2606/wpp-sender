using WppSender.Domain;
using WppSender.Application.Shared;

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

    public async Task<Resultado> ExecutarAsync(string email, string senha)
    {
        var usuario = new Usuario(Guid.NewGuid(), email, _hasher.Hash(senha));

        var criado = await _repositorio.AdicionarSeForOPrimeiroAsync(usuario);
        if (!criado)
        {
            return Resultado.Falha(MensagemCadastroIndisponivel);
        }

        return Resultado.ComSucesso();
    }
}
