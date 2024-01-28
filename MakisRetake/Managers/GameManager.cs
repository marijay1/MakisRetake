using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using MakisRetake.Enums;

namespace MakisRetake.Managers;

public class GameManager {

    private Dictionary<int, int> thePlayerPoints = new Dictionary<int, int>();
    private int theCurrentConsecutiveWins = 0;
    private CsTeam theLastWinningTeam = CsTeam.None;
    private readonly int theConsecutiveWinsToScramble = 5;
    private readonly int theWinsToBreakStreak = 3;
    private readonly float theWinRatioToWarn = 0.60f;

    public const int ScoreForKill = 50;
    public const int ScoreForAssist = 25;
    public const int ScoreForDefuse = 25;
    public const int ScoreForMvp = 35;


    public GameManager() {

    }

    public void ResetPlayerScores()
    {
        thePlayerPoints = new Dictionary<int, int>();
    }

    public void AddScore(CCSPlayerController aPlayer, int aScore)
    {
        if (!aPlayer.IsValid || aPlayer.UserId == null)
        {
            return;
        }

        var myPlayerId = (int)aPlayer.UserId;

        if (!thePlayerPoints.TryAdd(myPlayerId, aScore))
        {
            // Add to the player's existing score
            thePlayerPoints[myPlayerId] += aScore;
        }
    }


    public void handleRoundWin(CsTeam aWinningTeam) {
        theCurrentConsecutiveWins++;

        if (aWinningTeam == CsTeam.Terrorist) {
            if (theCurrentConsecutiveWins >=  theWinsToBreakStreak) {
                Server.PrintToChatAll(String.Format("The Terrorist have won {0} rounds in a row! Teams are being scrambled.", theCurrentConsecutiveWins));
                scrambleTeams();
                theCurrentConsecutiveWins = 0;
            }

            if (theCurrentConsecutiveWins > (theConsecutiveWinsToScramble * theWinRatioToWarn)) {
                Server.PrintToChatAll(String.Format("The Terrorist have won {0} rounds in a row! {1} more rounds until scramble.", theCurrentConsecutiveWins, theConsecutiveWinsToScramble - theCurrentConsecutiveWins));
            }
        } 

        if (aWinningTeam == CsTeam.CounterTerrorist) {
            //TODO
        }

        if (theLastWinningTeam != CsTeam.None && aWinningTeam != theLastWinningTeam && theCurrentConsecutiveWins >= theWinsToBreakStreak) {
            Server.PrintToChatAll(String.Format("The {0} have broken the {1}'s winstreak of {3}!"));
            theCurrentConsecutiveWins = 0;
        }
    }

    private void scrambleTeams() {
        // TODO
        // var shuffledActivePlayers = Helpers.Shuffle(QueueManager.ActivePlayers);

        // var newTerrorists = shuffledActivePlayers.Take(QueueManager.GetTargetNumTerrorists()).ToList();
        // var newCounterTerrorists = shuffledActivePlayers.Except(newTerrorists).ToList();

        // SetTeams(newTerrorists, newCounterTerrorists);
    }

    public void autoPlantBomb(CCSPlayerController aPlayer, Bombsite aBombsite) {
        var myPlayerPawn = aPlayer.PlayerPawn;
        var myPlantedBomb = Utilities.CreateEntityByName<CPlantedC4>("planted_c4");

        if (!myPlayerPawn.IsValid || !aPlayer.IsValid || aPlayer.AbsOrigin == null
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

        var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
        gameRules.BombPlanted = true;
        gameRules.BombDefused = false;

        sendBombPlantEvent(aPlayer, aBombsite);
    }

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