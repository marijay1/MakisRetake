using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using CSPlus.Base.Entities;
using MakisRetake.Configs;

namespace MakisRetake.Managers;

public class QueueManager {
    private List<CCSPlayerController> theQueuePlayers = new();
    private List<CCSPlayerController> theActivePlayers = new();

    private RetakesConfig theRetakesConfig;

    public QueueManager(RetakesConfig aConfig) {
        theRetakesConfig = aConfig;
    }

    public int getTargetTerroristNum() {
        int myTerroristNum = (int)Math.Round(theRetakesConfig.theTerroristRatio * theActivePlayers.Count);

        return myTerroristNum > 0 ? myTerroristNum : 1;
    }

    public int getTargetCounterTerroristNum() {
        int myCounterTerroristNum = theActivePlayers.Count - getTargetTerroristNum();
        return myCounterTerroristNum > 0 ? myCounterTerroristNum : 1;
    }

    public void addPlayerToQueuePlayers(CCSPlayerController aPlayer) {
        aPlayer.PrintToChat($"{MakisRetake.MessagePrefix} {MakisRetake.Plugin.Localizer["mr.retakes.queue.Joined"]}");
        theQueuePlayers.Add(aPlayer);
    }

    public List<CCSPlayerController> getQueuePlayers() {
        return theQueuePlayers;
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

        var myPlayersToAddNum = theRetakesConfig.theMaxPlayers - theActivePlayers.Count;

        if (theQueuePlayers.Count > 0) {
            if (myPlayersToAddNum > 0) {
                var myPlayersToAdd = theQueuePlayers.Take(myPlayersToAddNum).ToList();
                foreach (var aPlayer in myPlayersToAdd) {
                    theQueuePlayers.Remove(aPlayer);
                    if (aPlayer.isPlayerValid()) {
                        theActivePlayers.Add(aPlayer);
                        aPlayer.setTeam(CsTeam.CounterTerrorist);
                    }
                }
                return;
            }

            foreach (var aPlayer in theQueuePlayers) {
                aPlayer.PrintToChat($"{MakisRetake.MessagePrefix} {MakisRetake.Plugin.Localizer["mr.retakes.queue.GameFull"]}");
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

        if (theActivePlayers.Count == theRetakesConfig.theMaxPlayers) {
            List<CCSPlayerController> myNonVipActivePlayers = theActivePlayers.Where(aPlayer => !AdminManager.PlayerHasPermissions(aPlayer, "@css/vip")).ToList();
            int myRandomIndex = new Random().Next(myNonVipActivePlayers.Count);

            CCSPlayerController myRemovedPlayer = myNonVipActivePlayers[myRandomIndex];
            myNonVipActivePlayers.Remove(myRemovedPlayer);
            theQueuePlayers.Remove(myRemovedPlayer);
            myRemovedPlayer.PrintToChat($"{MakisRetake.MessagePrefix} {MakisRetake.Plugin.Localizer["mr.retakes.queue.MovedToSpectator"]}");
        }
        theActivePlayers.Add(aVipPlayer);
    }
}