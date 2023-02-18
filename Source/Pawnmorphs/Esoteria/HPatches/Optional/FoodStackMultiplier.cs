using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.HPatches.Optional
{
    [OptionalPatch("PMFoodStackMultiplierCaption", "PMFoodStackMultiplierDescription", nameof(_enabled), true)]
    [HarmonyPatch]
    static class FoodStackMultiplier
    {
        static bool _enabled = true;

        static bool Prepare(MethodBase original)
        {
            if (original == null && _enabled)
                Log.Message("[PM] Optional meal size patch enabled.");

            return _enabled;
        }

        /// <summary>
        /// Copied vanilla code and added ingester.BodySize multiplier.
        /// </summary>
        [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.WillIngestStackCountOf)), HarmonyPrefix]
        static bool WillIngestStackCountOf(Pawn ingester, ThingDef def, float singleFoodNutrition, ref int __result)
        {
            float num = Mathf.Min(def.ingestible.maxNumToIngestAtOnce * ingester.BodySize, FoodUtility.StackCountForNutrition(ingester.needs.food.NutritionWanted, singleFoodNutrition));
            if (num < 1)
            {
                num = 1;
            }
            __result = (int)num;
            return false;
        }


        /// <summary>
        /// Copied vanilla code and added ingester.BodySize multiplier.
        /// Also reduced the number of calls to GetStatValue(StatDefOf.Nutrition).
        /// </summary>
        [HarmonyPatch(typeof(Thing), "IngestedCalculateAmounts"), HarmonyPrefix]
        static bool IngestedCalculateAmounts(Pawn ingester, float nutritionWanted, out int numTaken, out float nutritionIngested, Thing __instance, ThingDef ___def, int ___stackCount)
        {
            float nutrition = __instance.GetStatValue(StatDefOf.Nutrition);
            numTaken = Mathf.CeilToInt(nutritionWanted / nutrition);
            numTaken = (int)Mathf.Min(numTaken, ___def.ingestible.maxNumToIngestAtOnce * ingester.BodySize, ___stackCount);
            numTaken = Mathf.Max(numTaken, 1);
            nutritionIngested = (float)numTaken * nutrition;

            return false;
        }
    }
}
