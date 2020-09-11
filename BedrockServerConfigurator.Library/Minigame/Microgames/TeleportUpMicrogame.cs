using System;
using System.Threading.Tasks;
using BedrockServerConfigurator.Library.Commands;
using BedrockServerConfigurator.Library.Entities;

namespace BedrockServerConfigurator.Library.Minigame.Microgames
{
    public class TeleportUpMicrogame : Microgame
    {
        public int MinBlocks { get; }
        public int MaxBlocks { get; }

        public TeleportUpMicrogame(TimeSpan minDelay, TimeSpan maxDelay, ServerPlayer player, ServerApi api, int minBlocks, int maxBlocks) :
            base(minDelay, maxDelay, player, api)
        {
            MinBlocks = minBlocks;
            MaxBlocks = maxBlocks;
        }

        public override Func<Task> GetGame()
        {
            var amount = Utilities.RandomGenerator.Next(MinBlocks, MaxBlocks + 1);

            MicrogameCreated(new MicrogameEventArgs(this, $"Blocks: {amount}"));

            string[] messages =
            {
                $"Hey {Player.Name} I hope you know how to fly, it's just {amount} blocks in the air.",
                $"{Player.Name} can you take a better look at the weather up there, {amount} blocks high?",
                $"Don't mind me {Player.Name}, I'll just put you {amount} blocks in the air.",
                $"The view up there {amount} blocks in the air is great, isn't it {Player.Name}?"
            };

            var randomMessage = messages.RandomElement();

            async Task game()
            {
                await Api.Say(randomMessage);
                await Api.TeleportEntityLocal(Player.Name, 0, amount, 0);
            }

            return game;
        }
    }
}
