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
    }

    [GameEventHandler]
    public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo anInfo) {
        CCSPlayerController myPlayer = @event.Userid;

        if (!myPlayer.isPlayerValid()) {
            return HookResult.Continue;
        }

        myPlayer.SwitchTeam(CsTeam.Spectator);

        theQueueManager.addPlayerToQueuePlayers(myPlayer);

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo anInfo) {
        CCSGameRules myGameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
        if (myGameRules.WarmupPeriod) {
            return HookResult.Continue;
        }
        myGameRules.IsValveDS = true;

        theCurrentBombsite = (Bombsite)new Random().Next(0, 2);

        theGameManager.ResetPlayerScores();

        Random random = new Random();
        List<CCSPlayerController> activeTerrorists = theQueueManager.getActivePlayers().Where(aPlayer => aPlayer.Team == CsTeam.Terrorist).ToList();

        int randomIndex = random.Next(activeTerrorists.Count);
        if (randomIndex == 0) {
            foreach (CCSPlayerController aPlayer in Utilities.GetPlayers().Where(aPlayer => aPlayer.isPlayerPawnValid() && aPlayer.PawnIsAlive)) {
                aPlayer.PrintToChat("No Planter. Skipping round.");
                aPlayer.CommitSuicide(true, true);
            }
        }
        thePlanter = activeTerrorists[randomIndex];

        theGameManager.handleSpawns(theCurrentBombsite, theMapConfig);

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

        theGameManager.AddScore(myPlayer, GameManager.ScoreForDefuse);

        return HookResult.Continue;
    }

    public HookResult OnCommandJoinTeam(CCSPlayerController? aPlayer, CommandInfo anInfo) {
        return HookResult.Handled;
    }

    [GameEventHandler(HookMode.Pre)]
    public HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo anInfo) {
        if (Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!.WarmupPeriod && Utilities.GetPlayers().Count >= 2) {
            Server.ExecuteCommand("mp_warmup_end");
            foreach (CCSPlayerController aPlayer in Utilities.GetPlayers().Where(aPlayer => aPlayer.isPlayerPawnValid() && aPlayer.PawnIsAlive)) {
                aPlayer.PrintToChat("Enough players have joined. Starting game!");
                aPlayer.CommitSuicide(true, true);
            }
        }
        var myPlayer = @event.Userid;
        CsTeam myOldTeam = (CsTeam)@event.Oldteam;
        CsTeam myNewTeam = (CsTeam)@event.Team;
        @event.Silent = true;

        if (!myPlayer.isPlayerValid()) {
            return HookResult.Continue;
        }

        if (myNewTeam == CsTeam.Spectator) {
            if (theQueueManager.isPlayerActive(myPlayer)) {
                theQueueManager.removePlayerFromQueues(myPlayer);
            }
            return HookResult.Continue;
        }

        if (myOldTeam == CsTeam.Spectator && myNewTeam != CsTeam.Spectator) {
            theQueueManager.addPlayerToQueuePlayers(myPlayer);
        }

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
            theGameManager.AddScore(myAttacker, GameManager.ScoreForKill);
        }

        if (myAssister.isPlayerValid()) {
            theGameManager.AddScore(myAssister, GameManager.ScoreForAssist);
        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerAwardedMvp(EventRoundMvp @event, GameEventInfo anInfo) {
        CCSPlayerController myMvp = @event.Userid;

        if (myMvp.isPlayerValid()) {
            theGameManager.AddScore(myMvp, GameManager.ScoreForMvp);
        }

        return HookResult.Continue;
    }
}