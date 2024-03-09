using CounterStrikeSharp.API.Modules.Utils;
using MakisRetake.Configs.JsonProviders;
using MakisRetake.Enums;
using System.Text.Json.Serialization;

namespace MakisRetake.Configs;

public class MapSpawn {

    [JsonConverter(typeof(VectorProvider))]
    public Vector theVector { get; set; }

    [JsonConverter(typeof(QAngleProvider))]
    public QAngle theQAngle { get; set; }

    public CsTeam theTeam { get; set; }
    public Bombsite theBombsite { get; set; }
    public bool theCanBePlanter { get; set; }

    public MapSpawn() {
    }

    public MapSpawn(Vector aVector, QAngle aQAngle, CsTeam aTeam, Bombsite aBombsite, bool aCanBePlanter) {
        theVector = new Vector(aVector.X, aVector.Y, aVector.Z);
        theQAngle = new QAngle(aQAngle.X, aQAngle.Y, aQAngle.Z);
        theTeam = aTeam;
        theBombsite = aBombsite;
        theCanBePlanter = aCanBePlanter;
    }
}