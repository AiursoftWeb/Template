namespace Aiursoft.Template.Configuration;

public class OidcSettings
{
    public required string Authority { get; init; } = "https://your-oidc-provider.com";
    public required string ClientId { get; init; } = "your-client-id";
    public required string ClientSecret { get; init; } = "your-client-secret";
    public required string RolePropertyNames { get; init; } = "groups";
}
