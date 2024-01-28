using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using MakisRetake.Configs;
using System.Collections.Immutable;

namespace MakisRetake.Managers;

public class QueueManager {

    private readonly GameManager theGameManager;
    private readonly MakisConfig theMakiConfig;

    private readonly int theMaxActivePlayers = 9;
    private readonly float theTerroristRatio = 0.45f;

    private List<CCSPlayerController> theQueuePlayers = new();
    private HashSet<CCSPlayerController> theActivePlayers = new();

    public QueueManager(GameManager aGameManager, MakisConfig aMakiConfig) {
        theGameManager = aGameManager;
        theMakiConfig = aMakiConfig;
    }

    public int getTargetTerroristNum() {
        var myTerroristNum = (int)Math.Round(theTerroristRatio * theActivePlayers.Count);

        return myTerroristNum > 0 ? myTerroristNum : 1;
    }

    public int getTargetCounterTerroristNum() {
        return theActivePlayers.Count - getTargetCounterTerroristNum();
    }

    public void addPlayerToActive(CCSPlayerController aPlayer) {
        theActivePlayers.Add(aPlayer);
    }

    public void addPlayerToQueue(CCSPlayerController aPlayer) {
        theQueuePlayers.Add(aPlayer);
    }

    public void removePlayerFromActive(CCSPlayerController aPlayer) {
        theActivePlayers.Remove(aPlayer);
    }

    public void removePlayerFromQueue(CCSPlayerController aPlayer) {
        theQueuePlayers.Remove(aPlayer);
    }

    public HashSet<CCSPlayerController> getActivePlayers() {
        return theActivePlayers;
    }

    public void movePlayerToSpectator(CCSPlayerController aPlayer) {
        theActivePlayers.Remove(aPlayer);
        theQueuePlayers.Remove(aPlayer);
    }

    public bool isPlayerActive(CCSPlayerController aPlayer) {
        return theActivePlayers.Contains(aPlayer);
    }

    public bool isPlayerQueued(CCSPlayerController aPlayer) {
        return theQueuePlayers.Contains(aPlayer);
    }

    public bool isPlayerSpectator(CCSPlayerController aPlayer) {
        return (!theQueuePlayers.Contains(aPlayer) && !theActivePlayers.Contains(aPlayer));
    }

    public void balanceTeams() {
        if (!theMakiConfig.mySwitchTeamsOnWin) {
            return;
        }
        foreach (CCSPlayerController aPlayer in theActivePlayers) {
            
        }
    }

    public void updateQueue() {
        if (vipInQueue()) {
            foreach (var aPlayer in theQueuePlayers
                .Where(aPlayer => AdminManager.PlayerHasPermissions(aPlayer, "@css/vip"))
                .ToList()) {
                moveVipToActive(aPlayer);
            }
        }

        var myPlayersToAddNum = theMaxActivePlayers - theActivePlayers.Count;

        if (theQueuePlayers.Count > 0) {
            if (myPlayersToAddNum > 0) {
                var myPlayersToAdd = theQueuePlayers.Take(myPlayersToAddNum).ToList();
                foreach (var aPlayer in myPlayersToAdd) {
                    theQueuePlayers.Remove(aPlayer);
                    if (aPlayer.IsValid) {
                        theActivePlayers.Add(aPlayer);
                        aPlayer.SwitchTeam(CsTeam.CounterTerrorist);
                    }
                }

            }

            foreach (var aPlayer in theQueuePlayers) {
                aPlayer.PrintToChat("The game is currently full. Please wait for a spot to open up.");
            }
        }
    }

    private bool vipInQueue() {
        return theQueuePlayers.Where(aPlayer => AdminManager.PlayerHasPermissions(aPlayer, "@css/vip")).ToList().Count > 0;
    }

    private bool vipCanJoinActive() {
        return theActivePlayers.Where(aPlayer => !AdminManager.PlayerHasPermissions(aPlayer, "@css/vip")).ToList().Count > 0;
    }

    private void moveVipToActive(CCSPlayerController aVipPlayer) {
        if (!vipCanJoinActive()) {
            return;
        }

        if (theActivePlayers.Count == theMaxActivePlayers) {
            var myNonVipActivePlayers = theActivePlayers.Where(aPlayer => !AdminManager.PlayerHasPermissions(aPlayer, "@css/vip")).ToList();
            int myRandomIndex = new Random().Next(myNonVipActivePlayers.Count);

            var myRemovedPlayer = myNonVipActivePlayers[myRandomIndex];
            myNonVipActivePlayers.Remove(myRemovedPlayer);
            myRemovedPlayer.PrintToChat("You have been moved to Queue due to a VIP.");
        }
        addPlayerToActive(aVipPlayer);
    }
}