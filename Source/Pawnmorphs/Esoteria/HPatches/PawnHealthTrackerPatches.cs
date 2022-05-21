using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.HPatches
{
    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    public static class PawnHealthTrackerPatches
    {
        private static AccessTools.FieldRef<Pawn_HealthTracker, Pawn> _pawnFieldRef;
        private static System.Reflection.FieldInfo _fieldInfo;

        static PawnHealthTrackerPatches()
        {
            _pawnFieldRef = AccessTools.FieldRefAccess<Pawn_HealthTracker, Pawn>("pawn");
            _fieldInfo = AccessTools.Field(typeof(Pawn_HealthTracker), "pawn");
        }


        [HarmonyPatch(nameof(Pawn_HealthTracker.LethalDamageThreshold), MethodType.Getter), HarmonyPostfix]
        static float LethalDamageThresholdPostfix(float __result, [NotNull] Pawn_HealthTracker __instance)
        {
            Pawn pawn = _pawnFieldRef.Invoke(__instance);

            if (pawn.RaceProps?.Humanlike == true)
            {
                float? stat = Utilities.StatsUtility.GetStat(pawn, PMStatDefOf.PM_BodySize, 200);
                if (stat.HasValue)
                {
                    return __result * stat.Value;
                }
            }
            
            return __result;
        }

    }
}
