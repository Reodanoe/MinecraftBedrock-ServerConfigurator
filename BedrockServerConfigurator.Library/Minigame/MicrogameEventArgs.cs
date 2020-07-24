using System;
using BedrockServerConfigurator.Library.Entities;

namespace BedrockServerConfigurator.Library.Minigame
{
    public class MicrogameEventArgs : EventArgs
    {
        public DateTime CreatedOn { get; }
        public ServerPlayer Player { get; }
        public string MicrogameName { get; }
        public TimeSpan Delay { get; }
        public string AdditionalInfo { get; }

        public DateTime RunsOn => CreatedOn.Add(Delay);

        public MicrogameEventArgs(ServerPlayer player, string microgameName, TimeSpan delay, string additionalInfo = "")
        {
            CreatedOn = DateTime.Now;

            Player = player;
            MicrogameName = microgameName;
            Delay = delay;
            AdditionalInfo = additionalInfo;
        }

        public override string ToString()
        {
            return $"[{MicrogameName}] - ([{CreatedOn}] + Delay: {Delay} = [{RunsOn}]) - {Player.Name} - Additional info: \"{AdditionalInfo}\"";
        }
    }
}