using System;
using System.ComponentModel;
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

        public async Task SpawnMobsOnEntity(string target, string mob, int amount) =>
            await SpawnMobsOnEntity(new Entity(target), mob, amount);

        public async Task SpawnMobsOnEntity(IEntity target, string mob, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                await Server.RunCommandAsync(_builder.SummonMobOnEntity(target, mob));
            }
        }

        public async Task TeleportEntityToEntity(string from, string to) => 
            await TeleportEntityToEntity(new Entity(from), new Entity(to));

        public async Task TeleportEntityToEntity(IEntity from, IEntity to)
        {
            await Server.RunCommandAsync(_builder.Teleport(from, to));
        }

        public async Task TeleportEntityPublic(string from, float x, float y, float z) => 
            await TeleportEntityPublic(new Entity(from), x, y, z);

        public async Task TeleportEntityPublic(IEntity from, float x, float y, float z)
        {
            await Server.RunCommandAsync(_builder.TeleportToCoordinate(from, new PublicCoordinate(x, y, z)));
        }

        public async Task TeleportEntityLocal(string from, float x, float y, float z) => 
            await TeleportEntityLocal(new Entity(from), x, y, z);

        public async Task TeleportEntityLocal(IEntity from, float x, float y, float z)
        {
            await Server.RunCommandAsync(_builder.TeleportLocal(from, new LocalCoordinate(x, y, z)));
        }

        public async Task TimeSet(string timeOfDay) => 
            await TimeSet(Enum.Parse<MinecraftTime>(timeOfDay, true));

        public async Task TimeSet(MinecraftTime timeOfDay)
        {
            await Server.RunCommandAsync(_builder.TimeSet(timeOfDay));
        }

        public async Task Say(string message)
        {
            await Server.RunCommandAsync(_builder.Say(message));
        }

        public async Task SayInColor(string message, string color) => 
            await SayInColor(message, Enum.Parse<MinecraftColor>(color, true));

        public async Task SayInColor(string message, MinecraftColor color)
        {
            await Server.RunCommandAsync(_builder.SayInColor(message, color));
        }

        public async Task AddEffect(string entityName, string effect, int seconds, byte amplifier, bool hideParticles = false) =>
            await AddEffect(entityName, Enum.Parse<MinecraftEffect>(effect, true), seconds, amplifier, hideParticles);

        public async Task AddEffect(string entityName, MinecraftEffect effect, int seconds, byte amplifier, bool hideParticles = false)
        {
            await Server.RunCommandAsync(_builder.AddEffect(new Entity(entityName), effect, seconds, amplifier, hideParticles));
        }

        public async Task Op(Player player)
        {
            await Server.RunCommandAsync(_builder.Op(player));
        }

        public async Task DeOp(Player player)
        {
            await Server.RunCommandAsync(_builder.DeOp(player));
        }

        public bool IsServerRunning()
        {
            return Server.Running;
        }
    }
}
