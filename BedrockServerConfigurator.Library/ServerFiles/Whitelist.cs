using System;
using System.Collections.Generic;
using System.Text;

namespace BedrockServerConfigurator.Library.ServerFiles
{
    public class Whitelist
    {
        public string Name { get; set; }
        public long Xuid { get; set; }
        public bool IgnoresPlayerLimit { get; set; }
    }
}
