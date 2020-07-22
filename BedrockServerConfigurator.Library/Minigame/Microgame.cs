using BedrockServerConfigurator.Library.Commands;
using BedrockServerConfigurator.Library.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BedrockServerConfigurator.Library.Minigame
{
    public abstract class Microgame
    {
        public TimeSpan MinDelay { get; }
        public TimeSpan MaxDelay { get; }

        public event EventHandler<MicrogameEventArgs> OnCreatedMicrogame;

        public Microgame(TimeSpan minDelay, TimeSpan maxDelay)
        {
            MinDelay = minDelay;
            MaxDelay = maxDelay;
        }

        public abstract (TimeSpan, Action) DelayAndMicrogame(ServerPlayer player, Api api);

        protected virtual void OnMicrogameCreated(MicrogameEventArgs args)
        {
            OnCreatedMicrogame?.Invoke(this, args);
        }
    }
}
