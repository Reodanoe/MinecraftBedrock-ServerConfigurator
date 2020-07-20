using System;
using System.Collections.Generic;
using System.Text;
using BedrockServerConfigurator.Library.Entities;
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
                server.RunACommand(Builder.SummonMobOnEntity(new Entity(playerName), mob));
            }
        }

        public void TeleportEntityToEntity(int serverId, string from, string to)
        {
            var server = GetServer(serverId);

            server.RunACommand(Builder.Teleport(new Entity(from), new Entity(to)));
        }

        public void TeleportEntityPublic(int serverId, string from, float x, float y, float z)
        {
            var server = GetServer(serverId);

            server.RunACommand(Builder.TeleportToCoordinate(new Entity(from), PublicCoordinate.GetPublicCoordinate(x, y, z)));
        }

        public void TeleportEntityLocal(int serverId, string from, float x, float y, float z)
        {
            var server = GetServer(serverId);

            server.RunACommand(Builder.TeleportLocal(new Entity(from), LocalCoordinate.GetLocalCoordinate(x, y, z)));
        }

        public void TimeSet(int serverId, string timeOfDay)
        {
            var server = GetServer(serverId);
            
            server.RunACommand(Builder.TimeSet(Enum.Parse<Time>(timeOfDay, true)));
        }
    }
}
