using WppSender.Domain;

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

    public async Task<EditarCampanhaResult> ExecutarAsync(Guid id, string nome, string mensagem, DateTime? agendadoPara, int intervaloMinSegundos, int intervaloMaxSegundos)
    {
        var campanha = await _repositorio.BuscarPorIdAsync(id);
        if (campanha is null)
        {
            return EditarCampanhaResult.Falha(MensagemNaoEncontrada, EditarCampanhaErro.NaoEncontrada);
        }

        if (!campanha.PodeEditar())
        {
            return EditarCampanhaResult.Falha(MensagemNaoPermiteEdicao, EditarCampanhaErro.NaoPermiteEdicao);
        }

        try
        {
            campanha.AtualizarDados(nome, mensagem, agendadoPara, intervaloMinSegundos, intervaloMaxSegundos);
        }
        catch (ArgumentException ex)
        {
            return EditarCampanhaResult.Falha(ex.Message);
        }

        await _repositorio.AtualizarAsync(campanha);

        return EditarCampanhaResult.ComSucesso();
    }
}
