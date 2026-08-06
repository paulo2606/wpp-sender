using WppSender.Domain;
using WppSender.Application.Shared;

namespace WppSender.Application.Campanhas;

public class EditarCampanhaUseCase
{
    private const string MensagemNaoEncontrada = "Campanha não encontrada";
    private const string MensagemNaoPermiteEdicao = "Campanha só pode ser editada em rascunho ou agendada";

    private readonly ICampanhaRepository _repositorio;

    public EditarCampanhaUseCase(ICampanhaRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<Resultado<EditarCampanhaErro>> ExecutarAsync(Guid id, string nome, string mensagem, DateTime? agendadoPara, int intervaloMinSegundos, int intervaloMaxSegundos)
    {
        var campanha = await _repositorio.BuscarPorIdAsync(id);
        if (campanha is null)
        {
            return Resultado<EditarCampanhaErro>.Falha(MensagemNaoEncontrada, EditarCampanhaErro.NaoEncontrada);
        }

        if (!campanha.PodeEditar())
        {
            return Resultado<EditarCampanhaErro>.Falha(MensagemNaoPermiteEdicao, EditarCampanhaErro.NaoPermiteEdicao);
        }

        try
        {
            campanha.AtualizarDados(nome, mensagem, agendadoPara, intervaloMinSegundos, intervaloMaxSegundos);
        }
        catch (ArgumentException ex)
        {
            return Resultado<EditarCampanhaErro>.Falha(ex.Message);
        }

        await _repositorio.AtualizarAsync(campanha);

        return Resultado<EditarCampanhaErro>.ComSucesso();
    }
}
