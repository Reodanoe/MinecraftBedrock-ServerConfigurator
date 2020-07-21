using BedrockServerConfigurator.Library.Commands;
using BedrockServerConfigurator.Library.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BedrockServerConfigurator.Library
{
    /// <summary>
    /// This class creates random minigames for a player to deal with
    /// </summary>
    public class MiniGame
    {
        private readonly ServerPlayer player;
        private readonly Api api;

        private Action RunMinigame;
        private Timer runningTimer;

        public MiniGame(ServerPlayer player, Api api)
        {
            this.player = player;
            this.api = api;

            Start();
        }

        private void Start()
        {
            var (delay, runMinigame) = RandomMinigame();

            RunMinigame = runMinigame;
            runningTimer = new Timer(delay.TotalMilliseconds);

            runningTimer.Elapsed += Timer_Elapsed;
            runningTimer.AutoReset = true;
            runningTimer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (player.IsOnline)
            {
                RunMinigame();
            }

            var (delay, runMinigame) = RandomMinigame();

            runningTimer.Interval = delay.TotalMilliseconds;
            RunMinigame = runMinigame;
        }

        private (TimeSpan delay, Action game) RandomMinigame()
        {
            var allMinigames = new List<(TimeSpan, Action)>
            {
                TeleportUp(TimeSpan.FromSeconds(20), TimeSpan.FromMinutes(2), 5, 20),
                SpawnRandomMobs(TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2), 3, 7),
                BadEffect(TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2))
            };

            return allMinigames.RandomElement();
        }

        private (TimeSpan delay, Action game) TeleportUp(TimeSpan minDelay, TimeSpan maxDelay, int min, int max)
        {
            var toWait = RandomDelay(minDelay, maxDelay);
            var amount = Utilities.RandomGenerator.Next(min, max + 1);

            return (toWait, () =>
            {
                SayRandomMessage($"Hey {player.Name} I hope you know how to fly, it's just {amount} blocks in the air.",
                                 $"{player.Name} can you take a better look at the weather up there, {amount} blocks high?",
                                 $"Don't mind me {player.Name}, I'll just put you {amount} blocks in the air.",
                                 $"The view up there {amount} blocks in the air is great, isn't it {player.Name}?");

                api.TeleportEntityLocal(player.ServerId, player.Name, 0, amount, 0);
            });
        }

        private (TimeSpan delay, Action game) SpawnRandomMobs(TimeSpan minDelay, TimeSpan maxDelay, int min, int max)
        {
            var toWait = RandomDelay(minDelay, maxDelay);
            var amount = Utilities.RandomGenerator.Next(min, max + 1);
            var mob = RandomMob();

            return (toWait, () =>
            {
                SayRandomMessage($"Hey {player.Name}, I hope you like {mob}'s. And I hope you like {amount} of them.",
                                 $"{player.Name}, go ahead and hug all those {amount} {mob}'s.",
                                 $"Dear {player.Name}, enjoy some company with {amount} of your new {mob} friends.",
                                 $"Knock knock {player.Name}. Who's there you're asking? Oh just {amount} {mob}'s.");

                api.SpawnMobsOnAPlayer(player.ServerId, player.Name, mob, amount);
            });
        }

        private (TimeSpan delay, Action game) BadEffect(TimeSpan minDelay, TimeSpan maxDelay)
        {
            var toWait = RandomDelay(minDelay, maxDelay);
            var badEffect = BadEffectWithMessage();

            return (toWait, () =>
            {
                SayRandomMessage(badEffect.Value);

                api.AddEffect(player.ServerId, player.Name, badEffect.Key, 15, 1);
            });
        }

        private string RandomMob()
        {
            string[] hostileMobs =
            {
                "creeper",
                "skeleton",
                "zombie",
                "witch",
                "blaze",
                "slime"
            };

            return hostileMobs.RandomElement();
        }

        private KeyValuePair<MinecraftEffect, string[]> BadEffectWithMessage()
        {
            var badEffects = new Dictionary<MinecraftEffect, string[]>
            {
                [MinecraftEffect.Blindness] = new[]
                {
                    $"Hey {player.Name}, now you see, now you don't."
                },

                [MinecraftEffect.Hunger] = new[]
                {
                    $"Hmm... {player.Name}, you look a bit hungry, should probably eat something."
                },

                [MinecraftEffect.Nausea] = new[]
                {
                    $"It's nausea time {player.Name}. You spin my head right round, right round."
                },

                [MinecraftEffect.Slowness] = new[]
                {
                    $"Uh.. {player.Name}, you know there's a sprint button, right?"
                },

                [MinecraftEffect.Poison] = new[]
                {
                    $"Oof ouchie {player.Name}, oof ouch oof uf ouch. Don't worry it won't kill you, but something else probably will."
                }
            };

            return badEffects.RandomDictionaryElement();
        }

        private void SayRandomMessage(params string[] messages)
        {
            api.Say(player.ServerId, messages.RandomElement());
        }

        private TimeSpan RandomDelay(TimeSpan minDelay, TimeSpan maxDelay)
        {
            var timeBetween = maxDelay - minDelay;
            var randomDelayBeforeAddition = Utilities.RandomGenerator.Next((int)timeBetween.TotalMilliseconds);
            var randomDelayMiliseconds = minDelay.TotalMilliseconds + randomDelayBeforeAddition;

            return TimeSpan.FromMilliseconds(randomDelayMiliseconds);
        }
    }
}
