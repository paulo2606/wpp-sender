namespace WppSender.Api.Leads;

public record LeadResponse(
    Guid Id,
    string Nome,
    string Telefone,
    string? Instagram,
    string? Origem,
    string? Rua,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    string? Cep,
    Guid? GrupoId);
