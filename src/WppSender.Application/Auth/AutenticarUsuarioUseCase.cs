using WppSender.Domain;

namespace WppSender.Application.Auth;

public class AutenticarUsuarioUseCase
{
    private const string MensagemCredenciaisInvalidas = "Email ou senha inválidos";

    private readonly IUsuarioRepository _repositorio;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AutenticarUsuarioUseCase(IUsuarioRepository repositorio, IPasswordHasher hasher, IJwtTokenGenerator tokenGenerator)
    {
        _repositorio = repositorio;
        _hasher = hasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AutenticacaoResult> ExecutarAsync(string email, string senha)
    {
        var usuario = await _repositorio.BuscarPorEmailAsync(email);
        if (usuario is null || !usuario.Autenticar(senha, _hasher))
        {
            return AutenticacaoResult.Falha(MensagemCredenciaisInvalidas);
        }

        var token = _tokenGenerator.GerarToken(usuario);
        return AutenticacaoResult.ComSucesso(token);
    }
}
