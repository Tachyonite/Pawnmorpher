// BedPatches.cs created by Iron Wolf for Pawnmorph on 03/23/2020 4:43 PM
// last updated 03/23/2020  4:43 PM

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.CanUseBedEver))]
    static class BedPatches
    {
        static void Postfix(ref bool __result, [NotNull] Pawn p, ThingDef bedDef)
        {
            if (!__result && p.GetIntelligence() == Intelligence.Humanlike)
            {
                __result = bedDef?.building?.bed_humanlike == true; 
            }else if (__result && p.GetIntelligence() < Intelligence.ToolUser)
            {
                BuildingProperties building = bedDef?.building;
                __result = building != null &&  p.BodySize <= (double) building.bed_maxBodySize;
            }
        }
    }
}