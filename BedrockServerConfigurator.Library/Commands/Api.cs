using System;
using System.Collections.Generic;
using System.Text;
using BedrockServerConfigurator.Library.Location;

namespace BedrockServerConfigurator.Library.Commands
{
    public class Api
    {
        public Configurator Configurator { get; }
        public CommandBuilder Builder { get; } = new CommandBuilder();

        public Api(Configurator configurator)
        {
            Configurator = configurator;
        }

        private Server GetServer(int id)
        {
            return Configurator.AllServers[id];
        }

        public void SpawnMobsOnAPlayer(int serverId, string playerName, string mob, int amount)
        {
            var server = GetServer(serverId);

            for (int i = 0; i < amount; i++)
            {
                server.RunACommand(Builder.SummonMobOnPlayer(playerName, mob));
            }
        }

        public void Teleport(int serverId, string from, string to)
        {
            var server = GetServer(serverId);

            server.RunACommand(Builder.Teleport(from, to));
        }

        public void Teleport(int serverId, string from, float x, float y, float z)
        {
            var server = GetServer(serverId);

            server.RunACommand(Builder.Teleport(from, new Coordinate(new PublicPoint(Axis.X, x),
                                                                     new PublicPoint(Axis.Y, y), 
                                                                     new PublicPoint(Axis.Z, z))));
        }

        public void TimeSet(int serverId, string timeOfDay)
        {
            var server = GetServer(serverId);

            server.RunACommand(Builder.TimeSet(Enum.Parse<Time>(timeOfDay)));
        }
    }
}
