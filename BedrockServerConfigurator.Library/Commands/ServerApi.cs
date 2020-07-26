using System;
using System.Threading.Tasks;
using BedrockServerConfigurator.Library.Entities;
using BedrockServerConfigurator.Library.Location;

namespace BedrockServerConfigurator.Library.Commands
{
    public class ServerApi
    {
        public Server Server { get; }
        private readonly CommandBuilder _builder = new CommandBuilder();

        public ServerApi(Server server)
        {
            Server = server;
        }

        public async Task SpawnMobsOnAPlayer(string playerName, string mob, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                await Server.RunCommandAsync(_builder.SummonMobOnEntity(new Entity(playerName), mob));
            }
        }

        public async Task TeleportEntityToEntity(string from, string to)
        {
            await Server.RunCommandAsync(_builder.Teleport(new Entity(from), new Entity(to)));
        }

        public async Task TeleportEntityPublic(string from, float x, float y, float z)
        {
            await Server.RunCommandAsync(_builder.TeleportToCoordinate(new Entity(from), new PublicCoordinate(x, y, z)));
        }

        public async Task TeleportEntityLocal(string from, float x, float y, float z)
        {
            await Server.RunCommandAsync(_builder.TeleportLocal(new Entity(from), new LocalCoordinate(x, y, z)));
        }

        public async Task TimeSet(string timeOfDay)
        {
            await Server.RunCommandAsync(_builder.TimeSet(Enum.Parse<MinecraftTime>(timeOfDay, true)));
        }

        public async Task Say(string message)
        {
            await Server.RunCommandAsync(_builder.Say(message));
        }

        public async Task SayInColor(string message, string color)
        {
            await SayInColor(message, Enum.Parse<MinecraftColor>(color, true));
        }

        public async Task SayInColor(string message, MinecraftColor color)
        {
            await Server.RunCommandAsync(_builder.SayInColor(message, color));
        }

        public async Task AddEffect(string entityName, MinecraftEffect effect, int seconds, byte amplifier, bool hideParticles = false)
        {
            await Server.RunCommandAsync(_builder.AddEffect(new Entity(entityName), effect, seconds, amplifier, hideParticles));
        }

        public async Task AddEffect(string entityName, string effect, int seconds, byte amplifier, bool hideParticles = false) =>
            await AddEffect(entityName, Enum.Parse<MinecraftEffect>(effect, true), seconds, amplifier, hideParticles);

        public bool IsServerRunning()
        {
            return Server.Running;
        }
    }
}
