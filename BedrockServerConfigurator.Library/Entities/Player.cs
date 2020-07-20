using System;

namespace BedrockServerConfigurator.Library.Entities
{
    public class Player : IEntity
    {
        public string Username { get; set; }
        public long Xuid { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastAction { get; set; }
        public string Name => Username;
    }
}
