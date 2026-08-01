using WppSender.Domain;

namespace WppSender.Application.Leads;

public class CriarLeadUseCase
{
    private const string MensagemTelefoneDuplicado = "Telefone já cadastrado";

    private readonly ILeadRepository _repositorio;

    public CriarLeadUseCase(ILeadRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<CriarLeadResult> ExecutarAsync(string nome, string telefone, string? instagram, EnderecoInput? endereco, string? origem)
    {
        var enderecoEntidade = endereco is null
            ? null
            : new Endereco(Guid.NewGuid(), endereco.Rua, endereco.Numero, endereco.Complemento, endereco.Bairro, endereco.Cidade, endereco.Estado, endereco.Cep);

        var lead = new Lead(Guid.NewGuid(), nome, telefone, instagram, enderecoEntidade, origem);

        var existente = await _repositorio.BuscarPorTelefoneNormalizadoAsync(lead.TelefoneNormalizado);
        if (existente is not null)
        {
            return CriarLeadResult.Falha(MensagemTelefoneDuplicado);
        }

        await _repositorio.AdicionarAsync(lead);
        return CriarLeadResult.ComSucesso(lead.Id);
    }
}
