using System.Collections.Generic;

namespace BedrockServerConfigurator.Library.ServerFiles
{
    /// <summary>
    /// In server.properties file format
    /// </summary>
    public record Property(string Name, string Value, List<string> SummaryLines);
}
