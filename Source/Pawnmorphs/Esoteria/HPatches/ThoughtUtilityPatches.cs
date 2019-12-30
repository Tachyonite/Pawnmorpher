// ThoughtUtilityPatches.cs modified by Iron Wolf for Pawnmorph on 12/02/2019 7:45 AM
// last updated 12/02/2019  7:45 AM

using System.Linq;
using Harmony;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
    internal static class ThoughtUtilityPatches
    {
        [HarmonyPatch(typeof(ThoughtUtility))]
        [HarmonyPatch(nameof(ThoughtUtility.CanGetThought))]
        private static class CanGetThoughtPatch
        {
            [HarmonyPostfix]
            [UsedImplicitly]
            private static void CanGetThoughtPostfix([NotNull] Pawn pawn, [NotNull] ThoughtDef def, ref bool __result)
            {
                var tracker = pawn.GetAspectTracker();
                if (tracker != null)
                {
                    foreach (Aspect aspect in tracker)
                    {
                        if (aspect.NullifiedThoughts.Contains(def))
                        {
                            __result = false;
                            break;
                        } 
                    }
                }

                var tGroup = def.GetModExtension<ThoughtGroupDefExtension>();

                if (tGroup != null && !__result)
                    //if the default thought is invalid and it has a thought group extension check if any of the specific thoughts are valid 
                    foreach (ThoughtDef tGroupThought in tGroup.thoughts)
                    {
                        if (tGroupThought.HasModExtension<ThoughtGroupDefExtension>())
                        {
                            Log.Warning($"thought in {def.defName} has thought {tGroupThought.defName} with thought group extension! this is not currently supported as it may cause infinite recursion");
                            continue;
                        }

                        if (ThoughtUtility.CanGetThought(pawn, tGroupThought))
                        {
                            __result = true;
                            return;
                        }
                    }

                if (__result) __result = def.IsValidFor(pawn);
            }
        }
    }
}