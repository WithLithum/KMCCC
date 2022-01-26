namespace KMCCC.Authentication;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents an authenication info.
/// </summary>
public class AuthenticationInfo
{
    /// <summary>
    /// Gets or sets the name of the player.
    /// </summary>
    public string DisplayName { get; set; } = "Player";

    /// <summary>
    /// Gets the Universal Unique Identifier of this instance.
    /// </summary>
    public Guid UUID { get; set; }

    /// <summary>
    /// Gets the session token of this instance.
    /// </summary>
    public string AccessToken { get; set; } = "";

    /// <summary>
    /// Gets or sets the additional properties of this instance.
    /// </summary>
    public string Properties { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the error info of this instance.
    /// </summary>
    /// <remarks>
    /// No longer supported. Users relying on this property should use exceptions.
    /// </remarks>
    [Obsolete("Throw and catch exceptions when neccessary.")]
    public string Error { get; set; } = "";

    /// <summary>
    /// Gets or sets the user type.
    /// </summary>
    /// <value>
    /// Either <c>Legacy</c> or <c>Mojang</c>.
    /// </value>
    public string UserType { get; set; } = "Mojang";

    /// <summary>
    /// Gets or sets the other authentication information of this instance.
    /// </summary>
    public Dictionary<string, string> AdvancedInfo { get; } = new Dictionary<string, string>();
}
