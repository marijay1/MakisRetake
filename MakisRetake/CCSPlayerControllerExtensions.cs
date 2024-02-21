using CounterStrikeSharp.API.Core;

namespace CSPlus.Base.Entities;

public static class CCSPlayerControllerExtensions {

    public static bool isPlayerValid(this CCSPlayerController aPlayer) {
        return aPlayer != null && aPlayer.IsValid;
    }

    public static bool isPlayerPawnValid(this CCSPlayerController aPlayer) {
        return isPlayerValid(aPlayer) && aPlayer.PlayerPawn != null && aPlayer.PlayerPawn.IsValid;
    }
}