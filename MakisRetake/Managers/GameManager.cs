using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSPlus.Base.Entities;
using MakisRetake.Configs;
using MakisRetake.Enums;

namespace MakisRetake.Managers;

public class GameManager {
    public const int ScoreForKill = 50;
    public const int ScoreForAssist = 25;
    public const int ScoreForDefuse = 25;
    public const int ScoreForMvp = 35;

    private int theCurrentConsecutiveWins = 0;
    private readonly int theWinsToBreakStreak = 3;
    private readonly float theWinRatioToWarn = 0.60f;
    private CsTeam theLastWinningTeam = CsTeam.None;

    private Dictionary<CCSPlayerController, int> thePlayerPoints = new Dictionary<CCSPlayerController, int>();

    private QueueManager theQueueManager;
    private RetakesConfig theRetakesConfig;

    public GameManager(QueueManager aQueueManager, MakisConfig aConfig) {
        theQueueManager = aQueueManager;
        theRetakesConfig = aConfig.theRetakesConfig;
    }

    public void resetPlayerScores() {
        thePlayerPoints = new Dictionary<CCSPlayerController, int>();
    }

    public void addScore(CCSPlayerController aPlayer, int aScore) {
        if (!aPlayer.isPlayerValid()) {
            return;
        }

        if (!thePlayerPoints.TryAdd(aPlayer, aScore)) {
            thePlayerPoints[aPlayer] += aScore;
        }
    }

    public void handleRoundWin(CsTeam aWinningTeam) {
        theCurrentConsecutiveWins++;
        if (theLastWinningTeam != CsTeam.None) {
            return;
        }

        if (aWinningTeam != theLastWinningTeam) {
            if (theCurrentConsecutiveWins >= theWinsToBreakStreak) {
                Server.PrintToChatAll($"{MakisRetake.MessagePrefix} {MakisRetake.Plugin.Localizer["mr.retakes.team.WinStreakOver", aWinningTeam, theLastWinningTeam, theCurrentConsecutiveWins]}");
            }
            theCurrentConsecutiveWins = 0;
        }

        if (aWinningTeam == CsTeam.Terrorist) {
            if (theCurrentConsecutiveWins >= theRetakesConfig.theConsecutiveRoundsToScramble) {
                Server.PrintToChatAll($"{MakisRetake.MessagePrefix} {MakisRetake.Plugin.Localizer["mr.retakes.team.Scramble", theCurrentConsecutiveWins]}");
                scrambleTeams();
                theCurrentConsecutiveWins = 0;
            } else if (theCurrentConsecutiveWins > (theRetakesConfig.theConsecutiveRoundsToScramble * theWinRatioToWarn)) {
                Server.PrintToChatAll($"{MakisRetake.MessagePrefix} {MakisRetake.Plugin.Localizer["mr.retakes.team.ScrambleWarning", theCurrentConsecutiveWins, theRetakesConfig.theConsecutiveRoundsToScramble - theCurrentConsecutiveWins]}");
            }
        }

        if (aWinningTeam == CsTeam.CounterTerrorist) {
            balanceTeams();
        }
    }

    private void scrambleTeams() {
        List<CCSPlayerController> myActivePlayers = theQueueManager.getActivePlayers();

        if (myActivePlayers.Count == 2) {
            switchTeams(myActivePlayers);
        }

        Random random = new Random();

        for (int i = myActivePlayers.Count - 1; i > 0; i--) {
            int j = random.Next(i + 1);
            CCSPlayerController myTempPlayer = myActivePlayers[i];
            myActivePlayers[i] = myActivePlayers[j];
            myActivePlayers[j] = myTempPlayer;
        }

        List<CCSPlayerController> myNewTerrorists = myActivePlayers.Take(myActivePlayers.Count - theQueueManager.getTargetCounterTerroristNum()).ToList();
        List<CCSPlayerController> myNewCounterTerrorists = myActivePlayers.Except(myNewTerrorists).ToList();

        setTeams(myNewTerrorists, myNewCounterTerrorists);
    }

    private void balanceTeams() {
        theQueueManager.updateQueue();
        List<CCSPlayerController> myActivePlayers = theQueueManager.getActivePlayers();
        List<CCSPlayerController> myOldTerrorists = myActivePlayers.Where(aPlayer => aPlayer.Team == CsTeam.Terrorist).ToList();
        List<CCSPlayerController> myOldCounterTerrorists = myActivePlayers.Where(aPlayer => aPlayer.Team == CsTeam.CounterTerrorist).ToList();

        List<CCSPlayerController> myNewTerrorists = new();
        List<CCSPlayerController> myNewCounterTerrorists = new();

        if (myActivePlayers.Count == 2) {
            switchTeams(myActivePlayers);
        }

        CCSPlayerController myBestTerrorist = myOldTerrorists.MaxBy(p => thePlayerPoints[p])!;
        myNewTerrorists.Add(myBestTerrorist);
        myOldTerrorists.Remove(myBestTerrorist);
        while (!theQueueManager.getTargetTerroristNum().Equals(myNewTerrorists.Count)) {
            CCSPlayerController myBestCounterTerrorist = myOldCounterTerrorists.MaxBy(aPlayer => thePlayerPoints[aPlayer])!;
            myNewTerrorists.Add(myBestCounterTerrorist);
            myOldCounterTerrorists.Remove(myBestCounterTerrorist);
        }

        myOldCounterTerrorists.ForEach(aPlayer => myNewCounterTerrorists.Add(aPlayer));
        myOldTerrorists.ForEach(aPlayer => myNewCounterTerrorists.Add(aPlayer));

        setTeams(myNewTerrorists, myNewCounterTerrorists);
    }

    private void setTeams(List<CCSPlayerController> aTerrorists, List<CCSPlayerController> aCounterTerrorists) {
        aTerrorists.Where(aPlayer => aPlayer.isPlayerValid()).ToList().ForEach(aPlayer => aPlayer.SwitchTeam(CsTeam.Terrorist));
        aCounterTerrorists.Where(aPlayer => aPlayer.isPlayerValid()).ToList().ForEach(aPlayer => aPlayer.SwitchTeam(CsTeam.CounterTerrorist));
    }

    private void switchTeams(List<CCSPlayerController> anActivePlayers) {
        anActivePlayers.Where(aPlayer => aPlayer.isPlayerValid() && aPlayer.Team == CsTeam.Terrorist).ToList().ForEach(aPlayer => aPlayer.SwitchTeam(CsTeam.CounterTerrorist));
        anActivePlayers.Where(aPlayer => aPlayer.isPlayerValid() && aPlayer.Team == CsTeam.CounterTerrorist).ToList().ForEach(aPlayer => aPlayer.SwitchTeam(CsTeam.Terrorist));
    }

    public void handleSpawns(Bombsite aBombsite, MapConfig aMapConfig, CCSPlayerController aPlanter) {
        List<MapSpawn> mySpawns = aMapConfig.getMapSpawns()
            .Where(aSpawn => aSpawn.theBombsite.Equals(aBombsite)).ToList();

        Random myRandom = new Random();

        if (mySpawns.Count == 0) {
            Console.WriteLine("No Spawns!");
            return;
        }

        foreach (CCSPlayerController myPlayer in theQueueManager.getActivePlayers()) {
            CsTeam myTeam = myPlayer.Team;
            bool myIsPlanter = myPlayer.Equals(aPlanter);

            List<MapSpawn> myFilteredSpawns = new List<MapSpawn>();

            foreach (MapSpawn mySpawn in mySpawns) {
                if (mySpawn.theTeam != myTeam) {
                    continue;
                }
                if (mySpawn.theCanBePlanter != myIsPlanter) {
                    continue;
                }
                myFilteredSpawns.Add(mySpawn);
            }

            if (myFilteredSpawns.Count > 0) {
                int randomIndex = myRandom.Next(0, myFilteredSpawns.Count);
                MapSpawn myFinalSpawn = myFilteredSpawns[randomIndex];

                myPlayer.PlayerPawn.Value!.Teleport(myFinalSpawn.theVector, myFinalSpawn.theQAngle, new Vector());
                mySpawns.Remove(myFinalSpawn);
            } else {
                Console.WriteLine(myPlayer.Equals(aPlanter) ? "No planter spawns!" : "No non-planter spawns!");
            }
        }
    }

    public void announceBombsite(Bombsite aBombsite) {
        List<string> myAnnouncers = new List<string> {
                                    "balkan_epic",
                                    "leet_epic",
                                    "professional_epic",
                                    "professional_fem",
                                    "seal_epic",
                                    "swat_epic",
                                    "swat_fem"
                                    };

        int myNumberOfCounterTerrorists = theQueueManager.getActivePlayers().Where(aPlayer => aPlayer.Team == CsTeam.CounterTerrorist).ToList().Count;
        int myNumberOfTerrorists = theQueueManager.getActivePlayers().Where(aPlayer => aPlayer.Team == CsTeam.Terrorist).ToList().Count;

        string myAnnouncer = myAnnouncers[new Random().Next(0, myAnnouncers.Count)];

        foreach (CCSPlayerController myPlayer in Utilities.GetPlayers()) {
            myPlayer.PrintToChat($"{MakisRetake.MessagePrefix} {MakisRetake.Plugin.Localizer["mr.retakes.bombsite.announcement.Chat", aBombsite.ToString(), myNumberOfCounterTerrorists, myNumberOfTerrorists]}");
            //myPlayer.ExecuteClientCommand("snd_toolvolume .1");
            //myPlayer.ExecuteClientCommand($"play sounds/vo/agents/{myAnnouncer}/loc_{aBombsite.ToString().ToLower()}_01");
        }
    }

    //Code from https://github.com/B3none/cs2-retakes
    public void autoPlantBomb(CCSPlayerController aPlayer, Bombsite aBombsite) {
        var myPlantedBomb = Utilities.CreateEntityByName<CPlantedC4>("planted_c4");

        CCSPlayerPawn myPlayerPawn = aPlayer.PlayerPawn.Value;

        if (!aPlayer.isPlayerPawnValid() || !aPlayer.isPlayerValid() || myPlayerPawn.AbsOrigin == null
            || myPlantedBomb == null || myPlantedBomb.AbsOrigin == null) {
            //restart round?
            return;
        }

        myPlantedBomb.AbsOrigin.X = myPlayerPawn.AbsOrigin.X;
        myPlantedBomb.AbsOrigin.Y = myPlayerPawn.AbsOrigin.Y;
        myPlantedBomb.AbsOrigin.Z = myPlayerPawn.AbsOrigin.Z;
        myPlantedBomb.HasExploded = false;

        myPlantedBomb.BombSite = (int)aBombsite;
        myPlantedBomb.BombTicking = true;
        myPlantedBomb.CannotBeDefused = false;
        //cfg?
        myPlantedBomb.TimerLength = 40;

        myPlantedBomb.DispatchSpawn();

        CCSGameRules gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
        gameRules.BombPlanted = true;
        gameRules.BombDefused = false;

        sendBombPlantEvent(aPlayer, aBombsite);
    }

    //Code from https://github.com/B3none/cs2-retakes
    private void sendBombPlantEvent(CCSPlayerController aBombPlanter, Bombsite aBombsite) {
        if (aBombPlanter.PlayerPawn.Value == null) {
            return;
        }

        var myBombPlantEvent = NativeAPI.CreateEvent("bomb_planted", true);
        NativeAPI.SetEventPlayerController(myBombPlantEvent, "userid", aBombPlanter.Handle);
        NativeAPI.SetEventInt(myBombPlantEvent, "userid", (int)aBombPlanter.PlayerPawn.Value.Index);
        NativeAPI.SetEventInt(myBombPlantEvent, "site", (int)aBombsite);

        NativeAPI.FireEvent(myBombPlantEvent, false);
    }
}