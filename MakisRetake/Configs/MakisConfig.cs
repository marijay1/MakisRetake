using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MakisRetake.Configs;
public class MakisConfig : BasePluginConfig {
    [JsonPropertyName("Max Players")] public int myMaxPlayers { get; set; } = 9;
    [JsonPropertyName("Terrorist Ratio")] public float myTerroristRatio { get; set; } = 0.45f;
    [JsonPropertyName("Consecutive Rounds to Scramble")] public int myConsecutiveRoundsToScramble { get; set; } = 5;
    //What is this setting again?
    //[JsonPropertyName("Switch Teams on Win")] public bool mySwitchTeamsOnWin { get; set; } = true;
}

