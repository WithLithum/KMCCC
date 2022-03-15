using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net.Http.Json;
using System.Reflection.Metadata.Ecma335;

namespace KMCCC.Modules.Yggdrasil;

#region

#endregion

/// <summary>
/// Provides an implementation of a Yggdrasil authentication client.
/// </summary>
public class YggdrasilClient
{
    public const string MojangAuthServer = "https://authserver.mojang.com";
    public const string MojangProfileServer = "https://sessionserver.mojang.com/session/minecraft/profile/";
    
    private string Auth_Authentication => _authServer + "/authenticate";
    private string Auth_Refresh => _authServer + "/refresh";
    private string Auth_Validate => _authServer + "/validate";

    private readonly object _locker = new object();
    private readonly string _authServer;

    public YggdrasilClient(string? authServer = null) : this(authServer, Guid.NewGuid())
    {
    }

    public YggdrasilClient(Guid clientToken) : this(null, clientToken)
    {
    }

    public YggdrasilClient(string? authServer, Guid clientToken)
    {
        _authServer = authServer ?? MojangAuthServer;
        ClientToken = clientToken;
    }

    public Guid ClientToken { get; }
    public string AccessToken { get; private set; } = "";
    public Guid UUID { get; private set; }
    public string DisplayName { get; private set; } = "Player";
    public string Properties { get; private set; } = "{}";
    public string AccountType { get; private set; } = "mojang";

    public void Clear()
    {
        lock (_locker)
        {
            AccessToken = Guid.Empty.ToString();
            UUID = Guid.Empty;
            DisplayName = string.Empty;
            Properties = string.Empty;
            AccountType = string.Empty;
        }
    }

    private void UpdateFomrResponse(AuthenticationResponse response)
    {
        AccessToken = response.AccessToken;
        if (response.User != null)
        {
            AccountType = response.User.Legacy ? "Legacy" : "Mojang";
            Properties = response.User.Properties != null
                ? response.User.Properties.ToJson()
                : "{}";
        }
        else
        {
            AccountType = "Mojang";
            Properties = "{}";
        }
        DisplayName = response.SelectedProfile.Name;
        UUID = Guid.Parse(response.SelectedProfile.Id);
    }

    public Exception? Refresh(string accessToken, bool twitch = true)
    {
        lock (_locker)
        {
            Clear();
            try
            {
                using (var hc = new HttpClient())
                {
                    var request = new RefreshRequest
                    {
                        Agent = Agent.Minecraft,
                        AccessToken = accessToken,
                        RequestUser = twitch,
                        ClientToken = ClientToken.ToString("N")
                    };

                    var response = hc.PostAsync(Auth_Refresh,
                        JsonContent.Create(request)).GetAwaiter().GetResult();

                    if (response == null)
                    {
                        throw new HttpRequestException("Request was null.");
                    }
                    response.EnsureSuccessStatusCode();
                    var responseBody = response.Content.ReadFromJsonAsync<AuthenticationResponse>()
                        .Result;

                    if (responseBody == null || responseBody.AccessToken == null || responseBody.SelectedProfile == null)
                    {
                        throw new HttpRequestException("Response or one of it's components were null. Make sure the account owns Minecraft.");
                    }

                    UpdateFomrResponse(responseBody);
                    return null;
                }
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }

    public Exception? AuthToken(String accessToken, Guid uuid, string displayName)
    {
        lock (_locker)
        {
            Clear();
            if (string.IsNullOrWhiteSpace(displayName)) return new Exception("displayName为空");
            try
            {
                WebRequest Http = WebRequest.Create(Auth_Validate);
                Http.Method = "POST";
                Http.ContentType = "application/json";
                Http.Timeout = 100000;
                var requestBody = JsonSerializer.Serialize(new ValidateRequest
                {
                    AccessToken = accessToken,
                    ClientToken = ClientToken.ToString("N"),
                });
                byte[] postdata = Encoding.UTF8.GetBytes(requestBody);
                Http.GetRequestStream().Write(postdata, 0, postdata.Length);

                using (HttpWebResponse hwr = (HttpWebResponse)Http.GetResponse())
                {
                    if (Convert.ToInt32(hwr.StatusCode) == 204)
                    {
                        var LoginInfo = new AuthenticationResponse()
                        {
                            AccessToken = accessToken,
                            ClientToken = ClientToken.ToString("N"),
                            SelectedProfile = new GameProfile()
                            {
                                Id = uuid.ToString("N"),
                                Name = displayName
                            }
                        };
                        UpdateFomrResponse(LoginInfo);
                        return null;
                    }
                    else
                    {
                        using (StreamReader sr = new StreamReader(hwr.GetResponseStream()))
                        {
                            var response = JsonSerializer.Deserialize<Error>(sr.ReadToEnd());
                            return new Exception(response.ErrorMessage);
                        }
                    }
                }
            }

            catch (WebException ex)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(((HttpWebResponse)ex.Response).GetResponseStream(), true))
                    {
                        var ErrorJson = JsonSerializer.Deserialize<Error>(sr.ReadToEnd());
                        return new Exception(ErrorJson.ErrorMessage);
                    }
                }
                catch
                {
                    return ex;
                }
            }
        }
    }


    #region Authenticate

    public Exception Authenticate(string email, string password, string ExToken = null, bool twitchEnabled = true)
    {
        lock (_locker)
        {
            Clear();
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("Content-Type", "application/json");
                    var requestBody = JsonSerializer.Serialize(new AuthenticationRequest
                    {
                        Agent = Agent.Minecraft,
                        Email = email,
                        Password = password,
                        token = ExToken,
                        RequestUser = twitchEnabled,
                        ClientToken = ClientToken.ToString("N")
                    });
                    var responseBody = wc.UploadString(new Uri(Auth_Authentication), requestBody);
                    var response = JsonSerializer.Deserialize<AuthenticationResponse>(responseBody);
                    if (response.AccessToken == null)
                    {
                        return new Exception("获取AccessToken失败");
                    }
                    if (response.SelectedProfile == null)
                    {
                        return new Exception("获取SelectedProfile失败，可能该账号没有购买游戏");
                    }
                    UpdateFomrResponse(response);
                    return null;
                }
            }
            catch (WebException ex)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(((HttpWebResponse)ex.Response).GetResponseStream(), true))
                    {

                        var ErrorJson = JsonSerializer.Deserialize<AuthenticationResponse>(sr.ReadToEnd());
                        return new Exception(ErrorJson.ErrorMessage);
                    }
                }
                catch
                {
                    return ex;
                }
            }
        }
    }

    public Task<bool> AuthenticateAsync(string email, string password, string ExToken = null, bool twitchEnabled = true,
        CancellationToken token = default(CancellationToken))
    {
        Clear();
        var task = new TaskCompletionSource<bool>(token);
        try
        {
            using (var wc = new WebClient())
            {
                var requestBody = JsonSerializer.Serialize(new AuthenticationRequest
                {
                    Agent = Agent.Minecraft,
                    Email = email,
                    Password = password,
                    RequestUser = twitchEnabled,
                    token = ExToken,
                    ClientToken = ClientToken.ToString("N")
                });
                wc.UploadStringCompleted += (sender, e) =>
                {
                    try
                    {
                        if (e.Error != null)
                        {
                            task.SetException(e.Error);
                            return;
                        }
                        var response = JsonSerializer.Deserialize<AuthenticationResponse>(e.Result);
                        if ((response.AccessToken == null) || (response.SelectedProfile == null))
                        {
                            task.SetResult(false);
                            return;
                        }
                        UpdateFomrResponse(response);
                        task.SetResult(true);
                    }
                    catch (Exception exception)
                    {
                        task.SetException(exception);
                    }
                };
                wc.UploadStringAsync(new Uri(Auth_Authentication), requestBody);
                return task.Task;
            }
        }
        catch (Exception exception)
        {
            task.SetException(exception);
            return task.Task;
        }
    }

    #endregion
}

#region Request & Response

public class RefreshRequest
{
    [JsonPropertyName("agent")]
    public Agent? Agent { get; set; }

    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("requestUser")]
    public bool RequestUser { get; set; }

    [JsonPropertyName("clientToken")]
    public string? ClientToken { get; set; }
}

public class ValidateRequest
{
    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("clientToken")]
    public string? ClientToken { get; set; }
}


public class AuthenticationRequest
{
    [JsonPropertyName("agent")]
    public Agent? Agent { get; set; }

    [JsonPropertyName("username")]
    public string? Email { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("requestUser")]
    public bool RequestUser { get; set; }

    [JsonPropertyName("clientToken")]
    public string? ClientToken { get; set; }

    [JsonPropertyName("token")]
    public string? token { get; set; }
}

public class AuthenticationResponse
{
    [JsonPropertyName("clientToken")]
    public string? ClientToken { get; set; }

    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("availableProfiles")]
    public List<GameProfile>? AvailableProfiles { get; set; }

    [JsonPropertyName("selectedProfile")]
    public GameProfile? SelectedProfile { get; set; }

    [JsonPropertyName("user")]
    public User? User { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("cause")]
    public string? Cause { get; set; }
}

#endregion

public class Error
{
    [JsonPropertyName("error")]
    public string ErrorType { get; set; } = "uh";

    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; } = "Unknown.";

}

public class Agent
{
    public static readonly Agent Minecraft = new Agent { Name = "Minecraft", Version = 1 };

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("version")]
    public int Version { get; set; }
}

public class User
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("properties")]
    public List<Property> Properties { get; set; }

    [JsonPropertyName("legacy")]
    public bool Legacy { get; set; }
}

public class Property
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}

public class GameProfile
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

internal static class Extensions
{
    internal static string ToJson(this List<Property> properties)
    {
        var sb = new StringBuilder().Append('{');
        foreach (var item in properties)
        {
            sb.Append('\"').Append(item.Name).Append("\":[\"").Append(item.Value).Append("\"]");
        }
        return sb.Append("}").ToString();
    }
}
