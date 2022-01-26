namespace KMCCC.Authentication;
#region

using System.Threading;
using System.Threading.Tasks;

#endregion

/// <summary>
/// Represents an authentication provider.
/// </summary>
public interface IAuthenticator
{
    /// <summary>
    /// Gets the type of this authenticator.
    /// </summary>
    string Type { get; }

    /// <summary>
    /// Authenticates this instance.
    /// </summary>
    /// <returns>An instance of <see cref="AuthenticationInfo"/> representing the result.</returns>
    AuthenticationInfo Do();

    /// <summary>
    /// Authenticates this instance.
    /// </summary>
    /// <returns>An instance of <see cref="AuthenticationInfo"/> representing the result.</returns>
    Task<AuthenticationInfo> DoAsync(CancellationToken token);
}
