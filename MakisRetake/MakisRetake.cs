using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Utils;
using MakisRetake.Configs;
using MakisRetake.Enums;
using MakisRetake.Managers;
using System.Text;

namespace MakisRetake;

[MinimumApiVersion(194)]
public partial class MakisRetake : BasePlugin, IPluginConfig<MakisConfig> {
    private const string Version = "1.2.0";

    public override string ModuleName => "Maki's Retake";
    public override string ModuleVersion => Version;
    public override string ModuleAuthor => "Panduuuh";
    public override string ModuleDescription => "Main Retake plugin for Maki's";

    public static readonly string LogPrefix = $"[Maki's Retakes {Version}] ";
    public static readonly string MessagePrefix = $"[{ChatColors.LightPurple}Maki's Retakes{ChatColors.White}]";
    public static MakisRetake Plugin;

    private Bombsite theCurrentBombsite = Bombsite.A;
    private CCSPlayerController? thePlanter;

    public MakisConfig Config { get; set; } = null!;
    private GameManager theGameManager;
    private QueueManager theQueueManager;
    private MapConfig? theMapConfig;

    public MakisRetake() {
        Plugin = this;
    }

    public void OnConfigParsed(MakisConfig aMakiConfig) {
        Config = aMakiConfig;
    }

    public override void Load(bool aHotReload) {
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        AddCommandListener("jointeam", OnCommandJoinTeam, HookMode.Pre);

        theQueueManager = new QueueManager(Config.theRetakesConfig);
        theGameManager = new GameManager(theQueueManager, Config);

        if (theGameManager == null) {
            Console.WriteLine($"{LogPrefix}Game Manager is not loaded!");
        }

        if (aHotReload) {
            Server.ExecuteCommand($"map {Server.MapName}");
        }

        Console.WriteLine($"{LogPrefix}Plugin loaded!");
    }

    private void executeRetakesConfiguration() {
        string myRetakesConfigDirectory = Path.Combine(Path.GetDirectoryName(ModuleDirectory), "..", "..", "..", "cfg", "MakisRetake");
        string myRetakesConfigPath = Path.Combine(myRetakesConfigDirectory, "retakes.cfg");

        if (!File.Exists(myRetakesConfigPath)) {
            Directory.CreateDirectory(myRetakesConfigDirectory);

            string myRetakesServerCommands = @"
            // Things you shouldn't change:
            bot_kick
            bot_quota 0
            mp_autoteambalance 0
            mp_forcecamera 1
            mp_give_player_c4 0
            mp_halftime 0
            mp_ignore_round_win_conditions 0
            mp_join_grace_time 0
            mp_match_can_clinch 0
            mp_maxmoney 0
            mp_playercashawards 0
            mp_respawn_on_death_ct 0
            mp_respawn_on_death_t 0
            mp_solid_teammates 1
            mp_teamcashawards 0
            mp_warmup_pausetimer 1
            sv_skirmish_id 0

            // Things you can change, and may want to:
            mp_roundtime_defuse 0.25
            mp_autokick 0
            mp_c4timer 40
            mp_freezetime 1
            mp_friendlyfire 0
            mp_round_restart_delay 2
            sv_talk_enemy_dead 0
            sv_talk_enemy_living 0
            sv_deadtalk 1
            spec_replay_enable 0
            mp_maxrounds 30
            mp_match_end_restart 0
            mp_timelimit 0
            mp_match_restart_delay 10
            mp_death_drop_gun 1
            mp_death_drop_defuser 1
            mp_death_drop_grenade 1
            mp_warmuptime 300
            mp_buytime 90
            mp_buy_anywhere 1

            echo [Maki's Retakes] Config loaded!
        ";

            using (FileStream myRetakesConfig = File.Create(myRetakesConfigPath)) {
                byte[] retakesCfgBytes = Encoding.UTF8.GetBytes(myRetakesServerCommands);
                myRetakesConfig.Write(retakesCfgBytes, 0, retakesCfgBytes.Length);
            }
        }

        Server.ExecuteCommand("exec MakisRetake/retakes.cfg");
    }
}