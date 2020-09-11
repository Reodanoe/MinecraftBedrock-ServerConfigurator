using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<List<Command>> SpawnMobsOnEntity(string target, string mob, int amount) =>
            await SpawnMobsOnEntity(new Entity(target), mob, amount);

        public async Task<List<Command>> SpawnMobsOnEntity(IEntity target, string mob, int amount)
        {
            var ranCommands = new List<Command>(amount);

            for (int i = 0; i < amount; i++)
            {
                ranCommands.Add(await Server.RunCommandAsync(_builder.SummonMobOnEntity(target, mob)));
            }

            return ranCommands;
        }

        public async Task<Command> TeleportEntityToEntity(string from, string to) => 
            await TeleportEntityToEntity(new Entity(from), new Entity(to));

        public async Task<Command> TeleportEntityToEntity(IEntity from, IEntity to)
        {
            return await Server.RunCommandAsync(_builder.Teleport(from, to));
        }

        public async Task<Command> TeleportEntityPublic(string from, float x, float y, float z) => 
            await TeleportEntityPublic(new Entity(from), x, y, z);

        public async Task<Command> TeleportEntityPublic(IEntity from, float x, float y, float z)
        {
            return await Server.RunCommandAsync(_builder.TeleportToCoordinate(from, new PublicCoordinate(x, y, z)));
        }

        public async Task<Command> TeleportEntityLocal(string from, float x, float y, float z) => 
            await TeleportEntityLocal(new Entity(from), x, y, z);

        public async Task<Command> TeleportEntityLocal(IEntity from, float x, float y, float z)
        {
            return await Server.RunCommandAsync(_builder.TeleportLocal(from, new LocalCoordinate(x, y, z)));
        }

        public async Task<Command> TimeSet(string timeOfDay) => 
            await TimeSet(Enum.Parse<MinecraftTime>(timeOfDay, true));

        public async Task<Command> TimeSet(MinecraftTime timeOfDay)
        {
            return await Server.RunCommandAsync(_builder.TimeSet(timeOfDay));
        }

        public async Task<Command> Say(string message)
        {
            return await Server.RunCommandAsync(_builder.Say(message));
        }

        public async Task<Command> SayInColor(string message, string color) => 
            await SayInColor(message, Enum.Parse<MinecraftColor>(color, true));

        public async Task<Command> SayInColor(string message, MinecraftColor color)
        {
            return await Server.RunCommandAsync(_builder.SayInColor(message, color));
        }

        public async Task<Command> AddEffect(string entityName, string effect, int seconds, byte amplifier, bool hideParticles = false) =>
            await AddEffect(entityName, Enum.Parse<MinecraftEffect>(effect, true), seconds, amplifier, hideParticles);

        public async Task<Command> AddEffect(string entityName, MinecraftEffect effect, int seconds, byte amplifier, bool hideParticles = false)
        {
            return await Server.RunCommandAsync(_builder.AddEffect(new Entity(entityName), effect, seconds, amplifier, hideParticles));
        }

        public async Task<Command> SetOperator(Player player)
        {
            return await Server.RunCommandAsync(_builder.SetOperator(player));
        }

        public async Task<Command> SetMember(Player player)
        {
            return await Server.RunCommandAsync(_builder.SetMember(player));
        }

        public async Task<Command> SetVisitor(Player player)
        {
            return await Server.RunCommandAsync(_builder.SetVisitor(player));
        }

        public async Task<MinecraftPermission> GetPlayerPermissionAsync(Player player)
        {
            var playerPermission = (await Server.GetPermissionsAsync()).FirstOrDefault(x => x.Xuid == player.Xuid);

            return playerPermission?.Permission ?? Server.ServerProperties.DefaultPlayerPermissionLevel;
        }
    }
}
