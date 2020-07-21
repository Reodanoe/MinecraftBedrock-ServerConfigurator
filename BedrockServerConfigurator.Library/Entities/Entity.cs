using System;
using System.Collections.Generic;
using System.Linq;

namespace BedrockServerConfigurator.Library.Entities
{
    public class Entity : IEntity
    {
        public EntityType EntityType { get; }
        public string PlayerName { get; }

        public string Name => ToString();

        public static Dictionary<EntityType, string> EntityName => new Dictionary<EntityType, string>
        {
            [EntityType.All_Players] = "@a",
            [EntityType.All_Entities] = "@e",
            [EntityType.Closest_Player] = "@p",
            [EntityType.Random_Player] = "@r",
            [EntityType.Yourself] = "@s"
        };

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
            var entityLower = entity.ToLower();

            if (EntityName.ContainsValue(entityLower))
            {
                var key = EntityName.First(x => x.Value == entityLower).Key;
                EntityType = key;
            }
            else
            {
                PlayerName = entity;
                EntityType = EntityType.Player;
            }
        }

        private string EntityTag(EntityType entity) => entity switch
        {
            EntityType.Player => PlayerName,
            _ => EntityName[entity]
        };

        public override string ToString()
        {
            return EntityTag(EntityType);
        }
    }
}
