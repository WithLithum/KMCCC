using System;
using System.Threading;
using System.Threading.Tasks;
using KMCCC.Modules.Yggdrasil;
using System.Collections.Generic;

namespace KMCCC.Authentication;

#region Login

/// <summary>
///     正版验证器（直接登陆）
/// </summary>
public class YggdrasilLogin : IAuthenticator
{
    /// <summary>
    ///     新建正版验证器
    /// </summary>
    /// <param name="email">电子邮件地址</param>
    /// <param name="password">密码</param>
    /// <param name="twitchEnabled">是否启用Twitch</param>
    /// <param name="clientToken">clientToken</param>
    /// <param name="authServer">验证服务器</param>
    public YggdrasilLogin(string email, string password, bool twitchEnabled, Guid clientToken, string token = null, string authServer = null)
    {
        Email = email;
        Password = password;
        TwitchEnabled = twitchEnabled;
        ClientToken = clientToken;
        AuthServer = authServer;
        Token = token;
    }

    /// <summary>
    ///     新建正版验证器(随机的新ClientToken，如果要使用Vaildate，不推荐)
    /// </summary>
    /// <param name="email">电子邮件地址</param>
    /// <param name="password">密码</param>
    /// <param name="twitchEnabled">是否启用Twitch</param>
    /// <param name="authServer">验证服务器</param>
    public YggdrasilLogin(string email, string password, bool twitchEnabled, string token = null, string authServer = null)
    {
        Email = email;
        Password = password;
        TwitchEnabled = twitchEnabled;
        AuthServer = authServer;
        Token = token;
    }

    /// <summary>
    ///     电子邮件地址
    /// </summary>
    public string Email { get; }

    /// <summary>
    ///     密码
    /// </summary>
    public string Password { get; }

    /// <summary>
    ///     是否启用Twitch
    /// </summary>
    public bool TwitchEnabled { get; }

    /// <summary>
    /// </summary>
    public Guid ClientToken { get; }

    /// <summary>
    ///     第三方服务器
    /// </summary>
    public string AuthServer { get; set; }

    /// <summary>
    ///     第三方验证服务器的一些验证Token（伪正版）
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    ///     返回Yggdrasil验证器类型
    /// </summary>
    public string Type => "KMCCC.Yggdrasil";

    public AuthenticationInfo Do()
    {
        var client = new YggdrasilClient(AuthServer, ClientToken);
        var LoginError = client.Authenticate(Email, Password, Token, TwitchEnabled);

        if (LoginError == null)
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
        return new AuthenticationInfo
        {
            Error = LoginError.Message
        };
    }

    public Task<AuthenticationInfo> DoAsync(CancellationToken token)
    {
        var client = new YggdrasilClient(AuthServer, ClientToken);
        return client.AuthenticateAsync(Email, Password, Token, TwitchEnabled, token).ContinueWith(task =>
        {
            if ((task.Exception == null) && (task.Result))
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
            return new AuthenticationInfo
            {
                Error = task.Exception.Message
            };
        }, token);
    }
}

#endregion

#region Refresh

/// <summary>
///     正版验证器（直接登陆）
/// </summary>
public class YggdrasilRefresh : IAuthenticator
{
    /// <summary>
    ///     新建正版验证器
    /// </summary>
    /// <param name="accessToken">合法的Token</param>
    /// <param name="twitchEnabled">是否启用Twitch</param>
    /// <param name="clientToken">clientToken</param>
    public YggdrasilRefresh(String accessToken, bool twitchEnabled, Guid clientToken, string authServer = null)
    {
        AccessToken = accessToken;
        TwitchEnabled = twitchEnabled;
        ClientToken = clientToken;
        AuthServer = authServer;
    }

    /// <summary>
    ///     新建正版验证器(随机的新ClientToken)
    /// </summary>
    /// <param name="accessToken">合法的Token</param>
    /// <param name="twitchEnabled">是否启用Twitch</param>
    public YggdrasilRefresh(String accessToken, bool twitchEnabled, string authServer = null)
        : this(accessToken, twitchEnabled, Guid.NewGuid(), authServer)
    {
    }

    public String AccessToken { get; }

    /// <summary>
    ///     是否启用Twitch
    /// </summary>
    public bool TwitchEnabled { get; }

    /// <summary>
    /// </summary>
    public Guid ClientToken { get; }

    /// <summary>
    /// </summary>
    public string AuthServer { get; set; }

    /// <summary>
    ///     返回Yggdrasil验证器类型
    /// </summary>
    public string Type => "KMCCC.Yggdrasil";

    public AuthenticationInfo Do()
    {
        var client = new YggdrasilClient(AuthServer, ClientToken);
        var LoginError = client.Refresh(AccessToken, TwitchEnabled);

        if (LoginError == null)
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
        return new AuthenticationInfo
        {
            Error = LoginError.Message
        };
    }

    public Task<AuthenticationInfo> DoAsync(CancellationToken token)
    {
        return Task<AuthenticationInfo>.Factory.StartNew(Do, token);
    }
}

#endregion

/// <summary>
/// Provides authentication with a remote Yggdrasil server.
/// </summary>
/// <remarks>
/// <para>
/// For performence reasons token is stored as <see cref="string"/>.
/// </para>
/// <para>
/// Authenticator validates the info, refreshes existing tokens, and log the player in.
/// Please make sure you are providing a <i>stored</i> token, and do not create a new
/// one each launch.
/// </para>
/// <para>
/// We do not recommend storing player's password in any way. You should use refreshing
/// tokens.
/// </para>
/// </remarks>
public class QueuedYggdrasilAuthenticator : IAuthenticator
{
    public QueuedYggdrasilAuthenticator(string email, string password, string accessToken, string clientToken, string uuid, string displayName, string? authServer = null)
    {
        Email = email;
        Password = password;
        AccessToken = accessToken;
        ClientToken = clientToken;
        UUID = uuid;
        DisplayName = displayName;
        AuthServer = authServer;
    }

    /// <summary>
    /// The email address or account name of this instance.
    /// </summary>
    public string Email { get; }

    /// <summary>
    /// The password of this instance.
    /// </summary>
    public string Password { get; }

    /// <summary>
    /// Gets the session token of this instance.
    /// </summary>
    public string AccessToken { get; }

    /// <summary>
    /// Gets the token of this instance.
    /// </summary>
    public string ClientToken { get; }

    /// <summary>
    /// Gets the display name of this insatnce.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the identifier of this instance.
    /// </summary>
    public string UUID { get; }

    /// <summary>
    /// Gets or sets the server to authenticate.
    /// </summary>
    public string? AuthServer { get; set; }

    /// <inheritdoc />
    public string Type => "KMCCC.Yggdrasil";

    private IEnumerable<IAuthenticator> TryQueue()
    {
        if (Guid.TryParse(ClientToken, out Guid ct))
        {
            if (Guid.TryParse(UUID, out Guid id))
                yield return new InternalYggdrasilAuthenticator(AccessToken, ct, id, DisplayName, AuthServer);
            yield return new YggdrasilRefresh(AccessToken, false, ct, AuthServer);
        }
        yield return new YggdrasilLogin(Email, Password, false, null, AuthServer);
    }

    public AuthenticationInfo Do()
    {
        foreach (IAuthenticator auth in TryQueue())
        {
            var AuthInfo = auth.Do();
            try
            {
                return new AuthenticationInfo()
                {
                    AccessToken = AuthInfo.AccessToken,
                    DisplayName = AuthInfo.DisplayName,
                    UUID = AuthInfo.UUID,
                    Properties = AuthInfo.Properties,
                };
            }
            catch
            {
                // We want it keep running
            }
        }
        throw new InvalidOperationException("Failed to authenticate with all queues.");
    }

    public Task<AuthenticationInfo> DoAsync(CancellationToken token)
    {
        return Task<AuthenticationInfo>.Factory.StartNew(Do, token);
    }
}
