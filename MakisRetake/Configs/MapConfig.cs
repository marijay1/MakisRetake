using CounterStrikeSharp.API.Core;
using MakisRetake.Configs.JsonProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MakisRetake.Configs;
public class MapConfig {

    private readonly string theMapName;
    private readonly string theMapSpawnDirectory;
    private readonly string theMapSpawnPath;

    private HashSet<MapSpawn> theMapSpawns;

    public MapConfig(String aModuleDirectory, String aMapName) {
        theMapName = aMapName;
        theMapSpawnDirectory = Path.Combine(aModuleDirectory, aMapName);
        theMapSpawnPath = Path.Combine(theMapSpawnDirectory, $"{theMapName}.json");
        theMapSpawns = new HashSet<MapSpawn>();
    }

    //TODO require user to be in edit mode to add/remove spawns
    //it will show spawns
    //it will move any non-admins to spectator

    public String getMapName() {
        return theMapName;
    }

    public HashSet<MapSpawn> getMapSpawns() {
        return theMapSpawns;
    }

    public void load() {
        try {
            if (!File.Exists(theMapSpawnPath)) {
                throw new FileNotFoundException();
            }

            string myJsonData = File.ReadAllText(theMapSpawnPath);
            JsonSerializerOptions myOptions = new JsonSerializerOptions();
            myOptions.Converters.Add(new VectorProvider());
            myOptions.Converters.Add(new QAngleProvider());

            theMapSpawns = JsonSerializer.Deserialize<HashSet<MapSpawn>>(myJsonData, myOptions);

            if (theMapSpawns == null || theMapSpawns.Count < 0) {
                throw new Exception("No Spawns found in config");
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

