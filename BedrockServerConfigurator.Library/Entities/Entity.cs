using System;
using System.Collections.Generic;
using System.Linq;

namespace BedrockServerConfigurator.Library.Entities
{
    public class Entity : IEntity
    {
        public EntityType EntityType { get; }
        private readonly string _playerName;

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

        /// <summary>
        /// Enter player's name, entering @a, @e, or other @'s will result in setting this entity to its correct type
        /// </summary>
        /// <param name="entity">Player's name or @a, @e, ...</param>
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
                _playerName = entity;
                EntityType = EntityType.Player;
            }
        }

        private string EntityTag(EntityType entity) => entity switch
        {
            EntityType.Player => _playerName,
            _ => EntityName[entity]
        };

        public override string ToString()
        {
            return EntityTag(EntityType);
        }
    }
}
