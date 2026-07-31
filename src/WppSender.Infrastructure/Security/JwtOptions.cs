namespace WppSender.Infrastructure.Security;

public class JwtOptions
{
    public string SigningKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; }
}
