using System;
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

            server.RunACommand(Builder.TeleportToCoordinate(new Entity(from), new PublicCoordinate(x, y, z)));
        }

        public void TeleportEntityLocal(int serverId, string from, float x, float y, float z)
        {
            var server = GetServer(serverId);

            server.RunACommand(Builder.TeleportLocal(new Entity(from), new LocalCoordinate(x, y, z)));
        }

        public void TimeSet(int serverId, string timeOfDay)
        {
            var server = GetServer(serverId);

            server.RunACommand(Builder.TimeSet(Enum.Parse<MinecraftTime>(timeOfDay, true)));
        }

        public void Say(int serverId, string message)
        {
            var server = GetServer(serverId);

            server.RunACommand(Builder.Say(message));
        }

        public void SayInColor(int serverId, string message, string color)
        {
            SayInColor(serverId, message, Enum.Parse<MinecraftColor>(color, true));
        }

        public void SayInColor(int serverId, string message, MinecraftColor color)
        {
            var server = GetServer(serverId);

            server.RunACommand(Builder.SayInColor(message, color));
        }

        public void AddEffect(int serverId, string entityName, MinecraftEffect effect, int seconds, byte amplifier, bool hideParticles = false)
        {
            var server = GetServer(serverId);

            server.RunACommand(Builder.AddEffect(new Entity(entityName), effect, seconds, amplifier, hideParticles));
        }

        public void AddEffect(int serverId, string entityName, string effect, int seconds, byte amplifier, bool hideParticles = false) =>
            AddEffect(serverId, entityName, Enum.Parse<MinecraftEffect>(effect, true), seconds, amplifier, hideParticles);

        public bool IsServerRunning(int serverId)
        {
            var server = GetServer(serverId);

            return server.Running;
        }
    }
}
