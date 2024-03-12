using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CSPlus.Base.Entities;
using MakisRetake.Configs;
using MakisRetake.Enums;
using MakisRetake.Managers;

namespace MakisRetake;

public partial class MakisRetake {

    private void OnMapStart(string aMapName) {
        executeRetakesConfiguration();
        theMapConfig = new MapConfig(ModuleDirectory, aMapName);
        theGameManager.resetGameManager();
    }

    [GameEventHandler]
    public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo anInfo) {
        CCSPlayerController myPlayer = @event.Userid;

        if (!myPlayer.isPlayerValid()) {
            return HookResult.Continue;
        }

        myPlayer.setTeam(CsTeam.Spectator);
        theQueueManager.addPlayerToQueuePlayers(myPlayer);

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo anInfo) {
        if (Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!.WarmupPeriod) {
            return HookResult.Continue;
        }

        if (theQueueManager.getActivePlayers().Count < 2) {
            Server.PrintToChatAll($"{MessagePrefix} {Localizer["mr.retakes.events.WarmupStart"]}");
            Server.ExecuteCommand("mp_warmup_start");
            theQueueManager.getActivePlayers().ForEach(aPlayer => { theQueueManager.removePlayerFromQueues(aPlayer); theQueueManager.addPlayerToQueuePlayers(aPlayer); });
            theGameManager.resetGameManager();
            return HookResult.Continue;
        }

        List<CCSPlayerController> myActiveTerrorists = theQueueManager.getActivePlayers().Where(aPlayer => aPlayer.TeamNum.Equals((int)CsTeam.Terrorist)).ToList();

        if (myActiveTerrorists.Count == 0 || theGameManager.getNumOfPlayersOnTeam(CsTeam.CounterTerrorist) == 0) {
            theGameManager.scrambleTeams();
            if (myActiveTerrorists.Count == 0) {
                foreach (CCSPlayerController aPlayer in Utilities.GetPlayers().Where(aPlayer => aPlayer.isPlayerPawnValid() && aPlayer.PawnIsAlive)) {
                    aPlayer.PrintToChat($"{MessagePrefix} {Localizer["mr.retakes.events.NoPlanter"]}");
                    aPlayer.CommitSuicide(true, true);
                }
            }

            return HookResult.Continue;
        }

        Random myRandom = new Random();

        theCurrentBombsite = myRandom.Next(0, 2) == 0 ? Bombsite.A : Bombsite.B;

        theGameManager.resetPlayerScores();

        int randomIndex = myRandom.Next(myActiveTerrorists.Count);
        thePlanter = myActiveTerrorists[randomIndex];

        theGameManager.handleSpawns(theCurrentBombsite, theMapConfig, thePlanter);
        theGameManager.announceBombsite(theCurrentBombsite);

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundFreezeEnd(EventRoundFreezeEnd @event, GameEventInfo anInfo) {
        if (Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!.WarmupPeriod) {
            return HookResult.Continue;
        }

        if (thePlanter != null && thePlanter.isPlayerPawnValid()) {
            theGameManager.autoPlantBomb(thePlanter, theCurrentBombsite);
        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo anInfo) {
        if (Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!.WarmupPeriod) {
            return HookResult.Continue;
        }

        theGameManager.handleRoundWin((CsTeam)@event.Winner);

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnBombDefused(EventBombDefused @event, GameEventInfo anInfo) {
        CCSPlayerController myPlayer = @event.Userid;

        theGameManager.addScore(myPlayer, GameManager.ScoreForDefuse);

        return HookResult.Continue;
    }

    public HookResult OnCommandJoinTeam(CCSPlayerController? aPlayer, CommandInfo anInfo) {
        if (aPlayer == null || anInfo.ArgCount < 2 || !Enum.TryParse<CsTeam>(anInfo.GetArg(1), out CsTeam myNewTeam)) {
            return HookResult.Handled;
        }

        CsTeam myOldTeam = aPlayer.Team;

        if (Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!.WarmupPeriod &&
            theQueueManager.getQueuePlayers().Where(aPlayer => aPlayer.isPlayerConnected()).Count() >= 2) {
            if (myNewTeam == CsTeam.Terrorist || myNewTeam == CsTeam.CounterTerrorist) {
                Server.ExecuteCommand("mp_warmup_end");

                Server.PrintToChatAll($"{MessagePrefix} {Localizer["mr.retakes.events.WarmupEnd"]}");
                theGameManager.scrambleTeams();
            }
            return HookResult.Continue;
        }

        if (!aPlayer.isPlayerValid()) {
            return HookResult.Handled;
        }

        if (myNewTeam == CsTeam.Spectator) {
            if (theQueueManager.isPlayerActive(aPlayer)) {
                theQueueManager.removePlayerFromQueues(aPlayer);
                aPlayer.PrintToChat($"{MakisRetake.MessagePrefix} {MakisRetake.Plugin.Localizer["mr.retakes.queue.JoinSpectator"]}");
            }
            return HookResult.Continue;
        }

        if (!theQueueManager.getQueuePlayers().Contains(aPlayer) && !theQueueManager.getActivePlayers().Contains(aPlayer)) {
            if (myNewTeam == CsTeam.Terrorist || myNewTeam == CsTeam.CounterTerrorist) {
                theQueueManager.addPlayerToQueuePlayers(aPlayer);
                aPlayer.PrintToChat($"{MakisRetake.MessagePrefix} {MakisRetake.Plugin.Localizer["mr.retakes.queue.Joined"]}");
            }
            return HookResult.Handled;
        }

        if (theQueueManager.getQueuePlayers().Contains(aPlayer)) {
            if (myNewTeam == CsTeam.Terrorist || myNewTeam == CsTeam.CounterTerrorist) {
                aPlayer.PrintToChat($"{MakisRetake.MessagePrefix} {MakisRetake.Plugin.Localizer["mr.retakes.queue.AlreadyInQueue"]}");
                return HookResult.Handled;
            }

            theQueueManager.removePlayerFromQueues(aPlayer);
            return HookResult.Continue;
        }

        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Pre)]
    public HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo anInfo) {
        @event.Silent = true;

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo anInfo) {
        CCSPlayerController myPlayer = @event.Userid;

        theQueueManager.removePlayerFromQueues(myPlayer);

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo anInfo) {
        CCSPlayerController myAttacker = @event.Attacker;
        CCSPlayerController myAssister = @event.Assister;

        if (myAttacker.isPlayerPawnValid()) {
            theGameManager.addScore(myAttacker, GameManager.ScoreForKill);
        }

        if (myAssister.isPlayerValid()) {
            theGameManager.addScore(myAssister, GameManager.ScoreForAssist);
        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerAwardedMvp(EventRoundMvp @event, GameEventInfo anInfo) {
        CCSPlayerController myMvp = @event.Userid;

        if (myMvp.isPlayerValid()) {
            theGameManager.addScore(myMvp, GameManager.ScoreForMvp);
        }

        return HookResult.Continue;
    }
}