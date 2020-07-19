using System;
using System.Collections.Generic;
using System.Text;
using BedrockServerConfigurator.Library.Models;
using BedrockServerConfigurator.Library.Location;

namespace BedrockServerConfigurator.Library.Commands
{
    public class CommandBuilder
    {
        /// <summary>
        /// Spawns amount of mobs on a player
        /// </summary>
        /// <param name="playerName">Username of a player in MineCraft server</param>
        /// <param name="mob">Mob name</param>
        /// <returns></returns>
        public Command SummonMobOnPlayer(string playerName, string mob)
        {
            return new Command($"execute {playerName} ~ ~ ~ /summon {mob}");
        }

        /// <summary>
        /// Sets time of world
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public Command TimeSet(Time time)
        {
            return new Command($"time set {time}");
        }

        public Command Teleport(IEntity from, IEntity to) => Teleport(from.Name, to.Name);

        public Command Teleport(string from, string to)
        {
            return new Command($"tp {from} {to}");
        }

        public Command Teleport(IEntity from, Coordinate coordinate) => Teleport(from.Name, coordinate);

        public Command Teleport(string from, Coordinate coordinate)
        {
            return new Command($"tp {from} {coordinate}");
        }
    }
}
