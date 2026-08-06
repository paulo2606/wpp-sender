namespace WppSender.Infrastructure.Security;

public class JwtOptions
{
    public string SigningKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; }
    public string Issuer { get; set; } = "wpp-sender-api";
    public string Audience { get; set; } = "wpp-sender-front-end";
}
