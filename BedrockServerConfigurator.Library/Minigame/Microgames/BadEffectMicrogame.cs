using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BedrockServerConfigurator.Library.Entities;

namespace BedrockServerConfigurator.Library.Minigame.Microgames
{
    public class BadEffectMicrogame : Microgame
    {
        public BadEffectMicrogame(TimeSpan minDelay, TimeSpan maxDelay, ServerPlayer player) :
            base(minDelay, maxDelay, player)
        {
        }

        public override Func<Task> GetGame()
        {
            var (effect, messages) = BadEffectWithMessage(Player.Name);

            MicrogameCreated(new MicrogameEventArgs(this, $"Effect: {effect}"));

            async Task game()
            {
                await Api.Say(messages.RandomElement());
                await Api.AddEffect(Player.Name, effect, 15, 1);
            }

            return game;
        }

        private static KeyValuePair<MinecraftEffect, string[]> BadEffectWithMessage(string playerName)
        {
            var badEffects = new Dictionary<MinecraftEffect, string[]>
            {
                [MinecraftEffect.Blindness] = new[]
                {
                    $"Hey {playerName}, now you see, now you don't."
                },

                [MinecraftEffect.Hunger] = new[]
                {
                    $"Hmm... {playerName}, you look a bit hungry, should probably eat something."
                },

                [MinecraftEffect.Nausea] = new[]
                {
                    $"It's nausea time {playerName}. You spin my head right round, right round."
                },

                [MinecraftEffect.Slowness] = new[]
                {
                    $"Uh.. {playerName}, you know there's a sprint button, right?"
                },

                [MinecraftEffect.Poison] = new[]
                {
                    $"Oof ouchie {playerName}, oof ouch oof uf ouch. Don't worry it won't kill you, but something else probably will."
                }
            };

            return badEffects.RandomElement();
        }
    }
}
