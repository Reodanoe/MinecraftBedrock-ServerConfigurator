﻿using BedrockServerConfigurator.Library.Commands;
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
        protected ServerPlayer Player { get; }
        protected Api Api { get; }

        protected TimeSpan RandomDelay => Utilities.RandomDelay(MinDelay, MaxDelay);

        /// <summary>
        /// Runs inside `DelayAndMicrogame()` use `MicrogameCreated()` to call it in derived types
        /// </summary>
        public event EventHandler<MicrogameEventArgs> OnMicrogameCreated;

        /// <summary>
        /// Runs after timer has started counting down until when microgame runs
        /// </summary>
        public event EventHandler OnMicrogameCountdownStarted;

        /// <summary>
        /// Runs right after microgame finished
        /// </summary>
        public event Action<Microgame> OnMicrogameEnded;

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
            if(timer == null)
            {
                var (delay, game) = DelayAndMicrogame();
                microgameToRun = game;

                timer = new Timer(delay.TotalMilliseconds);
                timer.Elapsed += Timer_Elapsed;
                timer.AutoReset = false;
                timer.Start();

                OnMicrogameCountdownStarted?.Invoke(this, null);
            }
        }

        public void StopMicrogame()
        {
            if(timer != null)
            {
                timer.Elapsed -= Timer_Elapsed;
                timer.Close();
                timer = null;
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Player.IsOnline && Api.IsServerRunning(Player.ServerId))
            {
                microgameToRun();
                OnMicrogameEnded?.Invoke(this);
            }

            StopMicrogame();
            StartMicrogame();            
        }

        /// <summary>
        /// Returns a TimeSpan and an Action. TimeSpan says when the Action (the microgame) should run.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="api"></param>
        /// <returns></returns>
        public abstract (TimeSpan, Action) DelayAndMicrogame();

        protected void MicrogameCreated(MicrogameEventArgs args)
        {
            // this is invoked twice in non parallel microgames, a bug
            OnMicrogameCreated?.Invoke(this, args);
        }
    }
}
