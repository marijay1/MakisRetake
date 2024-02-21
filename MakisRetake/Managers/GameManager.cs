using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using MakisRetake.Enums;
using CSPlus.Base.Entities;

namespace MakisRetake.Managers;

public class GameManager {
    private QueueManager theQueueManager;

    private Dictionary<CCSPlayerController, int> thePlayerPoints = new Dictionary<CCSPlayerController, int>();
    private int theCurrentConsecutiveWins = 0;
    private CsTeam theLastWinningTeam = CsTeam.None;
    private readonly int theConsecutiveWinsToScramble = 5;
    private readonly int theWinsToBreakStreak = 3;
    private readonly float theWinRatioToWarn = 0.60f;

    public const int ScoreForKill = 50;
    public const int ScoreForAssist = 25;
    public const int ScoreForDefuse = 25;
    public const int ScoreForMvp = 35;

    public GameManager(QueueManager aQueueManager) {
        theQueueManager = aQueueManager;
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
            if (theCurrentConsecutiveWins >= theConsecutiveWinsToScramble) {
                Server.PrintToChatAll($"The Terrorist have won {theCurrentConsecutiveWins} rounds in a row! Teams are being scrambled.");
                scrambleTeams();
                theCurrentConsecutiveWins = 0;
            } else if (theCurrentConsecutiveWins > (theConsecutiveWinsToScramble * theWinRatioToWarn)) {
                Server.PrintToChatAll($"The Terrorist have won {theCurrentConsecutiveWins} rounds in a row! {theConsecutiveWinsToScramble - theCurrentConsecutiveWins} more rounds until scramble.");
            }
        }

        if (aWinningTeam == CsTeam.CounterTerrorist) {
            balanceTeams();
        }
    }

    private void scrambleTeams() {
        List<CCSPlayerController> myActivePlayers = theQueueManager.getActivePlayers();

        Random random = new Random();
        for (int i = myActivePlayers.Count - 1; i > 0; i--) {
            int j = random.Next(i + 1);
            CCSPlayerController myTempPlayer = myActivePlayers[i];
            myActivePlayers[i] = myActivePlayers[j];
            myActivePlayers[j] = myTempPlayer;
        }

        List<CCSPlayerController> myNewTerrorists = myActivePlayers.Take(theQueueManager.getTargetTerroristNum()).ToList();
        List<CCSPlayerController> myNewCounterTerrorists = myActivePlayers.Except(myNewTerrorists).ToList();

        setTeams(myNewTerrorists, myNewCounterTerrorists);
    }

    private void balanceTeams() {
        theQueueManager.updateQueue();
        List<CCSPlayerController> myOldTerrorists = theQueueManager.getActivePlayers().Where(aPlayer => aPlayer.Team == CsTeam.Terrorist).ToList();
        List<CCSPlayerController> myOldCounterTerrorists = theQueueManager.getActivePlayers().Where(aPlayer => aPlayer.Team == CsTeam.CounterTerrorist).ToList();

        List<CCSPlayerController> myNewTerrorists = new();
        List<CCSPlayerController> myNewCounterTerrorists = new();

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