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
		var myPlayer = @event.Userid;

		//Check if player is valid
		if (!myPlayer.IsValid) {
			return HookResult.Continue;
    	}
		
		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnRoundPreStart(EventRoundPrestart @event, GameEventInfo anInfo) {
		//skip warmup if needed

		//Handle queue
		theQueueManager.updateQueue();

		return HookResult.Continue;	
	}

	[GameEventHandler]
	public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo anInfo) {
		//skip warmup if needed

		//choose bombsite
		// find reference for bombsite
		theCurrentBombsite = (Bombsite)new Random().Next(0, 2);

        //reset scores of players
        //spawn players
        //announce bombsite

        return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnRoundPostStart(EventRoundPoststart @event, GameEventInfo anInfo) {
		//skip warmup if needed

		//loop through players
		foreach (var player in theQueueManager.getActivePlayers().Where(aPlayer => aPlayer.IsValid)) {
			//check valid player
			//remove weapons, armour, and bomb
			//allocate weapons, grenades, equipment, and bomb
		}
		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnRoundFreezeEnd(EventRoundFreezeEnd @event, GameEventInfo anInfo) {
		//skip warmup if needed

		//autoplant
		//theGameManager.autoPlantBomb(/* player, bomb */);

		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo anInfo) {
		//set winner to last round winner

		return HookResult.Continue;
	}

	[GameEventHandler(HookMode.Pre)]
	public HookResult OnBombPlanted(EventBombPlanted @event, GameEventInfo anInfo) {
		//announce bombsite

		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnBombDefused(EventBombDefused @event, GameEventInfo anInfo) {
		//add score to player who defused
		CCSPlayerController myPlayer = @event.Userid;

		//add score to player
		theGameManager.AddScore(myPlayer, GameManager.ScoreForDefuse);

		return HookResult.Continue;
	}

	[GameEventHandler(HookMode.Pre)]
	public HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo anInfo) {
        
        var myPlayer = @event.Userid;
        @event.Silent = true;

        if (!myPlayer.IsValid) {
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
				theQueueManager.movePlayerToSpectator(myPlayer);
				return HookResult.Continue;
			}

		}
		return HookResult.Continue;
	}

	[GameEventHandler]
	public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo anInfo) {
		//remove player from queues
		CCSPlayerController myPlayer = @event.Userid;

		// check if player and gamemanager are null yadda yadda 
        
		//Refactor this
        theQueueManager.movePlayerToSpectator(myPlayer);
        
        return HookResult.Continue;
	}

	[GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo anInfo) {

        CCSPlayerController myAttacker = @event.Attacker;
        CCSPlayerController myAssister = @event.Assister;

        if (myAttacker.IsValid)
        {
            theGameManager.AddScore(myAttacker, GameManager.ScoreForKill);
        }

        if (myAssister.IsValid)
        {
            theGameManager.AddScore(myAssister, GameManager.ScoreForAssist);
        }
 
        return HookResult.Continue;
    }

	[GameEventHandler]
	public HookResult OnPlayerAwardedMvp (EventRoundMvp @event, GameEventInfo anInfo) {

		CCSPlayerController myMvp = @event.Userid;

		if (myMvp.IsValid) {
            theGameManager.AddScore(myMvp, GameManager.ScoreForMvp);
        }

		return HookResult.Continue;
	}
}