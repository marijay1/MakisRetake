using CounterStrikeSharp.API.Modules.Utils;
using MakisRetake.Configs.JsonProviders;
using MakisRetake.Enums;
using System.Text.Json.Serialization;

namespace MakisRetake.Configs;
public class MapSpawn {

    [JsonConverter(typeof(VectorProvider))]
    public Vector theVector { get; }
    [JsonConverter(typeof(QAngleProvider))]
    public QAngle theQAngle { get; }
    public CsTeam theTeam { get; }
    public Bombsite theBombsite { get; }
    public bool theCanBePlanter { get; }
    [JsonIgnore]
    public bool theIsInUse { get; set; } = false;

    public MapSpawn(Vector aVector, QAngle aQAngle, CsTeam aTeam, Bombsite aBombsite, bool aCanBePlanter) {
        theVector = aVector;
        theQAngle = aQAngle;
        theTeam = aTeam;
        theBombsite = aBombsite;
        theCanBePlanter = aCanBePlanter;
    }

}

