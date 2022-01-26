namespace KMCCC.Authentication;
#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#endregion

/// <summary>
/// Provides offline authentication.
/// </summary>
/// <remarks>
/// Offline authentication are used when there is no internet available, or
/// needs to authenticate locally. Users who authenticates offline are
/// unable to join any servers with <c>online-mode=true</c> unless
/// re-authenticated through mods.
/// <para>
/// We recommend use online authentication (either Mojang or third-party)
/// whenever possible. If you are using this authenticator as a fallback,
/// then you might as well use FallbackAuthenticator (WIP), with existing UUID
/// and username.
/// </para>
/// </remarks>
public class OfflineAuthenticator : IAuthenticator
{
    /// <summary>
    /// Gets the name of the player.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OfflineAuthenticator"/> class.
    /// UUID is calculated from player's name.
    /// </summary>
    /// <param name="displayName">The name of the player.</param>
    /// <exception cref="ArgumentNullException">The display name is null.</exception>
    /// <exception cref="ArgumentException">The display is invalid.</exception>
    public OfflineAuthenticator(string displayName)
    {
        if (displayName == null) throw new ArgumentNullException(nameof(displayName));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Empty display name.", nameof(displayName));
        if (displayName.Any(char.IsWhiteSpace)) throw new ArgumentException("Display name cannot contain whitespaces.", nameof(displayName));

        DisplayName = displayName;
    }

    /// <inheritdoc />
    public string Type => "KMCCC.Offline";

    /// <inheritdoc />
    /// <exception cref="FormatException">The display name is invalid.</exception>
    /// <exception cref="InvalidOperationException">The display name is empty.</exception>
    public AuthenticationInfo Do()
    {
        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            throw new InvalidOperationException("Display name is empty.");
        }
        if (DisplayName.Any(char.IsWhiteSpace))
        {
            throw new FormatException("Display name cannot contain white spaces.");
        }
        return new AuthenticationInfo
        {
            AccessToken = Guid.NewGuid().ToString(),
            DisplayName = DisplayName,
            UUID = Tools.UsefulTools.GetPlayerUuid("DisplayName"),
            UserType = "mojang"
        };
    }

    /// <inheritdoc />
    /// <exception cref="FormatException">The display name is invalid.</exception>
    /// <exception cref="InvalidOperationException">The display name is empty.</exception>
    public Task<AuthenticationInfo> DoAsync(CancellationToken token)
    {
        return Task.Factory.StartNew(Do, token);
    }
}
