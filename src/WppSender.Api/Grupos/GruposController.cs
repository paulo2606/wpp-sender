using Microsoft.AspNetCore.Mvc;
using WppSender.Api.Auth;
using WppSender.Application.Grupos;

namespace WppSender.Api.Grupos;

[ApiController]
[Route("api/grupos")]
public class GruposController : ControllerBase
{
    private readonly CriarGrupoUseCase _criarUseCase;
    private readonly EditarGrupoUseCase _editarUseCase;
    private readonly ExcluirGrupoUseCase _excluirUseCase;
    private readonly ListarGruposUseCase _listarUseCase;

    public GruposController(
        CriarGrupoUseCase criarUseCase,
        EditarGrupoUseCase editarUseCase,
        ExcluirGrupoUseCase excluirUseCase,
        ListarGruposUseCase listarUseCase)
    {
        _criarUseCase = criarUseCase;
        _editarUseCase = editarUseCase;
        _excluirUseCase = excluirUseCase;
        _listarUseCase = listarUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarGrupoRequest request)
    {
        var resultado = await _criarUseCase.ExecutarAsync(request.Nome, request.Descricao, request.LeadIds);

        if (!resultado.Sucesso)
        {
            return BadRequest(new ErroResponse(resultado.MensagemErro!));
        }

        return Ok(new { id = resultado.GrupoId });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Editar(Guid id, [FromBody] EditarGrupoRequest request)
    {
        var resultado = await _editarUseCase.ExecutarAsync(id, request.Nome, request.Descricao);

        if (!resultado.Sucesso)
        {
            return resultado.Erro switch
            {
                EditarGrupoErro.NaoEncontrado => NotFound(new ErroResponse(resultado.MensagemErro!)),
                _ => BadRequest(new ErroResponse(resultado.MensagemErro!)),
            };
        }

        return Ok();
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
    public async Task<IActionResult> Listar([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
    {
        var resultado = await _listarUseCase.ExecutarAsync(pagina, tamanhoPagina);
        var itens = resultado.Itens
            .Select(g => new GrupoResponse(g.Id, g.Nome, g.Descricao, g.QuantidadeLeads))
            .ToList();

        return Ok(new ListaGruposResponse(itens, resultado.Total, resultado.Pagina, resultado.TamanhoPagina));
    }
}
