using WppSender.Domain;
using WppSender.Application.Shared;

namespace WppSender.Application.Leads;

public class ExcluirLeadUseCase
{
    private const string MensagemLeadNaoEncontrado = "Lead não encontrado";

    private readonly ILeadRepository _repositorio;

    public ExcluirLeadUseCase(ILeadRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<Resultado> ExecutarAsync(Guid id)
    {
        var lead = await _repositorio.BuscarPorIdAsync(id);
        if (lead is null || !lead.EstaAtivo)
        {
            return Resultado.Falha(MensagemLeadNaoEncontrado);
        }

        lead.Excluir();
        await _repositorio.AtualizarAsync(lead);

        return Resultado.ComSucesso();
    }
}
