using BedrockServerConfigurator.Library.Commands;
using BedrockServerConfigurator.Library.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BedrockServerConfigurator.Library.Minigame.Microgames
{
    public class SpawnRandomMobsMicrogame : Microgame
    {
        public int MinMobs { get; }
        public int MaxMobs { get; }

        readonly string[] hostileMobs =
        {
            "creeper",
            "skeleton",
            "zombie",
            "witch",
            "blaze"
        };

        public SpawnRandomMobsMicrogame(TimeSpan minDelay, TimeSpan maxDelay, int minMobs, int maxMobs) :
            base(minDelay, maxDelay)
        {
            MinMobs = minMobs;
            MaxMobs = maxMobs;
        }

        public override (TimeSpan, Action) DelayAndMicrogame(ServerPlayer player, Api api)
        {
            var delay = Utilities.RandomDelay(MinDelay, MaxDelay);

            var amount = Utilities.RandomGenerator.Next(MinMobs, MaxMobs + 1);
            var mob = hostileMobs.RandomElement();

            OnMicrogameCreated(new MicrogameEventArgs(player, "Spawn random mobs", delay, $"Mobs: {mob}, Amount: {amount}"));

            string[] messages =
            {
                $"Hey {player.Name}, I hope you like {mob}'s. And I hope you like {amount} of them.",
                $"{player.Name}, go ahead and hug all those {amount} {mob}'s.",
                $"Dear {player.Name}, enjoy some company with {amount} of your new {mob} friends.",
                $"Knock knock {player.Name}. Who's there you're asking? Oh just {amount} {mob}'s."
            };

            var randomMessage = messages.RandomElement();

            void game()
            {
                api.Say(player.ServerId, randomMessage);
                api.SpawnMobsOnAPlayer(player.ServerId, player.Name, mob, amount);
            }

            return (delay, game);
        }
    }
}
