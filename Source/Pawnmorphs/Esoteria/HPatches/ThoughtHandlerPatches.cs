﻿// ThoughtMemoryHandlerPatches.cs modified by Iron Wolf for Pawnmorph on 12/02/2019 8:31 AM
// last updated 12/02/2019  8:31 AM

using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
    internal static class ThoughtHandlerPatches
    {
        [HarmonyPatch(typeof(MemoryThoughtHandler))]
        [HarmonyPatch(nameof(MemoryThoughtHandler.TryGainMemory), typeof(Thought_Memory), typeof(Pawn))]
        [UsedImplicitly]
        private static class TryAddMemoryPatch
        {
            [HarmonyPrefix]
            [UsedImplicitly]
            private static bool TryGainMemoryPrefix([NotNull] ref Thought_Memory newThought, Pawn otherPawn,
                                                    [NotNull] MemoryThoughtHandler __instance)
            {
                //need to handle got some lovin thought first 
                if (newThought.def == ThoughtDefOf.GotSomeLovin && (__instance.pawn.IsFormerHuman()
                                                                 || otherPawn?.IsFormerHuman() == true))
                {
                    var sub = (Thought_Memory) ThoughtMaker.MakeThought(PMThoughtDefOf.SapientAnimalGotSomeSnuggling);
                    sub.moodPowerFactor = newThought.moodPowerFactor / 1.5f;
                    newThought = sub;
                    return true; 
                }


                newThought = newThought.GetSubstitute(__instance.pawn);

                return true;
            }
        }

        [HarmonyPatch(typeof(SituationalThoughtHandler))]
        [HarmonyPatch("TryCreateThought")]
        internal static class TryAddSituationalThoughtPatch
        {
            internal static bool TryCreateThoughtPrefix([NotNull] ref ThoughtDef def,
                                                        [NotNull] SituationalThoughtHandler __instance)
            {
                
                var tGroup = def.GetModExtension<ThoughtGroupDefExtension>();
                if (tGroup == null) return true; //quit early if there is no def extension 

                ThoughtDef nDef = def.GetSubstitute(__instance.pawn);
                if (nDef == def) return true;
                def = nDef;
                return !Traverse.Create(__instance).Field("tmpCachedThoughts").GetValue<HashSet<ThoughtDef>>().Contains(def);
            }
        }

        [HarmonyPatch(typeof(ThoughtWorker_PsychologicallyNude))]
        static class NudePatches
        {
            [HarmonyPatch("CurrentStateInternal"), HarmonyPostfix]
            static void DisableForSapientHumanoids(Pawn p, ref ThoughtState __result)
            {
                if (__result.Active && p?.HasSapienceState() == true) __result = false;
            }
        }

        [HarmonyPatch(typeof(ThoughtWorker_NudistNude))]
        static class NudistPatches
        {
            [HarmonyPatch("CurrentStateInternal"), HarmonyPostfix]
            static void DisableForSapientHumanoids(Pawn p, ref ThoughtState __result)
            {
                if (__result.Active && p?.HasSapienceState() == true) __result = false;
            }
        }
    }
}