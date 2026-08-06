using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using WppSender.Application.Auth;
using WppSender.Infrastructure.Security;

namespace WppSender.Api.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AutenticarUsuarioUseCase _autenticarUseCase;
    private readonly RegistrarUsuarioUseCase _registrarUseCase;
    private readonly JwtOptions _jwtOptions;

    public AuthController(AutenticarUsuarioUseCase autenticarUseCase, RegistrarUsuarioUseCase registrarUseCase, IOptions<JwtOptions> jwtOptions)
    {
        _autenticarUseCase = autenticarUseCase;
        _registrarUseCase = registrarUseCase;
        _jwtOptions = jwtOptions.Value;
    }

    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var resultado = await _autenticarUseCase.ExecutarAsync(request.Email, request.Senha);

        if (!resultado.Sucesso)
        {
            return Unauthorized(new ErroResponse(resultado.MensagemErro!));
        }

        Response.Cookies.Append("wpp_auth", resultado.Valor!, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
        });

        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("wpp_auth");
        return Ok();
    }

    [AllowAnonymous]
    [EnableRateLimiting("registrar")]
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
