using System;
using System.Collections.Generic;
using System.Linq;

namespace BedrockServerConfigurator.Library.Entities
{
    public class Entity : IEntity
    {
        public MinecraftEntityType EntityType { get; }
        private readonly string _playerName;

        public string Name => ToString();

        public static Dictionary<MinecraftEntityType, string> EntityName => new Dictionary<MinecraftEntityType, string>
        {
            [MinecraftEntityType.All_Players] = "@a",
            [MinecraftEntityType.All_Entities] = "@e",
            [MinecraftEntityType.Closest_Player] = "@p",
            [MinecraftEntityType.Random_Player] = "@r",
            [MinecraftEntityType.Yourself] = "@s"
        };

        public Entity(MinecraftEntityType entity)
        {
            if (entity == MinecraftEntityType.Player)
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
                EntityType = MinecraftEntityType.Player;
            }
        }

        public Entity(Player player)
        {
            _playerName = player.Username;
            EntityType = MinecraftEntityType.Player;
        }

        private string EntityTag(MinecraftEntityType entity) => entity switch
        {
            MinecraftEntityType.Player => _playerName,
            _ => EntityName[entity]
        };

        public override string ToString()
        {
            return EntityTag(EntityType);
        }
    }
}
