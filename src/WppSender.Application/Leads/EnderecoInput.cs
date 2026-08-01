namespace WppSender.Application.Leads;

public record EnderecoInput(string Rua, string Numero, string? Complemento, string Bairro, string Cidade, string Estado, string Cep);
