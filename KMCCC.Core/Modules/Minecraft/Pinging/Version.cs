namespace KMCCC.Modules.Minecraft.Pinging;

using LitJson;

public class Version
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("protocol")]
    public int Protocol { get; set; }
}
