using WppSender.Domain;
using WppSender.Application.Shared;

namespace WppSender.Application.Leads;

public class CriarLeadUseCase
{
    private const string MensagemTelefoneDuplicado = "Telefone já cadastrado";

    private readonly ILeadRepository _repositorio;

    public CriarLeadUseCase(ILeadRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<ResultadoComValor<Guid>> ExecutarAsync(string nome, string telefone, string? instagram, EnderecoInput? endereco, string? origem, Guid? grupoId = null)
    {
        Lead lead;
        try
        {
            var enderecoEntidade = endereco is null
                ? null
                : new Endereco(Guid.NewGuid(), endereco.Rua, endereco.Numero, endereco.Complemento, endereco.Bairro, endereco.Cidade, endereco.Estado, endereco.Cep);

            lead = new Lead(Guid.NewGuid(), nome, telefone, instagram, enderecoEntidade, origem, grupoId);
        }
        catch (ArgumentException ex)
        {
            return ResultadoComValor<Guid>.Falha(ex.Message);
        }

        var existente = await _repositorio.BuscarPorTelefoneNormalizadoAsync(lead.TelefoneNormalizado);
        if (existente is not null)
        {
            return ResultadoComValor<Guid>.Falha(MensagemTelefoneDuplicado);
        }

        await _repositorio.AdicionarAsync(lead);
        return ResultadoComValor<Guid>.ComSucesso(lead.Id);
    }
}
