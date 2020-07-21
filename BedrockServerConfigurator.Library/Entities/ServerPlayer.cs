using System;
using System.Collections.Generic;
using System.Text;

namespace BedrockServerConfigurator.Library.Entities
{
    public class ServerPlayer : Player
    {
        public bool IsOnline { get; set; }
        public DateTime LastAction { get; set; }
        public int ServerId { get; set; }
    }
}
