using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using MakisRetake.Configs;
using MakisRetake.Enums;
using MakisRetake.Managers;
using System.Runtime.Serialization.Json;

namespace MakisRetake;

public partial class MakisRetake {
	private void OnMapStart(string aMapName) {
        if (theMapConfig == null || theMapConfig.getMapName().Equals(Server.MapName)) {
            theMapConfig = new MapConfig(ModuleDirectory, Server.MapName);
            theMapConfig.load();
        }
    }

	[GameEventHandler]
    public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo anInfo) {
		CCSPlayerController myPlayer = @event.Userid;

		//Check if player is valid
		if (!thePlayerManager.isPlayerValid(myPlayer)) {
			return HookResult.Continue;
    	}
		
		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo anInfo) {
        //skip warmup if needed
        if (Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!.WarmupPeriod) {
            return HookResult.Continue;
        }

        //choose bombsite
        theCurrentBombsite = (Bombsite)new Random().Next(0, 2);

		//reset scores of players
		theGameManager.ResetPlayerScores();

        Random random = new Random();
        List<CCSPlayerController> activeTerrorists = theQueueManager.getActivePlayers().Where(aPlayer => aPlayer.Team.Equals(CsTeam.Terrorist)).ToList();

        int randomIndex = random.Next(activeTerrorists.Count);
        thePlanter = activeTerrorists[randomIndex];

        foreach (CCSPlayerController player in theQueueManager.getActivePlayers().Where(aPlayer => thePlayerManager.isPlayerPawnValid(aPlayer))) {
			if (thePlayerManager.isPlayerValid(player)) {
				MapSpawn myMapSpawn = theMapConfig!.getRandomNonPlanterSpawn(theCurrentBombsite, player.Team);
                player.Teleport(myMapSpawn.theVector, myMapSpawn.theQAngle, new Vector(0f, 0f, 0f));



                //remove weapons, armour, and bomb
                //allocate weapons, grenades, equipment, and bomb
            }

        }

        //announce bombsite

        return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnRoundFreezeEnd(EventRoundFreezeEnd @event, GameEventInfo anInfo) {
        //skip warmup if needed
        if (Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!.WarmupPeriod) {
            return HookResult.Continue;
        }

        if (thePlayerManager.isPlayerValid(thePlanter) && thePlayerManager.isPlayerPawnValid(thePlanter)) {
            //autoplant
            theGameManager.autoPlantBomb(thePlanter, theCurrentBombsite);
        }

		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo anInfo) {
        //skip warmup
        if (Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!.WarmupPeriod) {
            return HookResult.Continue;
        }

        theGameManager.handleRoundWin((CsTeam)@event.Winner);

		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnBombDefused(EventBombDefused @event, GameEventInfo anInfo) {
		CCSPlayerController myPlayer = @event.Userid;

		//add score to player
		theGameManager.AddScore(myPlayer, GameManager.ScoreForDefuse);

		return HookResult.Continue;
	}

	[GameEventHandler(HookMode.Pre)]
	public HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo anInfo) {
        
        var myPlayer = @event.Userid;
        @event.Silent = true;

        if (!thePlayerManager.isPlayerValid(myPlayer)) {
            return HookResult.Continue;
        }

		CsTeam myOldTeam = (CsTeam)@event.Oldteam;
		CsTeam myNewTeam = (CsTeam)@event.Team;
        
		if (myOldTeam == CsTeam.None) {
			if (myOldTeam == myNewTeam) {
				//Player selected auto-select
				return HookResult.Continue;
			} else if (myNewTeam == CsTeam.Spectator) {
				//Player just joined the server
				return HookResult.Continue;
			}
		}

		if (theQueueManager.isPlayerActive(myPlayer)) {
			if (myNewTeam == CsTeam.Spectator) {
				theQueueManager.removePlayerFromQueues(myPlayer);
				return HookResult.Continue;
			}

		}
		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo anInfo) {
		//remove player from queues
		CCSPlayerController myPlayer = @event.Userid;
        
		//Refactor this
        theQueueManager.removePlayerFromQueues(myPlayer);
        
        return HookResult.Continue;
	}

	[GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo anInfo) {

        CCSPlayerController myAttacker = @event.Attacker;
        CCSPlayerController myAssister = @event.Assister;

        if (thePlayerManager.isPlayerValid(myAttacker)) {
            theGameManager.AddScore(myAttacker, GameManager.ScoreForKill);
        }

        if (thePlayerManager.isPlayerValid(myAssister)) {
            theGameManager.AddScore(myAssister, GameManager.ScoreForAssist);
        }
 
        return HookResult.Continue;
    }

	[GameEventHandler]
	public HookResult OnPlayerAwardedMvp (EventRoundMvp @event, GameEventInfo anInfo) {

		CCSPlayerController myMvp = @event.Userid;

		if (thePlayerManager.isPlayerValid(myMvp)) {
            theGameManager.AddScore(myMvp, GameManager.ScoreForMvp);
        }

		return HookResult.Continue;
	}
}