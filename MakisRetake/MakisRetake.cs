using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Utils;
using MakisRetake.Configs;
using MakisRetake.Enums;
using MakisRetake.Managers;

namespace MakisRetake;

[MinimumApiVersion(159)]
public partial class MakisRetake : BasePlugin, IPluginConfig<MakisConfig> {

    private const string Version = "0.0.1";

    public override string ModuleName => "Maki's Retake";
    public override string ModuleVersion => Version;
    public override string ModuleAuthor => "Panduuuh";
    public override string ModuleDescription => "Main Retake plugin for Maki's";

    public static readonly string LogPrefix = $"[Maki's Retakes {Version}] ";
    public static readonly string MessagePrefix = $"[{ChatColors.LightPurple}Maki's Retakes{ChatColors.White}] ";

    private Bombsite theCurrentBombsite = Bombsite.A;
    private CCSPlayerController? thePlanter;

    public MakisConfig Config { get; set; } = null!;
    private readonly PlayerManager thePlayerManager;
    private GameManager theGameManager;
    private QueueManager theQueueManager;
    private MapConfig? theMapConfig;

    public MakisRetake() {
        thePlayerManager = new PlayerManager();
        theQueueManager = new QueueManager(thePlayerManager);
        theGameManager = new GameManager(thePlayerManager, theQueueManager);
    }

    public void OnConfigParsed(MakisConfig aMakiConfig) {
        Config = aMakiConfig;
    }

    public override void Load(bool aHotReload) {
        RegisterListener<Listeners.OnMapStart>(OnMapStart);

        if (theGameManager == null) {
            Console.WriteLine($"{LogPrefix}Game Manager is not loaded!");
        }

        if (aHotReload) {
            Server.ExecuteCommand($"map {Server.MapName}");
        }

        Console.WriteLine($"{LogPrefix}Plugin loaded!");
    }
}