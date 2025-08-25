namespace Agendor.Core.Entities
{
    public class JwtOptions
    {
        public string Issuer { get; init; } = "Agendor.Api";
        public string Audience { get; init; } = "Agendor.Web";
        public string SecretKey { get; init; } = "troque-esta-chave-super-secreta-32+chars";
        public int ExpiresMinutes { get; init; } = 60;
    }
}
