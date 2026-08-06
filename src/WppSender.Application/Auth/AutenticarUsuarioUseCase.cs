using WppSender.Domain;
using WppSender.Application.Shared;

namespace WppSender.Application.Auth;

public class AutenticarUsuarioUseCase
{
    private const string MensagemCredenciaisInvalidas = "Email ou senha inválidos";

    private const string HashFalsoParaEvitarVazamentoDeTempo = "$2a$11$GK9gUJEEdHzUoX1qSNbrP.leET5hEeGtV5ESAFXedkTfXAxi2fh0C";

    private readonly IUsuarioRepository _repositorio;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AutenticarUsuarioUseCase(IUsuarioRepository repositorio, IPasswordHasher hasher, IJwtTokenGenerator tokenGenerator)
    {
        _repositorio = repositorio;
        _hasher = hasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<ResultadoComValor<string>> ExecutarAsync(string email, string senha)
    {
        var usuario = await _repositorio.BuscarPorEmailAsync(email);
        var senhaConfere = usuario is not null
            ? usuario.Autenticar(senha, _hasher)
            : _hasher.Verify(senha, HashFalsoParaEvitarVazamentoDeTempo);

        if (usuario is null || !senhaConfere)
        {
            return ResultadoComValor<string>.Falha(MensagemCredenciaisInvalidas);
        }

        var token = _tokenGenerator.GerarToken(usuario);
        return ResultadoComValor<string>.ComSucesso(token);
    }
}
