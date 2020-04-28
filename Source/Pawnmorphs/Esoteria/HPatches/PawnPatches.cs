// PawnPatches.cs created by Iron Wolf for Pawnmorph on 02/19/2020 5:41 PM
// last updated 04/26/2020  9:22 AM

using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
    [HarmonyPatch(typeof(Pawn))]
    static class PawnPatches
    {
        [HarmonyPatch(nameof(Pawn.CombinedDisabledWorkTags), MethodType.Getter), HarmonyPostfix]
        static void FixCombinedDisabledWorkTags(ref WorkTags __result, [NotNull] Pawn __instance)
        {
            var hediffs = __instance.health?.hediffSet?.hediffs;       
            if (hediffs == null) return;

            foreach (Hediff hediff in hediffs)
            {
                if (hediff is IWorkModifier wM)
                {
                    __result |= ~wM.AllowedWorkTags;
                }
                else
                {
                    foreach (HediffStage hediffStage in hediff.def.stages.MakeSafe())
                    {
                        if (hediffStage is IWorkModifier sWM)
                        {
                            __result |= ~sWM.AllowedWorkTags; 
                        }
                    }
                }
            }
        }

        [HarmonyPatch(nameof(Pawn.IsColonist), MethodType.Getter), HarmonyPrefix]
        static bool FixIsColonist(ref bool __result, [NotNull] Pawn __instance)
        {
            var sTracker = __instance.GetSapienceTracker();
            if (sTracker?.CurrentState != null)
            {
                __result = __instance.Faction == Faction.OfPlayer && sTracker.CurrentIntelligence == Intelligence.Humanlike;
                return false;
            }

            return true; 
        }

        [HarmonyPatch(nameof(Pawn.WorkTypeIsDisabled)), HarmonyPostfix]
        static void FixWorkTypeIsDisabled(ref bool __result, [NotNull] WorkTypeDef w, [NotNull] Pawn __instance)
        {
            if (__result) return;

            List<Hediff> hediffs = __instance.health?.hediffSet?.hediffs;
            if (hediffs == null) return;

            foreach (Hediff hediff in hediffs)
                if (hediff is IWorkModifier wM)
                {
                    if (wM.WorkTypeFilter == null) continue;
                    if (!wM.WorkTypeFilter.PassesFilter(w))
                    {
                        __result = true;
                        return;
                    }
                }
                else
                {
                    foreach (HediffStage hediffStage in hediff.def.stages.MakeSafe())
                        if (hediffStage is IWorkModifier sWM)
                            if (sWM.WorkTypeFilter?.PassesFilter(w) == false)
                            {
                                __result = true;
                                return;
                            }
                }
        }
    }
}