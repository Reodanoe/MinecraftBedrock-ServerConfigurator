using System;

namespace BedrockServerConfigurator.Library.Entities
{
    public class ServerPlayer : Player
    {
        public bool IsOnline { get; internal set; }
        public DateTime LastAction { get; internal set; }
        public int ServerId { get; internal set; }
    }
}
