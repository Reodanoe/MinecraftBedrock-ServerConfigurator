using System;
using BedrockServerConfigurator.Library.Entities;

namespace BedrockServerConfigurator.Library.Minigame
{
    public class MicrogameEventArgs : EventArgs
    {
        public DateTime CreatedOn { get; }

        public Microgame Sender { get; }
        public string AdditionalInfo { get; }

        public TimeSpan TimeLeft => Sender.RunsIn.Subtract(DateTime.Now);

        public MicrogameEventArgs(Microgame sender, string additionalInfo = "")
        {
            CreatedOn = DateTime.Now;

            Sender = sender;
            AdditionalInfo = additionalInfo;
        }

        public override string ToString()
        {
            return $"[{Sender.GetType().Name}] - ([{CreatedOn}] + Delay: {TimeLeft} = [{Sender.RunsIn}]) - {Sender.Player.Username} - Additional info: \"{AdditionalInfo}\"";
        }
    }
}