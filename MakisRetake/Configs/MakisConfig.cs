using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace MakisRetake.Configs;

public class MakisConfig : BasePluginConfig {
    [JsonPropertyName("Retakes Config")] public RetakesConfig theRetakesConfig { get; set; } = new RetakesConfig();
}

public class RetakesConfig {
    [JsonPropertyName("Max Players")] public int theMaxPlayers { get; set; } = 9;
    [JsonPropertyName("Terrorist Ratio")] public float theTerroristRatio { get; set; } = 0.45f;
    [JsonPropertyName("Consecutive Rounds to Scramble")] public int theConsecutiveRoundsToScramble { get; set; } = 5;
}