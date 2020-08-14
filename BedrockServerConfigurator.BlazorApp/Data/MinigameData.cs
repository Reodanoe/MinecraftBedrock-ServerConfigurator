using System.Collections.Generic;
using BedrockServerConfigurator.Library.Commands;
using BedrockServerConfigurator.Library.Entities;
using BedrockServerConfigurator.Library.Minigame;

namespace BedrockServerConfigurator.BlazorApp.Data
{
    public class MinigameData
    {
        public List<ServerPlayer> SelectedPlayers { get; set; } = new List<ServerPlayer>();

        public Minigame Minigame { get; set; }

        public bool Running { get; set; }

        public ServerApi Api { get; set; }
    }
}