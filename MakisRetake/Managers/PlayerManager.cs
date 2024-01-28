using CounterStrikeSharp.API.Core;

namespace MakisRetake.Managers;

public class PlayerManager {

    public PlayerManager() {

    }

    public static bool IsPlayerConnected(CCSPlayerController player) {
        return player.Connected == PlayerConnectedState.PlayerConnected;
    }
}