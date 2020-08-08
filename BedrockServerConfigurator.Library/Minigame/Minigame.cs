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
            Microgames.GroupBy(a => a.Player)
                      .ToDictionary(b => b.Key, c => c.ToList());

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
                runningMicrogames.AddRange(Microgames);

                foreach (var game in runningMicrogames)
                {
                    game.OnMicrogameCreated += MicrogameCreated;
                    game.StartMicrogame(true);
                }
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
            runningMicrogames.ForEach(x => x.StopMicrogame());

            runningMicrogames.Clear();
        }

        private void MicrogameCreated(object sender, MicrogameEventArgs e)
        {
            // maybe save it to a list so I can see which ones are upcoming or something
            Console.WriteLine(e);
        }

        /// <summary>
        /// When a game ends it starts a new one
        /// </summary>
        /// <param name="game"></param>
        private void Solo_MicrogameEnded(object sender, MicrogameEventArgs game)
        {
            runningMicrogames.Remove(game.Sender);

            var newGame = RegisterNewSoloMicrogame(game.Sender);
            runningMicrogames.Add(newGame);
        }

        /// <summary>
        /// Replaces old microgame of a player for a new one
        /// </summary>
        /// <param name="oldGame"></param>
        /// <returns></returns>
        private Microgame RegisterNewSoloMicrogame(Microgame oldGame)
        {
            oldGame.OnMicrogameCreated -= MicrogameCreated;
            oldGame.OnMicrogameEnded -= Solo_MicrogameEnded;

            var newGame = StartRandomMicrogameForPlayer(oldGame.Player);

            return newGame;
        }

        /// <summary>
        /// Selects a new microgame for a player and starts it
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private Microgame StartRandomMicrogameForPlayer(ServerPlayer player)
        {
            var newGame = RandomPlayerMicrogame(player);
            newGame.OnMicrogameCreated += MicrogameCreated;
            newGame.OnMicrogameEnded += Solo_MicrogameEnded;
            newGame.StartMicrogame(false);

            return newGame;
        }

        /// <summary>
        /// Selects a new random microgame that's registered under a player
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private Microgame RandomPlayerMicrogame(ServerPlayer player) =>
            GroupedMicrogamesByPlayer[player].RandomElement();
    }
}
