using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    [HarmonyPatch(typeof(HealthCardUtility), nameof(HealthCardUtility.GetPawnCapacityTip), new Type[] { typeof(Pawn), typeof(PawnCapacityDef) })]
    [StaticConstructorOnStartup]
    static class Patch_HealthCardUtility_GetPawnCapacityTip
    {
        static void Postfix(ref string __result, Pawn pawn, PawnCapacityDef capacity)
        {
            List<PawnCapacityUtility.CapacityImpactor> list = new List<PawnCapacityUtility.CapacityImpactor>();
            PawnCapacityUtility.CalculateCapacityLevel(pawn.health.hediffSet, capacity, list);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(__result);
            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is AspectCapacityImpactor)
                    {
                        stringBuilder.AppendLine(string.Format("  {0}", list[i].Readable(pawn)));
                    }
                }
            }
            __result = stringBuilder.ToString();
        }
    }
}
