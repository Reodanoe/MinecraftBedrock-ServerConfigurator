using System.Collections.Generic;

namespace BedrockServerConfigurator.BlazorApp.Data
{
    public class ServerData
    {
        /// <summary>
        /// Command that is currently written in input of server component
        /// </summary>
        public string CurrentCommand { get; set; }

        /// <summary>
        /// List of commands that were run
        /// </summary>
        public List<string> Commands { get; set; } = new List<string>();

        /// <summary>
        /// Holds all messages server sent
        /// </summary>
        public List<string> ServerMessages { get; set; } = new List<string>();

        /// <summary>
        /// Holds info if server is listening to new messages
        /// </summary>
        public bool LoggingNewMessages { get; set; }

        /// <summary>
        /// Minigame running on a server
        /// </summary>
        public MinigameData Minigame { get; set; } = new MinigameData();

        /// <summary>
        /// Used for if user is changing properties of server
        /// </summary>
        public bool PropertyChanged { get; set; }
    }
}
