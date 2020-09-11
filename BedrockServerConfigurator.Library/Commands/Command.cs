namespace BedrockServerConfigurator.Library.Commands
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

        public static Command operator +(Command first, Command second)
        {
            return new Command(first.MinecraftCommand + second.MinecraftCommand);
        }
    }
}
