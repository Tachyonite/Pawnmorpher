// WildmanUtilityPatches.cs created by Iron Wolf for Pawnmorph on 03/15/2020 2:15 PM
// last updated 03/15/2020  2:15 PM

using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.HPatches
{
    [HarmonyPatch(typeof(WildManUtility))]
    static class WildmanUtilityPatches
    {
        [HarmonyPatch(nameof(WildManUtility.NonHumanlikeOrWildMan))]
       
            [HarmonyPostfix]
            static void FixNonHumanlikeOrWildmanPostfix(ref bool __result, [NotNull] Pawn p)
            {
                if (__result && p.RaceProps.Animal)
                {
                    __result = !p.IsHumanlike(); 
                }
            }
        

        [HarmonyPatch(nameof(WildManUtility.AnimalOrWildMan))]
       
            [HarmonyPostfix]
            static void FixAnimalOrWildman(ref bool __result, [NotNull] Pawn p)
            {
                if (__result && p.RaceProps.Animal)
                {
                    __result = !p.IsHumanlike(); 
                }
            }


#if false
        [HarmonyPatch(nameof(WildManUtility.IsWildMan))]
            [HarmonyPostfix]
            static void FixIsWildman(ref bool __result, [NotNull] Pawn p)
            {
                
            }
#endif

    }
}