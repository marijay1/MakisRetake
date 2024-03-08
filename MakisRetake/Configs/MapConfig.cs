using MakisRetake.Configs.JsonProviders;
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
        load();
    }

    //TODO require user to be in edit mode to add/remove spawns
    //it will show spawns
    //it will move any non-admins to spectator

    public String getMapName() {
        return theMapName;
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
        } catch (JsonException ex) {
            throw new Exception("Error deserializing JSON data: " + ex.Message);
        } catch (IOException ex) {
            throw new Exception("Error reading or writing JSON file: " + ex.Message);
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