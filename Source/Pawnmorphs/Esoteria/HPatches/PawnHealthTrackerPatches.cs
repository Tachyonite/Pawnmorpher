﻿using HarmonyLib;
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
        [HarmonyPatch(nameof(Pawn_HealthTracker.LethalDamageThreshold), MethodType.Getter), HarmonyPostfix]
        static float LethalDamageThresholdPostfix(float __result, [NotNull] Pawn ___pawn)
        {
            if (___pawn.RaceProps?.Humanlike == true)
            {
                return __result * ___pawn.BodySize;
            }
            
            return __result;
        }
    }
}
