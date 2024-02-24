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

    public void ResetPlayerScores() {
        thePlayerPoints = new Dictionary<CCSPlayerController, int>();
    }

    public void AddScore(CCSPlayerController aPlayer, int aScore) {
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
                Server.PrintToChatAll($"The {aWinningTeam} have broken the {theLastWinningTeam}'s winstreak of {theCurrentConsecutiveWins}!");
            }
            theCurrentConsecutiveWins = 0;
        }

        if (aWinningTeam == CsTeam.Terrorist) {
            if (theCurrentConsecutiveWins >= theRetakesConfig.theConsecutiveRoundsToScramble) {
                Server.PrintToChatAll($"The Terrorist have won {theCurrentConsecutiveWins} rounds in a row! Teams are being scrambled.");
                scrambleTeams();
                theCurrentConsecutiveWins = 0;
            } else if (theCurrentConsecutiveWins > (theRetakesConfig.theConsecutiveRoundsToScramble * theWinRatioToWarn)) {
                Server.PrintToChatAll($"The Terrorist have won {theCurrentConsecutiveWins} rounds in a row! {theRetakesConfig.theConsecutiveRoundsToScramble - theCurrentConsecutiveWins} more rounds until scramble.");
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
        List<MapSpawn> mySpawns = aMapConfig.getMapSpawns().Where(aSpawn => aSpawn.theBombsite == aBombsite).ToList();
        Random myRandom = new Random();

        if (mySpawns.Count == 0) {
            Console.WriteLine("No Spawns!");
            return;
        }

        foreach (var myPlayer in theQueueManager.getActivePlayers()) {
            List<MapSpawn> myFilteredSpawns = mySpawns.Where(aSpawn => !aSpawn.theCanBePlanter && aSpawn.theTeam == myPlayer.Team).ToList();
            MapSpawn mySpawn = myPlayer == aPlanter ? mySpawns.FirstOrDefault(aSpawn => aSpawn.theCanBePlanter) : myFilteredSpawns.FirstOrDefault();

            if (mySpawn != null) {
                myPlayer.PlayerPawn.Value!.Teleport(mySpawn.theVector, mySpawn.theQAngle, new Vector());
                mySpawns.Remove(mySpawn);
            } else {
                Console.WriteLine(myPlayer == aPlanter ? "No planter spawns!" : "No non-planter spawns!");
            }
        }
    }

    //Code from https://github.com/B3none/cs2-retakes
    public void autoPlantBomb(CCSPlayerController aPlayer, Bombsite aBombsite) {
        var myPlayerPawn = aPlayer.PlayerPawn;
        var myPlantedBomb = Utilities.CreateEntityByName<CPlantedC4>("planted_c4");

        if (!aPlayer.isPlayerPawnValid() || !aPlayer.isPlayerValid() || aPlayer.AbsOrigin == null
            || myPlantedBomb == null || myPlantedBomb.AbsOrigin == null) {
            //restart round?
            return;
        }

        myPlantedBomb.AbsOrigin.X = aPlayer.AbsOrigin.X;
        myPlantedBomb.AbsOrigin.Y = aPlayer.AbsOrigin.Y;
        myPlantedBomb.AbsOrigin.Z = aPlayer.AbsOrigin.Z;
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