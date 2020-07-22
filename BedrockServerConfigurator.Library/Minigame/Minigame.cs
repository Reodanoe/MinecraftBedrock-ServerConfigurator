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
    public class Minigame
    {
        private readonly ServerPlayer player;
        private readonly Api api;

        private List<Microgame> microgames;

        public Minigame(ServerPlayer player, Api api) : this(player, api, BasicMicrogames(player, api))
        { 
        }

        public Minigame(ServerPlayer player, Api api, List<Microgame> microgames)
        {
            this.player = player;
            this.api = api;
            this.microgames = microgames;

            microgames.ForEach(x => x.OnCreatedMicrogame += MicrogameCreated);
        }

        private void MicrogameCreated(object sender, MicrogameEventArgs e)
        {
            // maybe save it to a list so I can see which ones are upcoming or something
            Console.WriteLine(e);
        }

        public void Start()
        {
            microgames.ForEach(x => x.StartMicrogame());
        }

        public void Stop()
        {
            microgames.ForEach(x => x.StopMicrogame());
        }

        public static List<Microgame> BasicMicrogames(ServerPlayer player, Api api)
        {
            return new List<Microgame>
            {
                new TeleportUpMicrogame(TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2), player, api, 5, 20),
                new SpawnRandomMobsMicrogame(TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2), player, api, 3, 7),
                new BadEffectMicrogame(TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2), player, api)
            };
        }
    }
}
