namespace BedrockServerConfigurator.Library.Models
{
    public class PlayerEntity : IEntity
    {
        public static string All_Players => "@a";
        public static string All_Entities => "@e";
        public static string Closest_Player => "@p";
        public static string Random_Player => "@r";
        public static string Yourself => "@s";

        public Entity EntityType { get; }

        public string Name => ToString();

        public PlayerEntity(Entity entity)
        {
            EntityType = entity;
        } 

        public static string EntityTag(Entity entity) => entity switch
        {
            Entity.All_Players => All_Players,
            Entity.All_Entities => All_Entities,
            Entity.Closest_Player => Closest_Player,
            Entity.Random_Player => Random_Player,
            Entity.Yourself => Yourself,
            _ => null
        };

        public override string ToString()
        {
            return EntityTag(EntityType);
        }
    }
}
