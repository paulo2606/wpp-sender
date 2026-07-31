using WppSender.Domain;

namespace WppSender.Application.Auth;

public class AutenticarUsuarioUseCase
{
    private const string MensagemCredenciaisInvalidas = "Email ou senha inválidos";

    // Hash bcrypt válido de uma senha fixa, nunca correspondida por nenhuma senha real.
    // Usado só pra manter o custo computacional do Verify constante quando o email não
    // existe, evitando que a diferença de tempo de resposta revele quais emails têm conta.
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

    public async Task<AutenticacaoResult> ExecutarAsync(string email, string senha)
    {
        var usuario = await _repositorio.BuscarPorEmailAsync(email);
        var senhaConfere = usuario is not null
            ? usuario.Autenticar(senha, _hasher)
            : _hasher.Verify(senha, HashFalsoParaEvitarVazamentoDeTempo);

        if (usuario is null || !senhaConfere)
        {
            return AutenticacaoResult.Falha(MensagemCredenciaisInvalidas);
        }

        var token = _tokenGenerator.GerarToken(usuario);
        return AutenticacaoResult.ComSucesso(token);
    }
}
