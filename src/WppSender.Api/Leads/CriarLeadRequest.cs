namespace WppSender.Api.Leads;

public record CriarLeadRequest(string Nome, string Telefone, string? Instagram, EnderecoRequest? Endereco, string? Origem, Guid? GrupoId);
