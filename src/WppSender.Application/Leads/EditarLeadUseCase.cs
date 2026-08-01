using WppSender.Domain;

namespace WppSender.Application.Leads;

public class EditarLeadUseCase
{
    private const string MensagemLeadNaoEncontrado = "Lead não encontrado";
    private const string MensagemTelefoneDuplicado = "Telefone já cadastrado";

    private readonly ILeadRepository _repositorio;

    public EditarLeadUseCase(ILeadRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<EditarLeadResult> ExecutarAsync(Guid id, string nome, string telefone, string? instagram, EnderecoInput? endereco, string? origem)
    {
        var lead = await _repositorio.BuscarPorIdAsync(id);
        if (lead is null || !lead.EstaAtivo)
        {
            return EditarLeadResult.Falha(MensagemLeadNaoEncontrado);
        }

        var telefoneNormalizadoNovo = Lead.NormalizarTelefone(telefone);
        if (telefoneNormalizadoNovo != lead.TelefoneNormalizado)
        {
            var outroComMesmoTelefone = await _repositorio.BuscarPorTelefoneNormalizadoAsync(telefoneNormalizadoNovo);
            if (outroComMesmoTelefone is not null && outroComMesmoTelefone.Id != lead.Id)
            {
                return EditarLeadResult.Falha(MensagemTelefoneDuplicado);
            }
        }

        var enderecoEntidade = endereco is null
            ? null
            : new Endereco(Guid.NewGuid(), endereco.Rua, endereco.Numero, endereco.Complemento, endereco.Bairro, endereco.Cidade, endereco.Estado, endereco.Cep);

        lead.AtualizarDados(nome, telefone, instagram, enderecoEntidade, origem);
        await _repositorio.AtualizarAsync(lead);

        return EditarLeadResult.ComSucesso();
    }
}
