// ThoughtMemoryHandlerPatches.cs modified by Iron Wolf for Pawnmorph on 12/02/2019 8:31 AM
// last updated 12/02/2019  8:31 AM

using System.Collections.Generic;
using Harmony;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
    internal static class ThoughtHandlerPatches
    {
        [HarmonyPatch(typeof(MemoryThoughtHandler))]
        [HarmonyPatch(nameof(MemoryThoughtHandler.TryGainMemory), new []{typeof(Thought_Memory), typeof(Pawn)})]
        [UsedImplicitly]
        private static class TryAddMemoryPatch
        {
            [HarmonyPrefix]
            [UsedImplicitly]
            private static bool TryGainMemoryPrefix([NotNull] ref Thought_Memory newThought, Pawn otherPawn,
                                                    [NotNull] MemoryThoughtHandler __instance)
            {
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
    }
}