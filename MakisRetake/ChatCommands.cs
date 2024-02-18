using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using MakisRetake.Configs;
using MakisRetake.Enums;

namespace MakisRetake;

public partial class MakisRetake {
    [ConsoleCommand("css_addspawn", "Adds a spawn point to the retakes config.")]
    [CommandHelper(minArgs: 3, usage: "[T/CT] [A/B] [Y/N (planter spawn)]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    [RequiresPermissions("@css/admin")]
    public void AddSpawnCommand(CCSPlayerController? aPlayer, CommandInfo aCommandInfo) {
        if (aPlayer == null) {
            //Player is null
            return;
        }
        if (!thePlayerManager.isPlayerPawnValid(aPlayer)) {
            //player pawn is not valid
            return;
        }

        if (aCommandInfo.ArgCount != 4) {
            //Invalid number of arguments
            aCommandInfo.ReplyToCommand("!addspawn [T/CT] [A/B] [Y/N (planter spawn)]");
        }

        string myTeamString = aCommandInfo.GetArg(1).ToUpper();
        string myBombsiteString = aCommandInfo.GetArg(2).ToUpper();
        string myPlanterSpawnString = aCommandInfo.GetArg(3).ToUpper();

        if (myTeamString == null &&
            myTeamString != "T" &&
            myTeamString != "CT") {
            //Team is invalid
            return;
        }

        CsTeam myTeam = myTeamString == "T" ? CsTeam.Terrorist : CsTeam.CounterTerrorist;

        if (myBombsiteString == null &&
            myBombsiteString != "A" &&
            myBombsiteString != "B") {
            //Bombsite is invalid
            return;
        }

        Bombsite myBombsite = myBombsiteString == "A" ? Bombsite.A : Bombsite.B;

        if (myPlanterSpawnString == null &&
            myPlanterSpawnString != "Y" &&
            myPlanterSpawnString != "N") {
            //planter boolean is invalid or blank, defaulting to false
            myPlanterSpawnString = "N";
        }

        bool myPlanterSpawn = myPlanterSpawnString == "Y" ? true : false;

        MapSpawn myMapSpawn = new MapSpawn(aPlayer.PlayerPawn.Value!.AbsOrigin!, aPlayer.PlayerPawn.Value!.AbsRotation!, myTeam, myBombsite, myPlanterSpawn);

        theMapConfig.addSpawn(myMapSpawn);
    }

    [ConsoleCommand("css_removespawn", "Removes the spawn closest to you.")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    [RequiresPermissions("@css/admin")]
    public void RemoveSpawnCommand(CCSPlayerController? aPlayer, CommandInfo aCommandInfo) {
        if (aPlayer == null) {
            //Player is null
            return;
        }
        if (!thePlayerManager.isPlayerPawnValid(aPlayer)) {
            //player pawn is not valid
            return;
        }

        Vector myPlayerVector = aPlayer.PlayerPawn.Value!.AbsOrigin!;
        double myClosestSpawnDistance = 9999.9;
        MapSpawn? myClosestSpawn = null;

        foreach (MapSpawn aMapSpawn in theMapConfig.getMapSpawns()) {
            Vector mySpawnVector = aMapSpawn.theVector;

            double myDistanceX = mySpawnVector.X - myPlayerVector.X;
            double myDistanceY = mySpawnVector.Y - myPlayerVector.Y;

            double myDistance = Math.Sqrt(Math.Pow(myDistanceX, 2) + Math.Pow(myDistanceY, 2));

            if (myDistance > myClosestSpawnDistance) {
                continue;
            }

            myClosestSpawnDistance = myDistance;
            myClosestSpawn = aMapSpawn;
        }

        if (myClosestSpawn != null) {
            theMapConfig.removeSpawn(myClosestSpawn);
        }
    }
}