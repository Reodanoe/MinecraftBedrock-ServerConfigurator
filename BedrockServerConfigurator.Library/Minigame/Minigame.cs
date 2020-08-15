using System;
using System.Collections.Generic;
using System.Linq;
using BedrockServerConfigurator.Library.Entities;

namespace BedrockServerConfigurator.Library.Minigame
{
    public class Minigame
    {
        public List<Microgame> Microgames { get; private set; }

        public bool Running { get; private set; }

        /// <summary>
        /// Groups property Microgames by a player
        /// </summary>
        public Dictionary<ServerPlayer, List<Microgame>> GroupedMicrogamesByPlayer =>
            Microgames.GroupBy(a => a.Player)
                      .ToDictionary(b => b.Key, c => c.ToList());

        private readonly List<Microgame> runningMicrogames = new List<Microgame>();

        public void Start(bool runAllMicrogamesAtOnce)
        {
            if (runAllMicrogamesAtOnce)
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

                // for each player a microgame will start
                // that microgame is selected randomly from the microgames that are assigned to a player
                // when it ends it then selects a new random microgame and starts it again
            }

            Running = true;
        }

        public void Stop()
        {
            runningMicrogames.ForEach(x => x.StopMicrogame());

            runningMicrogames.Clear();

            Running = false;
        }

        public void SetMicrogames(List<Microgame> microgames)
        {
            if (!Running)
            {
                Microgames = microgames;
            }
            else
            {
                throw new Exception("Can't set microgames while they're running");
            }
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
