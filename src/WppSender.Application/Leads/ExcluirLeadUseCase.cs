using WppSender.Domain;

namespace WppSender.Application.Leads;

public class ExcluirLeadUseCase
{
    private const string MensagemLeadNaoEncontrado = "Lead não encontrado";

    private readonly ILeadRepository _repositorio;

    public ExcluirLeadUseCase(ILeadRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<ExcluirLeadResult> ExecutarAsync(Guid id)
    {
        var lead = await _repositorio.BuscarPorIdAsync(id);
        if (lead is null || !lead.EstaAtivo)
        {
            return ExcluirLeadResult.Falha(MensagemLeadNaoEncontrado);
        }

        lead.Excluir();
        await _repositorio.AtualizarAsync(lead);

        return ExcluirLeadResult.ComSucesso();
    }
}
