using System.Collections.Generic;

namespace BedrockServerConfigurator.Library.ServerFiles
{
    public record Property(string Name, string Value, List<string> SummaryLines);
}
