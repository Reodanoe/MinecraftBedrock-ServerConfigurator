using BedrockServerConfigurator.Library.Commands;
using BedrockServerConfigurator.Library.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace BedrockServerConfigurator.Library.Minigame
{
    public abstract class Microgame
    {
        public TimeSpan MinDelay { get; }
        public TimeSpan MaxDelay { get; }

        protected TimeSpan RandomDelay => Utilities.RandomDelay(MinDelay, MaxDelay);

        public ServerPlayer Player { get; }
        public Api Api { get; }

        public event EventHandler<MicrogameEventArgs> OnCreatedMicrogame;
        public event EventHandler OnMicrogameStarted;

        private Timer timer;
        private Action microgameToRun;

        public Microgame(TimeSpan minDelay, TimeSpan maxDelay, ServerPlayer player, Api api)
        {
            MinDelay = minDelay;
            MaxDelay = maxDelay;
            Player = player;
            Api = api;
        }

        public void StartMicrogame()
        {
            var (delay, game) = DelayAndMicrogame();
            microgameToRun = game;

            OnMicrogameStarted?.Invoke(this, null);

            timer = new Timer(delay.TotalMilliseconds);
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = false;
            timer.Start();
        }

        public void StopMicrogame()
        {
            timer.Close();
            timer = null;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Player.IsOnline)
            {
                microgameToRun();
            }

            StartMicrogame();
        }

        /// <summary>
        /// Returns a TimeSpan and an Action. TimeSpan says when the Action (the microgame) should run.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="api"></param>
        /// <returns></returns>
        public abstract (TimeSpan, Action) DelayAndMicrogame();

        protected void OnMicrogameCreated(MicrogameEventArgs args)
        {
            OnCreatedMicrogame?.Invoke(this, args);
        }
    }
}
