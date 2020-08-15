using System.Collections.Generic;
using BedrockServerConfigurator.Library.Commands;
using BedrockServerConfigurator.Library.Entities;
using BedrockServerConfigurator.Library.Minigame;

namespace BedrockServerConfigurator.BlazorApp.Data
{
    public class MinigameData
    {
        /// <summary>
        /// Players that are selected for a minigame
        /// </summary>
        public List<ServerPlayer> SelectedPlayers { get; set; } = new List<ServerPlayer>();

        /// <summary>
        /// The minigame instance that runs microgames
        /// </summary>
        public Minigame Minigame { get; set; } = new Minigame();

        /// <summary>
        /// ServerApi that sends commands
        /// </summary>
        public ServerApi Api { get; set; }
    }
}