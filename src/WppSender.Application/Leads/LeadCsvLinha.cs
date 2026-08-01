namespace WppSender.Application.Leads;

public record LeadCsvLinha(
    int NumeroLinha,
    string Nome,
    string Telefone,
    string? Instagram,
    string? Rua,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    string? Cep,
    string? Origem);
