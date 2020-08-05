using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        public ObservableCollection<string> Messages { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Holds info if server is listening to new messages
        /// </summary>
        public bool LoggingNewMessages { get; set; }
    }
}
