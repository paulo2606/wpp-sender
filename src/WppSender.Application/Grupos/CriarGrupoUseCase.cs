using WppSender.Domain;

namespace WppSender.Application.Grupos;

public class CriarGrupoUseCase
{
    private const string MensagemListaLeadsVazia = "É necessário informar ao menos um lead";

    private readonly IGrupoRepository _grupoRepositorio;
    private readonly ILeadRepository _leadRepositorio;
    private readonly IUnitOfWork _unitOfWork;

    public CriarGrupoUseCase(IGrupoRepository grupoRepositorio, ILeadRepository leadRepositorio, IUnitOfWork unitOfWork)
    {
        _grupoRepositorio = grupoRepositorio;
        _leadRepositorio = leadRepositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<CriarGrupoResult> ExecutarAsync(string nome, string? descricao, IReadOnlyList<Guid> leadIds)
    {
        if (leadIds is null || leadIds.Count == 0)
        {
            return CriarGrupoResult.Falha(MensagemListaLeadsVazia);
        }

        var leadsValidos = new List<Lead>();
        var idsInvalidos = new List<Guid>();
        foreach (var leadId in leadIds)
        {
            var lead = await _leadRepositorio.BuscarPorIdAsync(leadId);
            if (lead is null || !lead.EstaAtivo)
            {
                idsInvalidos.Add(leadId);
            }
            else
            {
                leadsValidos.Add(lead);
            }
        }

        if (idsInvalidos.Count > 0)
        {
            var listaIds = string.Join(", ", idsInvalidos);
            return CriarGrupoResult.Falha($"Leads inválidos ou não encontrados: {listaIds}");
        }

        Grupo grupo;
        try
        {
            grupo = new Grupo(Guid.NewGuid(), nome, descricao);
        }
        catch (ArgumentException ex)
        {
            return CriarGrupoResult.Falha(ex.Message);
        }

        await _unitOfWork.ExecutarTransacaoAsync(async () =>
        {
            await _grupoRepositorio.AdicionarAsync(grupo);
            foreach (var lead in leadsValidos)
            {
                lead.AtribuirGrupo(grupo.Id);
                await _leadRepositorio.AtualizarAsync(lead);
            }
        });

        return CriarGrupoResult.ComSucesso(grupo.Id);
    }
}
