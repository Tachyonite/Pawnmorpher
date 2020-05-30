// BedPatches.cs created by Iron Wolf for Pawnmorph on 03/23/2020 4:43 PM
// last updated 03/23/2020  4:43 PM

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
    [HarmonyPatch(typeof(RestUtility))]
    internal static class BedPatches
    {
        [HarmonyPatch(nameof(RestUtility.CanUseBedEver))]
        [HarmonyPostfix]
        private static void CanUsebedEverPostfix(ref bool __result, [NotNull] Pawn p, ThingDef bedDef)
        {
            if (!__result && p.GetIntelligence() == Intelligence.Humanlike)
            {
                __result = bedDef?.building?.bed_humanlike == true;
            }
            else if (__result && p.GetIntelligence() < Intelligence.ToolUser)
            {
                BuildingProperties building = bedDef?.building;
                __result = building != null && p.BodySize <= (double) building.bed_maxBodySize;
            }
        }

        [HarmonyPatch(nameof(RestUtility.IsValidBedFor))]
        [HarmonyPostfix]
        private static void IsValidBedForFix(ref bool __result, Thing bedThing, Pawn sleeper, Pawn traveler,
                                             bool sleeperWillBePrisoner,
                                             bool checkSocialProperness, bool allowMedBedEvenIfSetToNoCare,
                                             bool ignoreOtherReservations)
        {
            if (!__result && sleeperWillBePrisoner && sleeper.GetIntelligence() == Intelligence.Animal)
            {
                if (!bedThing.Position.IsInPrisonCell(bedThing.Map)) return;

                BuildingProperties building = bedThing.def.building;
                __result = building?.bed_humanlike == false && sleeper.BodySize <= building.bed_maxBodySize;
            }
        }
    }

    [HarmonyPatch(typeof(Building_Bed))]
    static class BedBuildingPatches
    {
        [HarmonyPatch(nameof(Building_Bed.ForPrisoners), MethodType.Getter) ,HarmonyPostfix]
        static void FixForPrisoner(ref bool __result, Building_Bed __instance)
        {
            if (!__result)
            {
                __result = __instance.def.building?.bed_humanlike == false && __instance.Position.IsInPrisonCell(__instance.Map); 
            }
        }
    }
}