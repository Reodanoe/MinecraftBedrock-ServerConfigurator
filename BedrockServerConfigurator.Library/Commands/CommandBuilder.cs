using System;
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
        public Command TimeSet(MinecraftTime time)
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
        /// Sets player as operator
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public Command SetOperator(Player player)
        {
            return new Command($"op {player.Username}");
        }

        /// <summary>
        /// Sets player as member
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public Command SetMember(Player player)
        {
            return new Command("de") + SetOperator(player);
        }

        /// <summary>
        /// Sets player as visitor
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public Command SetVisitor(Player player)
        {
            throw new NotImplementedException("Can't set player as a visitor");
        }

        /// <summary>
        /// Says message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Command Say(string message)
        {
            return new Command($"say {message}");
        }

        /// <summary>
        /// Says message in color
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public Command SayInColor(string message, MinecraftColor color)
        {
            return Say(ColorMessage(message, color).MinecraftCommand);
        }

        /// <summary>
        /// Appends color to a message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public Command ColorMessage(string message, MinecraftColor color)
        {
            return new Command($"§{(int)color:x}{message}");
        }

        /// <summary>
        /// Adds effect to an entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="effect"></param>
        /// <param name="seconds"></param>
        /// <param name="amplifier"></param>
        /// <param name="hideParticles"></param>
        /// <returns></returns>
        public Command AddEffect(IEntity entity, MinecraftEffect effect, int seconds, byte amplifier, bool hideParticles = false)
        {
            return new Command($"effect {entity.Name} {effect} {seconds} {amplifier} {hideParticles}");
        }
    }
}
