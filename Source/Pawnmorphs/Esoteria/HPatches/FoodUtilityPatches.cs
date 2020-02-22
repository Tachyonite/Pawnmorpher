// FoodUtilityPatches.cs modified by Iron Wolf for Pawnmorph on 01/19/2020 4:33 PM
// last updated 01/19/2020  4:33 PM

using Harmony;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
    static class FoodUtilityPatches
    {

        [HarmonyPatch(typeof(FoodUtility)), HarmonyPatch(nameof(FoodUtility.AddFoodPoisoningHediff))]
        static class FoodPoisoningIgnoreChance
        {
            static bool Prefix(Pawn pawn, Thing ingestible,  FoodPoisonCause cause)
            {
                if (cause == FoodPoisonCause.DangerousFoodType)
                {
                    var ignoreChance = 1 - pawn.GetStatValue(PMStatDefOf.DangerousFoodSensitivity);
                    if (Rand.Value < ignoreChance) return false; //do not add food poisoning 
                }else if (cause == FoodPoisonCause.Rotten)
                {
                    var ignoreChance = 1 - pawn.GetStatValue(PMStatDefOf.RottenFoodSensitivity);
                    if (Rand.Value < ignoreChance) return false; //do not add food poisoning 
                }

                return true; 
            }
        }
    }
}