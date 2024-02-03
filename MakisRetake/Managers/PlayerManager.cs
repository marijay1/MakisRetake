using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;

namespace MakisRetake.Managers;

public class PlayerManager {

    public PlayerManager() {

    }

    public bool isPlayerValid(CCSPlayerController aPlayer) {
        return aPlayer != null && aPlayer.IsValid;
    }

    public bool isPlayerPawnValid (CCSPlayerController aPlayer) {
        return isPlayerValid(aPlayer) && aPlayer.PlayerPawn != null && aPlayer.PlayerPawn.IsValid;
    }
}