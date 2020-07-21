using System;
using System.Collections.Generic;
using System.Text;
using BedrockServerConfigurator.Library.Location;
using BedrockServerConfigurator.Library.Entities;

namespace BedrockServerConfigurator.Library.Commands
{
    public class CommandBuilder
    {
        /// <summary>
        /// Needs to be combined with other command, executes it on entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public Command ExecuteOnEntityBase(IEntity entity, Coordinate coordinate)
        {
            return new Command($"execute {entity.Name} {coordinate} ");
        }

        /// <summary>
        /// Executes a command on a given entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="coordinate"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public Command ExecuteOnEntityWithCommand(IEntity entity, Coordinate coordinate, Command command)
        {
            return ExecuteOnEntityBase(entity, coordinate) + command;
        }

        /// <summary>
        /// Summons mob on an entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="mob"></param>
        /// <returns></returns>
        public Command SummonMobOnEntity(IEntity entity, string mob)
        {
            return ExecuteOnEntityWithCommand(entity, new LocalCoordinate(), new Command($"summon {mob}"));
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

        /// <summary>
        /// Teleports entity to another
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public Command Teleport(IEntity from, IEntity to)
        {
            return new Command($"tp {from.Name} {to.Name}");
        }

        /// <summary>
        /// Teleports entity to a coordinate
        /// </summary>
        /// <param name="from"></param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public Command TeleportToCoordinate(IEntity from, Coordinate coordinate)
        {
            return new Command($"tp {from.Name} {coordinate}");
        }

        /// <summary>
        /// Teleports entity relatively to itself
        /// </summary>
        /// <param name="from"></param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public Command TeleportLocal(IEntity from, LocalCoordinate coordinate)
        {
            return ExecuteOnEntityWithCommand(from, new LocalCoordinate(), TeleportToCoordinate(from, coordinate));
        }

        /// <summary>
        /// Gives operator to a player
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public Command Op(IEntity player)
        {
            return new Command($"op {player.Name}");
        }

        /// <summary>
        /// Removes operator from a player
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public Command Deop(IEntity player)
        {
            return new Command("de") + Op(player);
        }
    }
}
