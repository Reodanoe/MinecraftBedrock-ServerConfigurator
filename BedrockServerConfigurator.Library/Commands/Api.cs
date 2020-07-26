using System;
using System.Threading.Tasks;
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
            if(Configurator.AllServers.TryGetValue(id, out Server server))
            {
                return server;
            }
            else
            {
                throw new IndexOutOfRangeException($"Server with id {id} wasn't found in Configurator.AllServers");
            }            
        }

        public async Task SpawnMobsOnAPlayer(int serverId, string playerName, string mob, int amount)
        {
            var server = GetServer(serverId);

            for (int i = 0; i < amount; i++)
            {
                await server.RunCommandAsync(Builder.SummonMobOnEntity(new Entity(playerName), mob));
            }
        }

        public async Task TeleportEntityToEntity(int serverId, string from, string to)
        {
            var server = GetServer(serverId);

            await server.RunCommandAsync(Builder.Teleport(new Entity(from), new Entity(to)));
        }

        public async Task TeleportEntityPublic(int serverId, string from, float x, float y, float z)
        {
            var server = GetServer(serverId);

            await server.RunCommandAsync(Builder.TeleportToCoordinate(new Entity(from), new PublicCoordinate(x, y, z)));
        }

        public async Task TeleportEntityLocal(int serverId, string from, float x, float y, float z)
        {
            var server = GetServer(serverId);

            await server.RunCommandAsync(Builder.TeleportLocal(new Entity(from), new LocalCoordinate(x, y, z)));
        }

        public async Task TimeSet(int serverId, string timeOfDay)
        {
            var server = GetServer(serverId);

            await server.RunCommandAsync(Builder.TimeSet(Enum.Parse<MinecraftTime>(timeOfDay, true)));
        }

        public async Task Say(int serverId, string message)
        {
            var server = GetServer(serverId);

            await server.RunCommandAsync(Builder.Say(message));
        }

        public async Task SayInColor(int serverId, string message, string color)
        {
            await SayInColor(serverId, message, Enum.Parse<MinecraftColor>(color, true));
        }

        public async Task SayInColor(int serverId, string message, MinecraftColor color)
        {
            var server = GetServer(serverId);

            await server.RunCommandAsync(Builder.SayInColor(message, color));
        }

        public async Task AddEffect(int serverId, string entityName, MinecraftEffect effect, int seconds, byte amplifier, bool hideParticles = false)
        {
            var server = GetServer(serverId);

            await server.RunCommandAsync(Builder.AddEffect(new Entity(entityName), effect, seconds, amplifier, hideParticles));
        }

        public async Task AddEffect(int serverId, string entityName, string effect, int seconds, byte amplifier, bool hideParticles = false) =>
            await AddEffect(serverId, entityName, Enum.Parse<MinecraftEffect>(effect, true), seconds, amplifier, hideParticles);

        public bool IsServerRunning(int serverId)
        {
            var server = GetServer(serverId);

            return server.Running;
        }
    }
}
