using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CSPlus.Base.Entities;

public static class CCSPlayerControllerExtensions {

    public static bool isPlayerValid(this CCSPlayerController aPlayer) {
        return aPlayer != null && aPlayer.IsValid;
    }

    public static bool isPlayerPawnValid(this CCSPlayerController aPlayer) {
        return isPlayerValid(aPlayer) && aPlayer.PlayerPawn != null && aPlayer.PlayerPawn.IsValid;
    }

    public static bool isPlayerConnected(this CCSPlayerController aPlayer) {
        return aPlayer.Connected == PlayerConnectedState.PlayerConnected;
    }

    public static void setTeam(this CCSPlayerController aPlayer, CsTeam aTeam) {
        aPlayer.SwitchTeam(aTeam);
        aPlayer.TeamNum = (byte)aTeam;
    }
}