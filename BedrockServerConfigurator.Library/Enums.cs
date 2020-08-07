using System;

namespace BedrockServerConfigurator.Library
{
    public enum MinecraftEntityType
    {
        All_Players,
        All_Entities,
        Closest_Player,
        Random_Player,
        Yourself,
        Player
    }

    public enum MinecraftTime
    {
        Sunrise,
        Day,
        Noon,
        Sunset,
        Night,
        Midnight
    }

    [Flags]
    public enum MinecraftColor : int
    {
       Black = 0x0,
       DarkBlue = 0x1,
       DarkGreen = 0x2,
       DarkAqua = 0x3,
       DarkRed = 0x4,
       DarkPurple = 0x5,
       Gold = 0x6,
       Gray = 0x7,
       DarkGray = 0x8,
       Blue = 0x9,
       Green = 0xA,
       Aqua = 0xB,
       Red = 0xC,
       LightPurple = 0xD,
       Yellow = 0xE,
       White = 0xF
    }

    public enum MinecraftEffect
    {
        Absorption,
        Bad_omen,
        Blindness,
        Clear,
        Conduit_power,
        Empty,
        Fatal_poison,
        Fire_resistance,
        Haste,
        Health_boost,
        Hunger,
        Instant_damage,
        Instant_health,
        Invisibility,
        Jump_boost,
        Levitation,
        Mining_fatigue,
        Nausea,
        Night_vision,
        Poison,
        Regeneration,
        Resistance,
        Saturation,
        Slow_falling,
        Slowness,
        Speed,
        Strength,
        Village_hero,
        Water_breathing,
        Weakness,
        Wither
    };

    public enum MinecraftPermission
    {
        Visitor,
        Member,
        Operator
    }

    public enum MinecraftGamemode
    {
        Adventure,
        Survival,
        Creative
    }

    public enum MinecraftDifficulty
    {
        Peaceful,
        Easy,
        Normal,
        Hard
    }
}
