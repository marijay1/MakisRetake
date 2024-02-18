using CounterStrikeSharp.API.Modules.Utils;
using MakisRetake.Configs.JsonProviders;
using MakisRetake.Enums;
using System.Text.Json;

namespace MakisRetake.Configs;

public class MapConfig {
    private readonly string theMapName;
    private readonly string theMapSpawnDirectory;
    private readonly string theMapSpawnPath;

    private List<MapSpawn> theMapSpawns;

    public MapConfig(String aModuleDirectory, String aMapName) {
        theMapName = aMapName;

        theMapSpawnDirectory = Path.Combine(Path.GetDirectoryName(aModuleDirectory), "..", "configs", "plugins", "MakisRetake", "MapSpawns");
        theMapSpawnPath = Path.Combine(theMapSpawnDirectory, $"{theMapName}.json");
        theMapSpawns = new List<MapSpawn>();
    }

    //TODO require user to be in edit mode to add/remove spawns
    //it will show spawns
    //it will move any non-admins to spectator

    public String getMapName() {
        return theMapName;
    }

    public MapSpawn getPlanterSpawn(Bombsite aBombsite) {
        List<MapSpawn> myPlanterSpawns = theMapSpawns.Where(aSpawn => aSpawn.theCanBePlanter && aSpawn.theBombsite.Equals(aBombsite)).ToList();

        if (myPlanterSpawns.Count == 0) {
            //put game in edit mode
            throw new Exception("No planter spawns!!!");
        }

        return myPlanterSpawns[new Random().Next(0, myPlanterSpawns.Count)];
    }

    public MapSpawn getRandomNonPlanterSpawn(Bombsite aBombsite, CsTeam aTeam) {
        List<MapSpawn> myNonPlanterSpawns = theMapSpawns.Where(aSpawn => !aSpawn.theCanBePlanter && aSpawn.theBombsite.Equals(aBombsite) && aSpawn.theTeam.Equals(aTeam)).ToList();

        if (myNonPlanterSpawns.Count == 0) {
            //put game in edit mode
            throw new Exception("No planter spawns!!!");
        }

        MapSpawn myMapSpawn = myNonPlanterSpawns.ElementAtOrDefault(new Random().Next(myNonPlanterSpawns.Count))!;
        while (myMapSpawn.theIsInUse) {
            myMapSpawn = myNonPlanterSpawns.ElementAtOrDefault(new Random().Next(myNonPlanterSpawns.Count))!;
        }

        return myMapSpawn;
    }

    public List<MapSpawn> getMapSpawns() {
        return theMapSpawns;
    }

    public void load() {
        try {
            if (File.Exists(theMapSpawnPath)) {
                string myJsonData = File.ReadAllText(theMapSpawnPath);
                JsonSerializerOptions myOptions = new JsonSerializerOptions();
                myOptions.Converters.Add(new VectorProvider());
                myOptions.Converters.Add(new QAngleProvider());

                theMapSpawns = JsonSerializer.Deserialize<List<MapSpawn>>(myJsonData, myOptions);

                if (theMapSpawns == null || theMapSpawns.Count < 0) {
                    throw new Exception("No Spawns found in config");
                }
            } else {
                theMapSpawns = new List<MapSpawn>();
                save();
            }
        } catch (Exception) {
            throw new Exception();
        }
    }

    public void save() {
        JsonSerializerOptions myOptions = new JsonSerializerOptions();
        myOptions.Converters.Add(new VectorProvider());
        myOptions.Converters.Add(new QAngleProvider());
        myOptions.WriteIndented = true;

        string myJsonString = JsonSerializer.Serialize(theMapSpawns, myOptions);

        try {
            if (!Directory.Exists(theMapSpawnDirectory)) {
                Directory.CreateDirectory(theMapSpawnDirectory);
            }

            File.WriteAllText(theMapSpawnPath, myJsonString);
        } catch (IOException) { }
    }

    public void addSpawn(MapSpawn aSpawn) {
        theMapSpawns.Add(aSpawn);
        save();
    }

    public void removeSpawn(MapSpawn aSpawn) {
        theMapSpawns.Remove(aSpawn);
        save();
    }
}