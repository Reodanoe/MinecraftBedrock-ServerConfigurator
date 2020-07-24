using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BedrockServerConfigurator.Library.Commands;
using BedrockServerConfigurator.Library.Entities;
using BedrockServerConfigurator.Library.Minigame.Microgames;

namespace BedrockServerConfigurator.Library.Minigame
{
    public class Minigame
    {
        public List<Microgame> Microgames { get; }
        public bool RunAllMicrogamesAtOnce { get; }

        // this won't exist
        private Microgame runningSingleMicrogame;

        public Dictionary<ServerPlayer, List<Microgame>> groupedMicrogames => 
            Microgames.GroupBy(a => a.Player).Select(b =>
            new
            {
                Player = b.Key,
                Microgames = b.ToList()
            }).ToDictionary(c => c.Player, d => d.Microgames);


        public Minigame(ServerPlayer player, Api api, bool runAllMicrogamesAtOnce) :
            this(BasicMicrogames(player, api), runAllMicrogamesAtOnce)
        {
        }

        public Minigame(List<Microgame> microgames, bool runAllMicrogamesAtOnce)
        {
            Microgames = microgames;
            RunAllMicrogamesAtOnce = runAllMicrogamesAtOnce;
        }

        private void MicrogameCreated(object sender, MicrogameEventArgs e)
        {
            // maybe save it to a list so I can see which ones are upcoming or something
            Console.WriteLine(e);
        }        

        public void Start()
        {
            if (RunAllMicrogamesAtOnce)
            {
                Microgames.ForEach(x => x.OnMicrogameCreated += MicrogameCreated);

                Microgames.ForEach(x => x.StartMicrogame());
            }
            else
            {
                // well, I didnt think this through completely
                // because those minigames arent bound to a player
                // it means that the players take turn
                // it does execute for each player separetly

                // runs microgames one by one, what could happen is that some players dont get to play
                runningSingleMicrogame = Microgames.RandomElement();

                runningSingleMicrogame.OnMicrogameEnded += MicrogameEnded;
                runningSingleMicrogame.OnMicrogameCreated += MicrogameCreated;

                runningSingleMicrogame.StartMicrogame(RunAllMicrogamesAtOnce);
            }
        }

        public void Stop()
        {
            if (RunAllMicrogamesAtOnce)
            {
                Microgames.ForEach(x => x.OnMicrogameCreated -= MicrogameCreated);

                Microgames.ForEach(x => x.StopMicrogame());
            }
            else
            {
                runningSingleMicrogame.OnMicrogameEnded -= MicrogameEnded;
                runningSingleMicrogame.OnMicrogameCreated -= MicrogameCreated;

                runningSingleMicrogame.StopMicrogame();
            }
        }

        private void MicrogameEnded(Microgame game)
        {
            runningSingleMicrogame.OnMicrogameEnded -= MicrogameEnded;
            runningSingleMicrogame.OnMicrogameCreated -= MicrogameCreated;
            
            Start();
        }

        public static List<Microgame> BasicMicrogames(ServerPlayer player, Api api)
        {
            return new List<Microgame>
            {
                //new TeleportUpMicrogame(TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(3), player, api, 5, 20),
                //new SpawnRandomMobsMicrogame(TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(3), player, api, 3, 7),
                //new BadEffectMicrogame(TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(3), player, api)

                // testing
                new TeleportUpMicrogame(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20), player, api, 5, 20),
                new SpawnRandomMobsMicrogame(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20), player, api, 3, 7),
                new BadEffectMicrogame(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20), player, api)
            };
        }
    }
}
