// PawnCapacityUtilsPatch.cs modified by Iron Wolf for Pawnmorph on 09/26/2019 6:10 PM
// last updated 09/26/2019  6:10 PM

using System;
using System.Collections.Generic;
using Harmony;
using UnityEngine;
using Verse;
namespace Pawnmorph.HPatches
{
    
    public static class PawnCapacityUtilsPatch
    {
        [HarmonyPatch(typeof(PawnCapacityUtility))]
        [HarmonyPatch(nameof(PawnCapacityUtility.CalculateCapacityLevel))]
        [HarmonyPatch(new Type[]
        {
            typeof(HediffSet), typeof(PawnCapacityDef), typeof(List<PawnCapacityUtility.CapacityImpactor>)
        })]
        public static class GetCapacityLvPatch
        {
            static void Postfix(ref float __result, HediffSet diffSet, PawnCapacityDef capacity,
                                List<AspectCapacityImpactor> impactors)
            {
                var pawn = diffSet.pawn;
                var aspectTracker = pawn.GetAspectTracker();
                if (aspectTracker != null && __result > 0)
                {
                    float offset = 0;
                    float postFix = 1;
                    float setMax = float.PositiveInfinity; 
                    foreach (Aspect aspect in aspectTracker.Aspects)
                    {
                        if(!aspect.HasCapMods) continue;
                        foreach (PawnCapacityModifier capMod in aspect.CapMods)
                        {
                            if(capMod.capacity != capacity) continue;

                            offset += capMod.offset;
                            postFix *= capMod.postFactor;
                            if (capMod.SetMaxDefined && (capMod.setMax < setMax))
                            {
                                setMax = capMod.setMax; 
                            }
                        }


                        impactors?.Add(new AspectCapacityImpactor(aspect));

                    }


                    

                    offset += GetTotalCapacityOffset(diffSet, capacity); //need to start with the uncapped offset value 
                    offset = Mathf.Min(offset * postFix, setMax); 
                    

                    GenMath.RoundedHundredth(Mathf.Max(offset, capacity.minValue));
                    __result = Mathf.Min(__result, offset); //take the min of the aspect modified value and the capped value from Rimworld's calculation 

                }




            }
        }


        /// <summary>
        /// find the original offset without setMax 
        /// </summary>
        /// <param name="hSet"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        static float GetTotalCapacityOffset(HediffSet hSet, PawnCapacityDef capacity)
        {
            float num = capacity.Worker.CalculateCapacityLevel(hSet);  
            float num3 = 1f;
            for (int i = 0; i < hSet.hediffs.Count; i++)
            {
                Hediff hediff = hSet.hediffs[i];
                List<PawnCapacityModifier> capMods = hediff.CapMods;
                if (capMods != null)
                {
                    for (int j = 0; j < capMods.Count; j++)
                    {
                        PawnCapacityModifier pawnCapacityModifier = capMods[j];
                        if (pawnCapacityModifier.capacity == capacity)
                        {
                            num += pawnCapacityModifier.offset;
                            num3 *= pawnCapacityModifier.postFactor;
                          
                            
                        }
                    }
                }
            }
            num *= num3;
            return num; 
        }

    }
}