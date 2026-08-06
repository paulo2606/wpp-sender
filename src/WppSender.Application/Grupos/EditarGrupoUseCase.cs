using WppSender.Domain;
using WppSender.Application.Shared;

namespace WppSender.Application.Grupos;

public class EditarGrupoUseCase
{
    private const string MensagemGrupoNaoEncontrado = "Grupo não encontrado";

    private readonly IGrupoRepository _repositorio;
    private readonly ILeadRepository _leadRepositorio;
    private readonly IUnitOfWork _unitOfWork;

    public EditarGrupoUseCase(IGrupoRepository repositorio, ILeadRepository leadRepositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _leadRepositorio = leadRepositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<Resultado<EditarGrupoErro>> ExecutarAsync(Guid id, string nome, string? descricao, IReadOnlyList<Guid>? leadIdsParaAdicionar = null)
    {
        var grupo = await _repositorio.BuscarPorIdAsync(id);
        if (grupo is null)
        {
            return Resultado<EditarGrupoErro>.Falha(MensagemGrupoNaoEncontrado, EditarGrupoErro.NaoEncontrado);
        }

        var leadsParaAdicionar = new List<Lead>();
        var idsInvalidos = new List<Guid>();
        foreach (var leadId in leadIdsParaAdicionar ?? Array.Empty<Guid>())
        {
            var lead = await _leadRepositorio.BuscarPorIdAsync(leadId);
            if (lead is null || !lead.EstaAtivo)
            {
                idsInvalidos.Add(leadId);
            }
            else
            {
                leadsParaAdicionar.Add(lead);
            }
        }

        if (idsInvalidos.Count > 0)
        {
            var listaIds = string.Join(", ", idsInvalidos);
            return Resultado<EditarGrupoErro>.Falha($"Leads inválidos ou não encontrados: {listaIds}");
        }

        try
        {
            grupo.AtualizarDados(nome, descricao);
        }
        catch (ArgumentException ex)
        {
            return Resultado<EditarGrupoErro>.Falha(ex.Message);
        }

        await _unitOfWork.ExecutarTransacaoAsync(async () =>
        {
            await _repositorio.AtualizarAsync(grupo);
            foreach (var lead in leadsParaAdicionar)
            {
                lead.AtribuirGrupo(grupo.Id);
                await _leadRepositorio.AtualizarAsync(lead);
            }
        });

        return Resultado<EditarGrupoErro>.ComSucesso();
    }
}
