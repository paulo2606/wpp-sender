using Microsoft.AspNetCore.Mvc;
using WppSender.Api.Auth;
using WppSender.Application.Leads;

namespace WppSender.Api.Leads;

[ApiController]
[Route("api/leads")]
public class LeadsController : ControllerBase
{
    private readonly CriarLeadUseCase _criarUseCase;
    private readonly EditarLeadUseCase _editarUseCase;
    private readonly ExcluirLeadUseCase _excluirUseCase;
    private readonly ListarLeadsUseCase _listarUseCase;
    private readonly ObterLeadUseCase _obterUseCase;
    private readonly ImportarLeadsCsvUseCase _importarUseCase;
    private readonly ExportarLeadsCsvUseCase _exportarUseCase;

    public LeadsController(
        CriarLeadUseCase criarUseCase,
        EditarLeadUseCase editarUseCase,
        ExcluirLeadUseCase excluirUseCase,
        ListarLeadsUseCase listarUseCase,
        ObterLeadUseCase obterUseCase,
        ImportarLeadsCsvUseCase importarUseCase,
        ExportarLeadsCsvUseCase exportarUseCase)
    {
        _criarUseCase = criarUseCase;
        _editarUseCase = editarUseCase;
        _excluirUseCase = excluirUseCase;
        _listarUseCase = listarUseCase;
        _obterUseCase = obterUseCase;
        _importarUseCase = importarUseCase;
        _exportarUseCase = exportarUseCase;
    }

    private static LeadResponse ParaLeadResponse(LeadResumo lead) => new(
        lead.Id,
        lead.Nome,
        lead.TelefoneNormalizado,
        lead.Instagram,
        lead.Origem,
        lead.Rua,
        lead.Numero,
        lead.Complemento,
        lead.Bairro,
        lead.Cidade,
        lead.Estado,
        lead.Cep);

    private static EnderecoInput? ParaEnderecoInput(EnderecoRequest? request) =>
        request is null
            ? null
            : new EnderecoInput(request.Rua, request.Numero, request.Complemento, request.Bairro, request.Cidade, request.Estado, request.Cep);

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarLeadRequest request)
    {
        var resultado = await _criarUseCase.ExecutarAsync(request.Nome, request.Telefone, request.Instagram, ParaEnderecoInput(request.Endereco), request.Origem);

        if (!resultado.Sucesso)
        {
            return Conflict(new ErroResponse(resultado.MensagemErro!));
        }

        return Ok(new { id = resultado.LeadId });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Editar(Guid id, [FromBody] EditarLeadRequest request)
    {
        var resultado = await _editarUseCase.ExecutarAsync(id, request.Nome, request.Telefone, request.Instagram, ParaEnderecoInput(request.Endereco), request.Origem);

        if (!resultado.Sucesso)
        {
            return resultado.Erro switch
            {
                EditarLeadErro.NaoEncontrado => NotFound(new ErroResponse(resultado.MensagemErro!)),
                EditarLeadErro.TelefoneDuplicado => Conflict(new ErroResponse(resultado.MensagemErro!)),
                _ => BadRequest(new ErroResponse(resultado.MensagemErro!)),
            };
        }

        return Ok();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var resultado = await _obterUseCase.ExecutarAsync(id);

        if (!resultado.Sucesso)
        {
            return NotFound(new ErroResponse(resultado.MensagemErro!));
        }

        return Ok(ParaLeadResponse(resultado.Lead!));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var resultado = await _excluirUseCase.ExecutarAsync(id);

        if (!resultado.Sucesso)
        {
            return NotFound(new ErroResponse(resultado.MensagemErro!));
        }

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? busca, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
    {
        var resultado = await _listarUseCase.ExecutarAsync(busca, pagina, tamanhoPagina);
        var itens = resultado.Itens
            .Select(ParaLeadResponse)
            .ToList();

        return Ok(new ListaLeadsResponse(itens, resultado.Total, pagina, tamanhoPagina));
    }

    [HttpPost("importar")]
    public async Task<IActionResult> Importar(IFormFile? arquivo)
    {
        if (arquivo is null || arquivo.Length == 0)
        {
            return BadRequest(new ErroResponse("Arquivo CSV obrigatório"));
        }

        await using var stream = arquivo.OpenReadStream();
        var resultado = await _importarUseCase.ExecutarAsync(stream);
        var puladas = resultado.Puladas
            .Select(p => new LeadPuladoResponse(p.NumeroLinha, p.Telefone, p.Motivo))
            .ToList();

        return Ok(new ImportarLeadsResponse(resultado.Importados, puladas));
    }

    [HttpGet("exportar")]
    public async Task<IActionResult> Exportar()
    {
        var memoryStream = new MemoryStream();
        await _exportarUseCase.ExecutarAsync(memoryStream);
        memoryStream.Position = 0;

        return File(memoryStream, "text/csv", "leads.csv");
    }
}
