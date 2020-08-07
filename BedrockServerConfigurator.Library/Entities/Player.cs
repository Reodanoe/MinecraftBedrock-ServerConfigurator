namespace BedrockServerConfigurator.Library.Entities
{
    public class Player : IEntity
    {
        public string Username { get; set; }
        public long Xuid { get; set; }

        /// <summary>
        /// Returns Username
        /// </summary>
        public string Name => Username;
    }
}
