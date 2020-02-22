// PlantPatches.cs modified by Iron Wolf for Pawnmorph on 12/31/2019 3:25 PM
// last updated 12/31/2019  3:25 PM

using Harmony;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.HPatches
{
    static class PlantPatches
    {

        [HarmonyPatch(typeof(WorkGiver_GrowerSow)), HarmonyPatch(nameof(WorkGiver_GrowerSow.JobOnCell))]
        static class SowerJobOnCellPatch
        {
            [HarmonyPrefix]
            static bool RespectRestrictionsPatch([NotNull] Pawn pawn, IntVec3 c, bool forced, ref Job __result)
            {
                if (pawn.Map == null) return true; 
                
                var plant = WorkGiver_Grower.CalculateWantedPlantDef(c, pawn.Map);
                if (plant != null)
                {
                    if (!plant.IsValidFor(pawn))
                    {
                        __result = null;
                        return false; 
                    }
                }

                return true; 
            }
        }

        [HarmonyPatch(typeof(WorkGiver_GrowerSow)), HarmonyPatch("ExtraRequirements")]
        static class ExtraRequirementsPatch
        {
            [HarmonyPostfix]
            static void RespectRequirementsPatch([NotNull] IPlantToGrowSettable settable, [NotNull] Pawn pawn, ref bool __result)
            {
                if (!__result) return;
                IntVec3 c;
                if (settable is Zone_Growing growingZone)
                {
                    c = growingZone.Cells[0]; 
                }
                else
                {
                    c = ((Thing) settable).Position; 
                }

                var plant = WorkGiver_Grower.CalculateWantedPlantDef(c, pawn.Map);
                if (plant == null) return;
                __result = plant.IsValidFor(pawn);
            }
        }
    }
}