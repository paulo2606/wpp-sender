using WppSender.Domain;

namespace WppSender.Application.Leads;

public class ObterLeadUseCase
{
    private const string MensagemLeadNaoEncontrado = "Lead não encontrado";

    private readonly ILeadRepository _repositorio;

    public ObterLeadUseCase(ILeadRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<ObterLeadResult> ExecutarAsync(Guid id)
    {
        var lead = await _repositorio.BuscarPorIdAsync(id);
        if (lead is null || !lead.EstaAtivo)
        {
            return ObterLeadResult.Falha(MensagemLeadNaoEncontrado);
        }

        return ObterLeadResult.ComSucesso(LeadResumo.DeLead(lead));
    }
}
