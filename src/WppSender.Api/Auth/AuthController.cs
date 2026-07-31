using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WppSender.Application.Auth;

namespace WppSender.Api.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AutenticarUsuarioUseCase _autenticarUseCase;
    private readonly RegistrarUsuarioUseCase _registrarUseCase;

    public AuthController(AutenticarUsuarioUseCase autenticarUseCase, RegistrarUsuarioUseCase registrarUseCase)
    {
        _autenticarUseCase = autenticarUseCase;
        _registrarUseCase = registrarUseCase;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var resultado = await _autenticarUseCase.ExecutarAsync(request.Email, request.Senha);

        if (!resultado.Sucesso)
        {
            return Unauthorized(new ErroResponse(resultado.MensagemErro!));
        }

        return Ok(new LoginResponse(resultado.Token!));
    }

    // Endpoint de bootstrap: só funciona enquanto não existir nenhum usuário na base.
    // Não é exposto no frontend — existe apenas para o único usuário se cadastrar uma vez.
    [AllowAnonymous]
    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarRequest request)
    {
        var resultado = await _registrarUseCase.ExecutarAsync(request.Email, request.Senha);

        if (!resultado.Sucesso)
        {
            return Conflict(new ErroResponse(resultado.MensagemErro!));
        }

        return Ok();
    }
}

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet("privado")]
    public IActionResult Privado() => Ok(new { status = "autenticado" });
}
