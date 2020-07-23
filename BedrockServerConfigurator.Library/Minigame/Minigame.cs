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
        private readonly List<Microgame> microgames;

        public bool RunAllMicrogamesAtOnce { get; }

        // this won't exist
        private Microgame runningSingleMicrogame;

        public Minigame(ServerPlayer player, Api api, bool runAllMicrogamesAtOnce) :
            this(BasicMicrogames(player, api), runAllMicrogamesAtOnce)
        {
        }

        public Minigame(List<Microgame> microgames, bool runAllMicrogamesAtOnce)
        {
            this.microgames = microgames;
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
                microgames.ForEach(x => x.OnMicrogameCreated += MicrogameCreated);

                microgames.ForEach(x => x.StartMicrogame());
            }
            else
            {
                // well, I didnt think this through completely
                // because those minigames arent bound to a player
                // it means that the players take turn
                // it does execute for each player separetly

                /* Something like this will be here instead
                var grouped = microgames.GroupBy(x => x.Player).Select(y =>
                new { 
                    Player = y.Key, 
                    Microgames = y.ToList()
                });

                foreach (var entry in grouped)
                {
                    var randomGame = entry.Microgames.RandomElement();
                    randomGame.DelayAndMicrogame();
                }
                */
                

                // runs microgames one by one, what could happen is that some players dont get to play
                runningSingleMicrogame = microgames.RandomElement();

                runningSingleMicrogame.OnMicrogameEnded += MicrogameEnded;
                runningSingleMicrogame.OnMicrogameCreated += MicrogameCreated;

                runningSingleMicrogame.StartMicrogame(RunAllMicrogamesAtOnce);
            }
        }

        public void Stop()
        {
            if (RunAllMicrogamesAtOnce)
            {
                microgames.ForEach(x => x.OnMicrogameCreated -= MicrogameCreated);

                microgames.ForEach(x => x.StopMicrogame());
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
