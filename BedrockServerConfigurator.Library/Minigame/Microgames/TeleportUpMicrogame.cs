using BedrockServerConfigurator.Library.Commands;
using BedrockServerConfigurator.Library.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BedrockServerConfigurator.Library.Minigame.Microgames
{
    public class TeleportUpMicrogame : Microgame
    {
        public int MinBlocks { get; }
        public int MaxBlocks { get; }

        public TeleportUpMicrogame(TimeSpan minDelay, TimeSpan maxDelay, int minBlocks, int maxBlocks) : 
            base(minDelay, maxDelay)
        {
            MinBlocks = minBlocks;
            MaxBlocks = maxBlocks;
        }

        public override (TimeSpan, Action) DelayAndMicrogame(ServerPlayer player, Api api)
        {
            var delay = Utilities.RandomDelay(MinDelay, MaxDelay);
            var amount = Utilities.RandomGenerator.Next(MinBlocks, MaxBlocks + 1);

            OnMicrogameCreated(new MicrogameEventArgs(player, "Teleport Up", delay, $"Blocks: {amount}"));

            string[] messages =
            {
                $"Hey {player.Name} I hope you know how to fly, it's just {amount} blocks in the air.",
                $"{player.Name} can you take a better look at the weather up there, {amount} blocks high?",
                $"Don't mind me {player.Name}, I'll just put you {amount} blocks in the air.",
                $"The view up there {amount} blocks in the air is great, isn't it {player.Name}?"
            };

            var randomMessage = messages.RandomElement();

            void game()
            {
                api.Say(player.ServerId, randomMessage);
                api.TeleportEntityLocal(player.ServerId, player.Name, 0, amount, 0);
            }

            return (delay, game);
        }
    }
}
