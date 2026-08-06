using WppSender.Domain;
using WppSender.Application.Shared;

namespace WppSender.Application.Leads;

public class ObterLeadUseCase
{
    private const string MensagemLeadNaoEncontrado = "Lead não encontrado";

    private readonly ILeadRepository _repositorio;

    public ObterLeadUseCase(ILeadRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<ResultadoComValor<LeadResumo>> ExecutarAsync(Guid id)
    {
        var lead = await _repositorio.BuscarPorIdAsync(id);
        if (lead is null || !lead.EstaAtivo)
        {
            return ResultadoComValor<LeadResumo>.Falha(MensagemLeadNaoEncontrado);
        }

        return ResultadoComValor<LeadResumo>.ComSucesso(LeadResumo.DeLead(lead));
    }
}
