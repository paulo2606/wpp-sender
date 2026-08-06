using Microsoft.AspNetCore.Mvc;
using WppSender.Api.Auth;
using WppSender.Application.Campanhas;
using WppSender.Domain;

namespace WppSender.Api.Campanhas;

[ApiController]
[Route("api/campanhas")]
public class CampanhasController : ControllerBase
{
    private readonly CriarCampanhaUseCase _criarUseCase;
    private readonly EditarCampanhaUseCase _editarUseCase;
    private readonly ExcluirCampanhaUseCase _excluirUseCase;
    private readonly ListarCampanhasUseCase _listarUseCase;
    private readonly ObterCampanhaUseCase _obterUseCase;
    private readonly IniciarCampanhaUseCase _iniciarUseCase;
    private readonly PausarCampanhaUseCase _pausarUseCase;
    private readonly RetomarCampanhaUseCase _retomarUseCase;
    private readonly CancelarCampanhaUseCase _cancelarUseCase;
    private readonly ListarEnviosFalhosUseCase _listarEnviosFalhosUseCase;
    private readonly ReenviarFalhasUseCase _reenviarFalhasUseCase;

    public CampanhasController(
        CriarCampanhaUseCase criarUseCase,
        EditarCampanhaUseCase editarUseCase,
        ExcluirCampanhaUseCase excluirUseCase,
        ListarCampanhasUseCase listarUseCase,
        ObterCampanhaUseCase obterUseCase,
        IniciarCampanhaUseCase iniciarUseCase,
        PausarCampanhaUseCase pausarUseCase,
        RetomarCampanhaUseCase retomarUseCase,
        CancelarCampanhaUseCase cancelarUseCase,
        ListarEnviosFalhosUseCase listarEnviosFalhosUseCase,
        ReenviarFalhasUseCase reenviarFalhasUseCase)
    {
        _criarUseCase = criarUseCase;
        _editarUseCase = editarUseCase;
        _excluirUseCase = excluirUseCase;
        _listarUseCase = listarUseCase;
        _obterUseCase = obterUseCase;
        _iniciarUseCase = iniciarUseCase;
        _pausarUseCase = pausarUseCase;
        _retomarUseCase = retomarUseCase;
        _cancelarUseCase = cancelarUseCase;
        _listarEnviosFalhosUseCase = listarEnviosFalhosUseCase;
        _reenviarFalhasUseCase = reenviarFalhasUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarCampanhaRequest request)
    {
        var resultado = await _criarUseCase.ExecutarAsync(request.Nome, request.Mensagem, request.GrupoId, request.AgendadoPara, request.IntervaloMinSegundos, request.IntervaloMaxSegundos);

        if (!resultado.Sucesso)
        {
            return BadRequest(new ErroResponse(resultado.MensagemErro!));
        }

        return Ok(new { id = resultado.Valor });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Editar(Guid id, [FromBody] EditarCampanhaRequest request)
    {
        var resultado = await _editarUseCase.ExecutarAsync(id, request.Nome, request.Mensagem, request.AgendadoPara, request.IntervaloMinSegundos, request.IntervaloMaxSegundos);

        if (!resultado.Sucesso)
        {
            return resultado.Erro switch
            {
                EditarCampanhaErro.NaoEncontrada => NotFound(new ErroResponse(resultado.MensagemErro!)),
                EditarCampanhaErro.NaoPermiteEdicao => Conflict(new ErroResponse(resultado.MensagemErro!)),
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
            return resultado.Erro switch
            {
                ExcluirCampanhaErro.NaoEncontrada => NotFound(new ErroResponse(resultado.MensagemErro!)),
                ExcluirCampanhaErro.NaoPermiteExclusao => Conflict(new ErroResponse(resultado.MensagemErro!)),
                _ => BadRequest(new ErroResponse(resultado.MensagemErro!)),
            };
        }

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] StatusCampanha? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
    {
        var resultado = await _listarUseCase.ExecutarAsync(status, pagina, tamanhoPagina);
        var itens = resultado.Itens.Select(ParaCampanhaResponse).ToList();

        return Ok(new ListaCampanhasResponse(itens, resultado.Total, resultado.Pagina, resultado.TamanhoPagina));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var resultado = await _obterUseCase.ExecutarAsync(id);
        if (resultado is null)
        {
            return NotFound(new ErroResponse("Campanha não encontrada"));
        }

        return Ok(new
        {
            campanha = ParaCampanhaResponse(resultado.Campanha),
            progresso = new ProgressoResponse(
                resultado.Progresso.Pendente,
                resultado.Progresso.Enviado,
                resultado.Progresso.Entregue,
                resultado.Progresso.Lido,
                resultado.Progresso.Falhou,
                resultado.Progresso.FalhouEntrega),
        });
    }

    [HttpPost("{id:guid}/iniciar")]
    public async Task<IActionResult> Iniciar(Guid id)
    {
        var resultado = await _iniciarUseCase.ExecutarAsync(id);

        if (!resultado.Sucesso)
        {
            return resultado.Erro switch
            {
                IniciarCampanhaErro.NaoEncontrada => NotFound(new ErroResponse(resultado.MensagemErro!)),
                IniciarCampanhaErro.StatusInvalido => Conflict(new ErroResponse(resultado.MensagemErro!)),
                _ => BadRequest(new ErroResponse(resultado.MensagemErro!)),
            };
        }

        return Ok();
    }

    [HttpPost("{id:guid}/pausar")]
    public async Task<IActionResult> Pausar(Guid id)
    {
        var resultado = await _pausarUseCase.ExecutarAsync(id);

        if (!resultado.Sucesso)
        {
            return resultado.Erro switch
            {
                PausarCampanhaErro.NaoEncontrada => NotFound(new ErroResponse(resultado.MensagemErro!)),
                _ => Conflict(new ErroResponse(resultado.MensagemErro!)),
            };
        }

        return Ok();
    }

    [HttpPost("{id:guid}/retomar")]
    public async Task<IActionResult> Retomar(Guid id)
    {
        var resultado = await _retomarUseCase.ExecutarAsync(id);

        if (!resultado.Sucesso)
        {
            return resultado.Erro switch
            {
                RetomarCampanhaErro.NaoEncontrada => NotFound(new ErroResponse(resultado.MensagemErro!)),
                RetomarCampanhaErro.StatusInvalido => Conflict(new ErroResponse(resultado.MensagemErro!)),
                _ => BadRequest(new ErroResponse(resultado.MensagemErro!)),
            };
        }

        return Ok();
    }

    [HttpPost("{id:guid}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        var resultado = await _cancelarUseCase.ExecutarAsync(id);

        if (!resultado.Sucesso)
        {
            return resultado.Erro switch
            {
                CancelarCampanhaErro.NaoEncontrada => NotFound(new ErroResponse(resultado.MensagemErro!)),
                _ => Conflict(new ErroResponse(resultado.MensagemErro!)),
            };
        }

        return Ok();
    }

    [HttpGet("{id:guid}/envios/falhos")]
    public async Task<IActionResult> ListarEnviosFalhos(Guid id)
    {
        var falhos = await _listarEnviosFalhosUseCase.ExecutarAsync(id);
        var resposta = falhos.Select(f => new EnvioFalhoResponse(f.EnvioId, f.LeadId, f.NomeLead, f.Erro)).ToList();

        return Ok(resposta);
    }

    [HttpPost("{id:guid}/reenviar-falhas")]
    public async Task<IActionResult> ReenviarFalhas(Guid id)
    {
        await _reenviarFalhasUseCase.ExecutarAsync(id);

        return Ok();
    }

    private static CampanhaResponse ParaCampanhaResponse(CampanhaResumo campanha) => new(
        campanha.Id,
        campanha.Nome,
        campanha.Mensagem,
        campanha.GrupoId,
        campanha.Status,
        campanha.AgendadoPara,
        campanha.IntervaloMinSegundos,
        campanha.IntervaloMaxSegundos);
}
