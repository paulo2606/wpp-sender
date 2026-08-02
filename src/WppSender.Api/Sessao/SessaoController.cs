using Microsoft.AspNetCore.Mvc;
using WppSender.Application.Campanhas;

namespace WppSender.Api.Sessao;

[ApiController]
[Route("api/sessao")]
public class SessaoController : ControllerBase
{
    private readonly IniciarSessaoWhatsAppUseCase _iniciarUseCase;
    private readonly ObterStatusSessaoUseCase _obterStatusUseCase;

    public SessaoController(IniciarSessaoWhatsAppUseCase iniciarUseCase, ObterStatusSessaoUseCase obterStatusUseCase)
    {
        _iniciarUseCase = iniciarUseCase;
        _obterStatusUseCase = obterStatusUseCase;
    }

    [HttpPost("iniciar")]
    public async Task<IActionResult> Iniciar()
    {
        var qrCode = await _iniciarUseCase.ExecutarAsync();

        return Ok(new IniciarSessaoResponse(qrCode));
    }

    [HttpGet("status")]
    public async Task<IActionResult> ObterStatus()
    {
        var status = await _obterStatusUseCase.ExecutarAsync();

        return Ok(new StatusSessaoResponse(status));
    }
}
