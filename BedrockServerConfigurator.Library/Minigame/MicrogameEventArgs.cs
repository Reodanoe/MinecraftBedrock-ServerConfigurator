using BedrockServerConfigurator.Library.Entities;
using System;

namespace BedrockServerConfigurator.Library.Minigame
{
    public class MicrogameEventArgs : EventArgs
    {
        public DateTime CreatedOn { get; }
        public ServerPlayer Player { get; }
        public string MicroGameName { get; }
        public TimeSpan Delay { get; }
        public string AdditionaInfo { get; }

        // I need to work on this
        public MicrogameEventArgs(ServerPlayer player, string microGameName, TimeSpan delay, string additionalInfo = "")
        {
            CreatedOn = DateTime.Now;

            Player = player;
            MicroGameName = microGameName;
            Delay = delay;
            AdditionaInfo = additionalInfo;
        }

        public override string ToString()
        {
            return $"[{MicroGameName}] - [{CreatedOn}] - Delay: {Delay} - Runs on player: {Player.Name} - Additional info: \"{AdditionaInfo}\"";
        }
    }
}