using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using CSPlus.Base.Entities;

namespace MakisRetake.Managers;

public class QueueManager {

    private readonly int theMaxActivePlayers = 9;
    private readonly float theTerroristRatio = 0.45f;

    private List<CCSPlayerController> theQueuePlayers = new();
    private List<CCSPlayerController> theActivePlayers = new();

    public QueueManager() {
    }

    public int getTargetTerroristNum() {
        var myTerroristNum = (int)Math.Round(theTerroristRatio * theActivePlayers.Count);

        return myTerroristNum > 0 ? myTerroristNum : 1;
    }

    public int getTargetCounterTerroristNum() {
        return theActivePlayers.Count - getTargetCounterTerroristNum();
    }

    public void addPlayerToQueuePlayers(CCSPlayerController aPlayer) {
        theQueuePlayers.Add(aPlayer);
    }

    public List<CCSPlayerController> getActivePlayers() {
        return theActivePlayers;
    }

    public void removePlayerFromQueues(CCSPlayerController aPlayer) {
        theActivePlayers.Remove(aPlayer);
        theQueuePlayers.Remove(aPlayer);
    }

    public bool isPlayerActive(CCSPlayerController aPlayer) {
        return theActivePlayers.Contains(aPlayer);
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
                    if (aPlayer.isPlayerValid()) {
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
            List<CCSPlayerController> myNonVipActivePlayers = theActivePlayers.Where(aPlayer => !AdminManager.PlayerHasPermissions(aPlayer, "@css/vip")).ToList();
            int myRandomIndex = new Random().Next(myNonVipActivePlayers.Count);

            CCSPlayerController myRemovedPlayer = myNonVipActivePlayers[myRandomIndex];
            myNonVipActivePlayers.Remove(myRemovedPlayer);
            theQueuePlayers.Remove(myRemovedPlayer);
            myRemovedPlayer.PrintToChat("You have been moved to Queue due to a VIP.");
        }
        theActivePlayers.Add(aVipPlayer);
    }
}