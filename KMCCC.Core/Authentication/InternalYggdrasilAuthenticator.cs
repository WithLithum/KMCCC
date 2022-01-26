using KMCCC.Modules.Yggdrasil;
using System.Security.Authentication;

namespace KMCCC.Authentication;

/// <summary>
/// Internal implementation of Yggdrasil authenticator.
/// </summary>
internal class InternalYggdrasilAuthenticator : IAuthenticator
{
    public InternalYggdrasilAuthenticator(String accessToken, Guid clientToken, Guid uuid, string displayName, string? authServer = null)
    {
        AccessToken = accessToken;
        ClientToken = clientToken;
        DisplayName = displayName;
        UUID = uuid;
        AuthServer = authServer;
    }

    public string Type => "KMCCC.Yggdrasil";

    public String AccessToken { get; }

    public Guid ClientToken { get; }

    public string DisplayName { get; }

    public Guid UUID { get; }

    public string? AuthServer { get; set; }

    public AuthenticationInfo Do()
    {
        var client = new YggdrasilClient(AuthServer, ClientToken);
        var exc = client.AuthToken(AccessToken, UUID, DisplayName);

        if (exc == null)
        {
            return new AuthenticationInfo
            {
                AccessToken = client.AccessToken,
                UserType = client.AccountType,
                DisplayName = client.DisplayName,
                Properties = client.Properties,
                UUID = client.UUID
            };
        }

        throw new AuthenticationException("Error occoured when trying to log in.", exc);
    }

    public Task<AuthenticationInfo> DoAsync(CancellationToken token)
    {
        return Task<AuthenticationInfo>.Factory.StartNew(Do, token);
    }
}
