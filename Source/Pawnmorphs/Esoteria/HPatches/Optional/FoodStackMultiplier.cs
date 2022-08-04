using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.HPatches.Optional
{
    [HarmonyPatch]
    static class FoodStackMultiplier
    {
        static bool Prepare(MethodBase original)
        {
            bool patch = PawnmorpherMod.Settings.patchMaxFoodEaten;
#if DEBUG
            if (patch)
                Log.Message("Max food eaten patched");
#endif

            return patch;
        }

        [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.WillIngestStackCountOf)), HarmonyPrefix]
        static bool WillIngestStackCountOf(Pawn ingester, ThingDef def, float singleFoodNutrition, ref int __result)
        {
            float num = Mathf.Min(def.ingestible.maxNumToIngestAtOnce * ingester.BodySize, FoodUtility.StackCountForNutrition(ingester.needs.food.NutritionWanted, singleFoodNutrition));
            if (num < 1)
            {
                num = 1;
            }
            __result = (int)num;
            Log.Message(num.ToString());

            return false;
        }


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
