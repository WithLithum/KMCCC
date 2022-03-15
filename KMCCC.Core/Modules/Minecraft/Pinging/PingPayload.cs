namespace KMCCC.Modules.Minecraft.Pinging;

using System.Text.Json.Serialization;

/// <summary>
/// C# represenation of the following JSON file
/// https://gist.github.com/thinkofdeath/6927216
/// 参数信息请参见 http://wiki.vg/Server_List_Ping
/// </summary>
public class PingPayload
{
    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    [JsonPropertyName("version")]
    public Version? Version { get; set; }

    /// <summary>
    /// Gets or sets the players information of this ping payload.
    /// </summary>
    [JsonPropertyName("players")]
    public Players? Players { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [JsonPropertyName("description")]
    public Description? Description { get; set; }

    /// <summary>
    /// Gets or sets the information about modifications.
    /// </summary>
    [JsonPropertyName("modinfo")]
    public Modinfo? Modifications { get; set; }

    /// <summary>
    /// Gets or sets the icon of the server.
    /// </summary>
    [JsonPropertyName("favicon")]
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the error information.
    /// </summary>
    [JsonPropertyName("error")]
    public string? ErrorInfo { get; set; }
}
