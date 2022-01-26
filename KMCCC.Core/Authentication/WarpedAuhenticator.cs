namespace KMCCC.Authentication;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides authentication with arbitrary <see cref="AuthenticationInfo"/>.
/// </summary>
public class WarpedAuhenticator : IAuthenticator
{
    private readonly AuthenticationInfo _info;

    /// <summary>
    /// Initializes a new instance of the <see cref="WarpedAuhenticator"/>
    /// </summary>
    /// <param name="info">The <see cref="AuthenticationInfo"/> to provide.</param>
    public WarpedAuhenticator(AuthenticationInfo info)
    {
        _info = info;
    }

    /// <inheritdoc />
    public string Type => "KMCCC.Warped";

    /// <inheritdoc />
    public AuthenticationInfo Do()
    {
        return _info;
    }

    /// <inheritdoc />
    public Task<AuthenticationInfo> DoAsync(CancellationToken token)
    {
        // No need to start a new thread, just give them info
        return Task.FromResult(_info);
    }
}
