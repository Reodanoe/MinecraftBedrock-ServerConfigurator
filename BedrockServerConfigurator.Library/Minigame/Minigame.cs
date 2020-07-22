using BedrockServerConfigurator.Library.Commands;
using BedrockServerConfigurator.Library.Entities;
using BedrockServerConfigurator.Library.Minigame.Microgames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BedrockServerConfigurator.Library.Minigame
{
    /// <summary>
    /// This class creates random minigames for a player to deal with
    /// </summary>
    public class MiniGame
    {
        private readonly ServerPlayer player;
        private readonly Api api;

        public Player ServerPlayer => player;

        private List<Microgame> microgames;

        public MiniGame(ServerPlayer player, Api api)
        {
            this.player = player;
            this.api = api;

            var basicMicrogames = BasicMicrogames();
            microgames = basicMicrogames;

            Start();
        }

        private void Start()
        {
            // two modes
            // microgames take turns
            // they all start at the same time
        }

        private List<Microgame> BasicMicrogames()
        {
            return new List<Microgame>
            {
                new TeleportUpMicrogame(TimeSpan.FromSeconds(20), TimeSpan.FromMinutes(2), 5, 20),
                new SpawnRandomMobsMicrogame(TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2), 3, 7),
                new BadEffectMicrogame(TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2))
            };
        }
    }
}
