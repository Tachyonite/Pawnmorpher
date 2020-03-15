// WildmanUtilityPatches.cs created by Iron Wolf for Pawnmorph on 03/15/2020 2:15 PM
// last updated 03/15/2020  2:15 PM

using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.HPatches
{
    static class WildmanUtilityPatches
    {
        [HarmonyPatch(typeof(WildManUtility), nameof(WildManUtility.NonHumanlikeOrWildMan))]
        static class NonHumanlikeOrWildmanPatch
        {
            [HarmonyPostfix]
            static void Postfix(ref bool __result, [NotNull] Pawn p)
            {
                if (__result && p.RaceProps.Animal)
                {
                    __result = !p.IsHumanlike(); 
                }
            }
        }

        [HarmonyPatch(typeof(WildManUtility), nameof(WildManUtility.AnimalOrWildMan))]
        static class AnimalOrWildmanPatch
        {
            [HarmonyPostfix]
            static void Postfix(ref bool __result, [NotNull] Pawn p)
            {
                if (__result && p.RaceProps.Animal)
                {
                    __result = !p.IsHumanlike(); 
                }
            }
        }



    }
}