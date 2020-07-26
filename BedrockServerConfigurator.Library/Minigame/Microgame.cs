using System;
using System.Threading.Tasks;
using System.Timers;
using BedrockServerConfigurator.Library.Commands;
using BedrockServerConfigurator.Library.Entities;

namespace BedrockServerConfigurator.Library.Minigame
{
    public abstract class Microgame
    {
        public TimeSpan MinDelay { get; }
        public TimeSpan MaxDelay { get; }
        public ServerPlayer Player { get; }
        public Api Api { get; }

        protected TimeSpan RandomDelay => Utilities.RandomDelay(MinDelay, MaxDelay);

        /// <summary>
        /// Runs inside DelayAndMicrogame(), use MicrogameCreated() to call it in derived types
        /// </summary>
        public event EventHandler<MicrogameEventArgs> OnMicrogameCreated;

        /// <summary>
        /// Runs right after microgame finished
        /// </summary>
        public event Action<Microgame> OnMicrogameEnded;

        private Timer timer;
        private Action microgameToRun;
        private bool microgameRepeats;

        public Microgame(TimeSpan minDelay, TimeSpan maxDelay, ServerPlayer player, Api api)
        {
            MinDelay = minDelay;
            MaxDelay = maxDelay;
            Player = player;
            Api = api;
        }

        public void StartMicrogame(bool repeats = true)
        {
            if(timer == null)
            {
                microgameRepeats = repeats;
                var (delay, game) = DelayAndMicrogame();
                microgameToRun = game;

                timer = new Timer(delay.TotalMilliseconds);
                timer.Elapsed += RunMicrogame;
                timer.AutoReset = false;
                timer.Start();
            }
        }

        public void StopMicrogame()
        {
            if(timer != null)
            {
                timer.Elapsed -= RunMicrogame;
                timer.Close();
                timer = null;
            }
        }

        private void RunMicrogame(object sender, ElapsedEventArgs e)
        {
            if (Player.IsOnline && Api.IsServerRunning(Player.ServerId))
            {
                microgameToRun();
            }

            StopMicrogame();

            OnMicrogameEnded?.Invoke(this);            

            if (microgameRepeats)
            {
                StartMicrogame();
            }
        }

        /// <summary>
        /// Returns a TimeSpan and an Action. TimeSpan says when the Action (the microgame) should run.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="api"></param>
        /// <returns></returns>
        public abstract (TimeSpan, Action) DelayAndMicrogame();

        /// <summary>
        /// Call this to log some information about new created microgame
        /// </summary>
        /// <param name="args"></param>
        protected void MicrogameCreated(MicrogameEventArgs args)
        {
            OnMicrogameCreated?.Invoke(this, args);
        }
    }
}
