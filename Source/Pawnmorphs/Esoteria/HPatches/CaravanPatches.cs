// CaravanPatches.cs created by Iron Wolf for Pawnmorph on 03/18/2020 12:18 PM
// last updated 03/18/2020  12:18 PM

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Pawnmorph.Caravans;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Pawnmorph.HPatches
{
    internal static class CaravanPatches
    {
        [HarmonyPatch(typeof(CaravanFormingUtility), nameof(CaravanFormingUtility.StartFormingCaravan))]
        private static class CreateSapientAnimalCaravanPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(List<Pawn> pawns, List<Pawn> downedPawns, Faction faction,
                                       List<TransferableOneWay> transferables, IntVec3 meetingPoint, IntVec3 exitSpot,
                                       int startingTile, int destinationTile)
            {
                if (pawns?.Any(p => p.IsSapientFormerHuman()) == true)
                {
                    if (startingTile < 0)
                    {
                        Log.Error("Can't start forming caravan because startingTile is invalid.");
                        return false;
                    }

                    if (!pawns.Any())
                    {
                        Log.Error("Can't start forming caravan with 0 pawns.");
                        return false;
                    }

                    if (pawns.Any(x => x.Downed))
                        Log.Warning("Forming a caravan with a downed pawn. This shouldn't happen because we have to create a Lord.");
                    List<TransferableOneWay> list = transferables.ToList();
                    list.RemoveAll(x => x.CountToTransfer <= 0 || !x.HasAnyThing || x.AnyThing is Pawn);
                    for (var i = 0; i < pawns.Count; i++)
                        pawns[i].GetLord()?.Notify_PawnLost(pawns[i], PawnLostCondition.ForcedToJoinOtherLord);
                    var lordJob =
                        new LordJob_FormAndSendSapientAnimalCaravan(list, downedPawns, meetingPoint, exitSpot, startingTile,
                                                                    destinationTile);
                    LordMaker.MakeNewLord(Faction.OfPlayer, lordJob, pawns[0].MapHeld, pawns);
                    foreach (Pawn pawn in pawns)
                    {
                        if (pawn.Spawned) pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    }

                    return false;
                }

                return true;
            }
        }
    }
}