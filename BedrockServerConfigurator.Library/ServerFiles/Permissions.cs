using Newtonsoft.Json;

namespace BedrockServerConfigurator.Library.ServerFiles
{
    public class Permissions
    {
        [JsonProperty("permission")]
        public MinecraftPermission Permission { get; internal set; }

        [JsonProperty("xuid")]
        public long Xuid { get; internal set; }
    }
}
