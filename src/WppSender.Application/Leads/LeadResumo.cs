using WppSender.Domain;

namespace WppSender.Application.Leads;

public record LeadResumo(
    Guid Id,
    string Nome,
    string TelefoneNormalizado,
    string? Instagram,
    string? Origem,
    string? Rua,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    string? Cep)
{
    public static LeadResumo DeLead(Lead lead) => new(
        lead.Id,
        lead.Nome,
        lead.TelefoneNormalizado,
        lead.Instagram,
        lead.Origem,
        lead.Endereco?.Rua,
        lead.Endereco?.Numero,
        lead.Endereco?.Complemento,
        lead.Endereco?.Bairro,
        lead.Endereco?.Cidade,
        lead.Endereco?.Estado,
        lead.Endereco?.Cep);
}
