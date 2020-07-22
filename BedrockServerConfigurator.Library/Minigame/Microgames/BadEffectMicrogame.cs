using BedrockServerConfigurator.Library.Commands;
using BedrockServerConfigurator.Library.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BedrockServerConfigurator.Library.Minigame.Microgames
{
    public class BadEffectMicrogame : Microgame
    {
        public BadEffectMicrogame(TimeSpan minDelay, TimeSpan maxDelay) : 
            base(minDelay, maxDelay)
        {
        }

        public override (TimeSpan, Action) DelayAndMicrogame(ServerPlayer player, Api api)
        {
            var toWait = Utilities.RandomDelay(MinDelay, MaxDelay);
            var (effect, messages) = BadEffectWithMessage(player.Name);

            OnMicrogameCreated(new MicrogameEventArgs(player, "Bad effect", toWait, $"Effect: {effect}"));

            void game()
            {
                api.Say(player.ServerId, messages.RandomElement());
                api.AddEffect(player.ServerId, player.Name, effect, 15, 1);
            }

            return (toWait, game);
        }

        private KeyValuePair<MinecraftEffect, string[]> BadEffectWithMessage(string name)
        {
            var badEffects = new Dictionary<MinecraftEffect, string[]>
            {
                [MinecraftEffect.Blindness] = new[]
                {
                    $"Hey {name}, now you see, now you don't."
                },

                [MinecraftEffect.Hunger] = new[]
                {
                    $"Hmm... {name}, you look a bit hungry, should probably eat something."
                },

                [MinecraftEffect.Nausea] = new[]
                {
                    $"It's nausea time {name}. You spin my head right round, right round."
                },

                [MinecraftEffect.Slowness] = new[]
                {
                    $"Uh.. {name}, you know there's a sprint button, right?"
                },

                [MinecraftEffect.Poison] = new[]
                {
                    $"Oof ouchie {name}, oof ouch oof uf ouch. Don't worry it won't kill you, but something else probably will."
                }
            };

            return badEffects.RandomDictionaryElement();
        }
    }
}
