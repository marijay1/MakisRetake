using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CSPlus.Base.Entities;
using MakisRetake.Configs;
using MakisRetake.Enums;

namespace MakisRetake;

public partial class MakisRetake {

    [ConsoleCommand("css_addspawn", "Adds a spawn point to the retakes config.")]
    [CommandHelper(minArgs: 3, usage: "[T/CT] [A/B] [Y/N (planter spawn)]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    [RequiresPermissions("@css/admin")]
    public void AddSpawnCommand(CCSPlayerController? aPlayer, CommandInfo aCommandInfo) {
        if (aPlayer == null || !aPlayer.isPlayerPawnValid()) {
            return;
        }

        CCSPlayerPawn myPlayerPawn = aPlayer.PlayerPawn.Value;

        if (aCommandInfo.ArgCount != 4) {
            aCommandInfo.ReplyToCommand("!addspawn [T/CT] [A/B] [Y/N (planter spawn)]");
            return;
        }

        string myTeamString = aCommandInfo.GetArg(1).ToUpper();
        string myBombsiteString = aCommandInfo.GetArg(2).ToUpper();
        string myPlanterSpawnString = aCommandInfo.GetArg(3).ToUpper();

        if (!new[] { "T", "CT" }.Contains(myTeamString) || !new[] { "A", "B" }.Contains(myBombsiteString)) {
            aCommandInfo.ReplyToCommand("!addspawn [T/CT] [A/B] [Y/N (planter spawn)]");
            return;
        }

        bool myIsPlanterSpawn = myPlanterSpawnString == "Y";

        theMapConfig.addSpawn(new MapSpawn(myPlayerPawn.AbsOrigin, myPlayerPawn.AbsRotation,
                                            myTeamString == "T" ? CsTeam.Terrorist : CsTeam.CounterTerrorist,
                                            myBombsiteString == "A" ? Bombsite.A : Bombsite.B,
                                            myIsPlanterSpawn));
    }

    [ConsoleCommand("css_removespawn", "Removes the spawn closest to you.")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    [RequiresPermissions("@css/admin")]
    public void RemoveSpawnCommand(CCSPlayerController? aPlayer, CommandInfo aCommandInfo) {
        if (aPlayer == null || !aPlayer.isPlayerPawnValid()) {
            return;
        }

        Vector myPlayerVector = aPlayer.PlayerPawn.Value!.AbsOrigin!;
        double closestSpawnDistance = double.MaxValue;
        MapSpawn? closestSpawn = null;

        foreach (MapSpawn aMapSpawn in theMapConfig.getMapSpawns()) {
            Vector spawnVector = aMapSpawn.theVector;

            double distanceX = spawnVector.X - myPlayerVector.X;
            double distanceY = spawnVector.Y - myPlayerVector.Y;

            double distance = Math.Sqrt(Math.Pow(distanceX, 2) + Math.Pow(distanceY, 2));

            if (distance > closestSpawnDistance) continue;

            closestSpawnDistance = distance;
            closestSpawn = aMapSpawn;
        }

        if (closestSpawn != null) {
            theMapConfig.removeSpawn(closestSpawn);
        }
    }
}