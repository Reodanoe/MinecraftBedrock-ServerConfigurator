using System;

namespace BedrockServerConfigurator.Library.Entities
{
    public class Entity : IEntity
    {
        public static string All_Players => "@a";
        public static string All_Entities => "@e";
        public static string Closest_Player => "@p";
        public static string Random_Player => "@r";
        public static string Yourself => "@s";

        public EntityType EntityType { get; }
        public string PlayerName { get; }

        public string Name => ToString();        

        public Entity(EntityType entity)
        {
            if(entity == EntityType.Player)
            {
                throw new Exception("To use EntityType.Player use other constructor for playerName");
            }

            EntityType = entity;
        }

        public Entity(string entity)
        {
            // if is for example @a set it to all players

            PlayerName = entity;
            EntityType = EntityType.Player;
        }

        private string EntityTag(EntityType entity) => entity switch
        {
            EntityType.All_Players => All_Players,
            EntityType.All_Entities => All_Entities,
            EntityType.Closest_Player => Closest_Player,
            EntityType.Random_Player => Random_Player,
            EntityType.Yourself => Yourself,
            EntityType.Player => PlayerName,
            _ => null
        };

        public override string ToString()
        {
            return EntityTag(EntityType);
        }
    }
}
