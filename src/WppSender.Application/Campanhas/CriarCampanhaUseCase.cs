using WppSender.Domain;
using WppSender.Application.Shared;

namespace WppSender.Application.Campanhas;

public class CriarCampanhaUseCase
{
    private const string MensagemGrupoSemLeadsAtivos = "O grupo selecionado não possui leads ativos";

    private readonly ICampanhaRepository _campanhaRepositorio;
    private readonly IEnvioRepository _envioRepositorio;
    private readonly ILeadRepository _leadRepositorio;
    private readonly IUnitOfWork _unitOfWork;

    public CriarCampanhaUseCase(ICampanhaRepository campanhaRepositorio, IEnvioRepository envioRepositorio, ILeadRepository leadRepositorio, IUnitOfWork unitOfWork)
    {
        _campanhaRepositorio = campanhaRepositorio;
        _envioRepositorio = envioRepositorio;
        _leadRepositorio = leadRepositorio;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResultadoComValor<Guid>> ExecutarAsync(string nome, string mensagem, Guid grupoId, DateTime? agendadoPara, int intervaloMinSegundos = 30, int intervaloMaxSegundos = 90)
    {
        var leadsDoGrupo = await _leadRepositorio.ListarAtivosPorGrupoAsync(grupoId);
        if (leadsDoGrupo.Count == 0)
        {
            return ResultadoComValor<Guid>.Falha(MensagemGrupoSemLeadsAtivos);
        }

        Campanha campanha;
        try
        {
            campanha = new Campanha(Guid.NewGuid(), nome, mensagem, grupoId, agendadoPara, intervaloMinSegundos, intervaloMaxSegundos);
        }
        catch (ArgumentException ex)
        {
            return ResultadoComValor<Guid>.Falha(ex.Message);
        }

        await _unitOfWork.ExecutarTransacaoAsync(async () =>
        {
            await _campanhaRepositorio.AdicionarAsync(campanha);
            var envios = leadsDoGrupo.Select(lead => new Envio(Guid.NewGuid(), campanha.Id, lead.Id)).ToList();
            await _envioRepositorio.AdicionarVariosAsync(envios);
        });

        return ResultadoComValor<Guid>.ComSucesso(campanha.Id);
    }
}
