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
        public ServerApi Api { get; }

        public TimeSpan RandomDelay => Utilities.RandomTimeSpan(MinDelay, MaxDelay);

        /// <summary>
        /// When microgame runs
        /// </summary>
        public DateTime RunsIn { get; private set; }

        /// <summary>
        /// Runs inside GetGame(), use MicrogameCreated() to call it in derived types
        /// </summary>
        public event EventHandler<MicrogameEventArgs> OnMicrogameCreated;

        /// <summary>
        /// Runs right after microgame finished
        /// </summary>
        public event EventHandler<MicrogameEventArgs> OnMicrogameEnded;

        private MicrogameEventArgs microgameEventArgs;

        private Timer timer;
        private Func<Task> microgameToRun;
        private bool microgameRepeats;

        public Microgame(TimeSpan minDelay, TimeSpan maxDelay, ServerPlayer player, ServerApi api)
        {
            if (maxDelay < minDelay)
            {
                throw new ArgumentException("maxDelay has to be bigger than minDelay", "maxDelay");
            }

            MinDelay = minDelay;
            MaxDelay = maxDelay;
            Player = player;
            Api = api;
        }

        public void StartMicrogame(bool repeats)
        {
            if(timer == null)
            {
                microgameRepeats = repeats;

                var delay = RandomDelay;
                RunsIn = DateTime.Now.Add(delay);

                microgameToRun = GetGame();

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

        private async void RunMicrogame(object sender, ElapsedEventArgs e)
        {
            if (Player.IsOnline && Api.IsServerRunning())
            {
                await microgameToRun();
            }

            StopMicrogame();

            OnMicrogameEnded?.Invoke(this, microgameEventArgs);            

            if (microgameRepeats)
            {
                StartMicrogame(true);
            }
        }

        /// <summary>
        /// Returns a Func<Task> where Task is the game that's gonna be ran
        /// </summary>
        /// <param name="player"></param>
        /// <param name="api"></param>
        /// <returns></returns>
        public abstract Func<Task> GetGame();

        /// <summary>
        /// Call this to log some information about new created microgame
        /// </summary>
        /// <param name="args"></param>
        protected void MicrogameCreated(MicrogameEventArgs args)
        {
            microgameEventArgs = args;
            OnMicrogameCreated?.Invoke(this, args);
        }
    }
}
