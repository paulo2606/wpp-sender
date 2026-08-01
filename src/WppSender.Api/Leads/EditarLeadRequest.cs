namespace WppSender.Api.Leads;

public record EditarLeadRequest(string Nome, string Telefone, string? Instagram, EnderecoRequest? Endereco, string? Origem);
