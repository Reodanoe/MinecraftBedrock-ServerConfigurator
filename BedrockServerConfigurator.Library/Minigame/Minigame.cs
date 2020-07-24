using System;
using System.Collections.Generic;
using System.Linq;
using BedrockServerConfigurator.Library.Entities;

namespace BedrockServerConfigurator.Library.Minigame
{
    public class Minigame
    {
        public List<Microgame> Microgames { get; }
        public bool RunAllMicrogamesAtOnce { get; }

        /// <summary>
        /// Groups property Microgames by a player
        /// </summary>
        public Dictionary<ServerPlayer, List<Microgame>> GroupedMicrogamesByPlayer => 
            Microgames.GroupBy(a => a.Player).Select(b =>
            new
            {
                Player = b.Key,
                Microgames = b.ToList()
            }).ToDictionary(c => c.Player, d => d.Microgames);

        private readonly List<Microgame> runningMicrogames = new List<Microgame>();

        /// <summary>
        /// Class for running microgames
        /// </summary>
        /// <param name="microgames">If true all microgames are started for every player, if false, microgames take turns</param>
        /// <param name="runAllMicrogamesAtOnce"></param>
        public Minigame(List<Microgame> microgames, bool runAllMicrogamesAtOnce)
        {
            Microgames = microgames;
            RunAllMicrogamesAtOnce = runAllMicrogamesAtOnce;
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
                foreach (var item in GroupedMicrogamesByPlayer)
                {
                    var game = StartRandomMicrogameForPlayer(item.Key);
                    runningMicrogames.Add(game);
                }
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
                runningMicrogames.ForEach(x => x.StopMicrogame());
                runningMicrogames.Clear();
            }
        }

        private void MicrogameCreated(object sender, MicrogameEventArgs e)
        {
            // maybe save it to a list so I can see which ones are upcoming or something
            Console.WriteLine(e);
        }

        private void MicrogameEnded(Microgame game)
        {
            runningMicrogames.Remove(game);

            var newGame = RegisterNewMicrogame(game);
            runningMicrogames.Add(newGame);
        }

        private Microgame RandomPlayerMicrogame(ServerPlayer player) => 
            GroupedMicrogamesByPlayer[player].RandomElement();

        private Microgame RegisterNewMicrogame(Microgame oldGame)
        {
            oldGame.OnMicrogameCreated -= MicrogameCreated;
            oldGame.OnMicrogameEnded -= MicrogameEnded;

            var newGame = StartRandomMicrogameForPlayer(oldGame.Player);

            return newGame;
        }

        private Microgame StartRandomMicrogameForPlayer(ServerPlayer player)
        {
            var newGame = RandomPlayerMicrogame(player);
            newGame.OnMicrogameCreated += MicrogameCreated;
            newGame.OnMicrogameEnded += MicrogameEnded;
            newGame.StartMicrogame(false);

            return newGame;
        }
    }
}
