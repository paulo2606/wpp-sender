using Microsoft.AspNetCore.Mvc;
using WppSender.Application.Dashboard;

namespace WppSender.Api.Dashboard;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly ContarCampanhasPorStatusUseCase _contarCampanhasUseCase;
    private readonly ContarLeadsRecentesUseCase _contarLeadsRecentesUseCase;
    private readonly CalcularTaxaEnvioUseCase _calcularTaxaEnvioUseCase;
    private readonly ContarLeadsPorGrupoUseCase _contarLeadsPorGrupoUseCase;
    private readonly ObterProximaCampanhaAgendadaUseCase _obterProximaCampanhaUseCase;

    public DashboardController(
        ContarCampanhasPorStatusUseCase contarCampanhasUseCase,
        ContarLeadsRecentesUseCase contarLeadsRecentesUseCase,
        CalcularTaxaEnvioUseCase calcularTaxaEnvioUseCase,
        ContarLeadsPorGrupoUseCase contarLeadsPorGrupoUseCase,
        ObterProximaCampanhaAgendadaUseCase obterProximaCampanhaUseCase)
    {
        _contarCampanhasUseCase = contarCampanhasUseCase;
        _contarLeadsRecentesUseCase = contarLeadsRecentesUseCase;
        _calcularTaxaEnvioUseCase = calcularTaxaEnvioUseCase;
        _contarLeadsPorGrupoUseCase = contarLeadsPorGrupoUseCase;
        _obterProximaCampanhaUseCase = obterProximaCampanhaUseCase;
    }

    [HttpGet("campanhas-por-status")]
    public async Task<IActionResult> CampanhasPorStatus()
    {
        var resultado = await _contarCampanhasUseCase.ExecutarAsync();

        return Ok(resultado);
    }

    [HttpGet("leads-recentes")]
    public async Task<IActionResult> LeadsRecentes([FromQuery] int dias = 7)
    {
        var resultado = await _contarLeadsRecentesUseCase.ExecutarAsync(dias);

        return Ok(new { dias, quantidade = resultado });
    }

    [HttpGet("taxa-envio")]
    public async Task<IActionResult> TaxaEnvio()
    {
        var resultado = await _calcularTaxaEnvioUseCase.ExecutarAsync();

        return Ok(resultado);
    }

    [HttpGet("leads-por-grupo")]
    public async Task<IActionResult> LeadsPorGrupo()
    {
        var resultado = await _contarLeadsPorGrupoUseCase.ExecutarAsync();

        return Ok(resultado);
    }

    [HttpGet("proxima-campanha-agendada")]
    public async Task<IActionResult> ProximaCampanhaAgendada()
    {
        var resultado = await _obterProximaCampanhaUseCase.ExecutarAsync();

        return resultado is null ? NotFound() : Ok(resultado);
    }
}
