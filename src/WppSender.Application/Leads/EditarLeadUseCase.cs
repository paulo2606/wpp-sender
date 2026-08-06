using WppSender.Domain;
using WppSender.Application.Shared;

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

    public async Task<Resultado<EditarLeadErro>> ExecutarAsync(Guid id, string nome, string telefone, string? instagram, EnderecoInput? endereco, string? origem, Guid? grupoId = null)
    {
        var lead = await _repositorio.BuscarPorIdAsync(id);
        if (lead is null || !lead.EstaAtivo)
        {
            return Resultado<EditarLeadErro>.Falha(MensagemLeadNaoEncontrado, EditarLeadErro.NaoEncontrado);
        }

        try
        {
            Lead.ValidarCamposObrigatorios(nome, telefone);
        }
        catch (ArgumentException ex)
        {
            return Resultado<EditarLeadErro>.Falha(ex.Message);
        }

        var telefoneNormalizadoNovo = Lead.NormalizarTelefone(telefone);
        if (telefoneNormalizadoNovo != lead.TelefoneNormalizado)
        {
            var outroComMesmoTelefone = await _repositorio.BuscarPorTelefoneNormalizadoAsync(telefoneNormalizadoNovo);
            if (outroComMesmoTelefone is not null && outroComMesmoTelefone.Id != lead.Id)
            {
                return Resultado<EditarLeadErro>.Falha(MensagemTelefoneDuplicado, EditarLeadErro.TelefoneDuplicado);
            }
        }

        Endereco? enderecoAtualizado;
        if (endereco is null)
        {
            enderecoAtualizado = null;
        }
        else if (lead.Endereco is not null)
        {
            lead.Endereco.AtualizarDados(endereco.Rua, endereco.Numero, endereco.Complemento, endereco.Bairro, endereco.Cidade, endereco.Estado, endereco.Cep);
            enderecoAtualizado = lead.Endereco;
        }
        else
        {
            enderecoAtualizado = new Endereco(Guid.NewGuid(), endereco.Rua, endereco.Numero, endereco.Complemento, endereco.Bairro, endereco.Cidade, endereco.Estado, endereco.Cep);
        }

        lead.AtualizarDados(nome, telefone, instagram, enderecoAtualizado, origem, grupoId);
        await _repositorio.AtualizarAsync(lead);

        return Resultado<EditarLeadErro>.ComSucesso();
    }
}
