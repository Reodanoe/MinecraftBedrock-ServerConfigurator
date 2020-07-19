namespace BedrockServerConfigurator.Library.Models
{
    public class Command
    {
        public string MinecraftCommand { get; }

        internal Command(string command)
        {
            MinecraftCommand = command;
        }

        public override string ToString()
        {
            return MinecraftCommand;
        }
    }
}
