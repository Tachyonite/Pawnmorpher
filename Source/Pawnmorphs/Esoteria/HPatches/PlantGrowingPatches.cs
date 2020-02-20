// PlantGrowingPatches.cs modified by Iron Wolf for Pawnmorph on 11/23/2019 9:18 AM
// last updated 11/23/2019  9:18 AM

using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
    internal static class PlantGrowingPatches
    {
#if false //disabled for now 
        internal static class WorkGiver_GrowerSowPatches
        {
            [HarmonyPatch(typeof(WorkGiver_GrowerSow), "ExtraRequirements")]
            internal static class ExtraRequirementsPatch
            {
                internal static void Postfix(Pawn pawn, IPlantToGrowSettable settable, ref bool __result)
                {
                    if (!__result) return;
                    //TODO implement some mutagenic plants 
                    if (false) //replace with check for mutagenic plant when we have some 
#pragma warning disable 162
                        __result = pawn.CanGrowMutagenicPlants();
#pragma warning restore 162
                }
            }
        }

        //should we also restrict the cutting of mutagenic plants? 

        internal static class WorkGiver_GrowerHarvestPatches
        {
            [HarmonyPatch(typeof(WorkGiver_GrowerHarvest), nameof(WorkGiver_GrowerHarvest.HasJobOnCell))]
            internal static class HasJobOnCellPatch
            {
                internal static void Postfix(Pawn pawn, IntVec3 c, ref bool __result)
                {
                    if (!__result) return;
                    if (pawn == null) return;
                    Plant plant = c.GetPlant(pawn.Map);
                    if (false) //replace with check for mutagenic plant when we have some 
                        __result = pawn.CanGrowMutagenicPlants();
                }
            }
        }

#endif
    }
}