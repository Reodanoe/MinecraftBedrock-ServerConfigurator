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
        private List<Microgame> microgames;

        public bool RunAllMicrogamesAtOnce { get; }

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

        private Microgame runningSingleMicrogame;

        public void Start()
        {
            if (RunAllMicrogamesAtOnce)
            {
                microgames.ForEach(x => x.OnMicrogameCreated += MicrogameCreated);

                microgames.ForEach(x => x.StartMicrogame());
            }
            else
            {
                runningSingleMicrogame = microgames.RandomElement();

                // work on this
                runningSingleMicrogame.OnMicrogameEnded += MicrogameEnded;
                runningSingleMicrogame.OnMicrogameCreated += MicrogameCreated;

                runningSingleMicrogame.StartMicrogame();
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
                // should unregister events here too

                runningSingleMicrogame.StopMicrogame();
            }
        }

        private void MicrogameEnded(Microgame game)
        {
            // runs the log to `MicrogameCreated` twice, fix it

            game.StopMicrogame();

            Start();
        }

        public static List<Microgame> BasicMicrogames(ServerPlayer player, Api api)
        {
            return new List<Microgame>
            {
                new TeleportUpMicrogame(TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(3), player, api, 5, 20),
                new SpawnRandomMobsMicrogame(TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(3), player, api, 3, 7),
                new BadEffectMicrogame(TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(3), player, api)

                // testing
                //new TeleportUpMicrogame(TimeSpan.Zero, TimeSpan.FromSeconds(20), player, api, 5, 20),
                //new SpawnRandomMobsMicrogame(TimeSpan.Zero, TimeSpan.FromSeconds(20), player, api, 3, 7),
                //new BadEffectMicrogame(TimeSpan.Zero, TimeSpan.FromSeconds(20), player, api)
            };
        }
    }
}
