// PawnPatches.cs created by Iron Wolf for Pawnmorph on 11/27/2019 1:16 PM
// last updated 11/27/2019  1:16 PM

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using JetBrains.Annotations;
using RimWorld;
using Verse;

#pragma warning disable 01591
#if true
namespace Pawnmorph.HPatches
{
    public static class PawnPatches
    {
        [HarmonyPatch(typeof(Pawn)),HarmonyPatch(nameof(Pawn.IsColonistPlayerControlled), MethodType.Getter)]
        internal static class IsColonistPlayerControlledPatch
        {
            [HarmonyPostfix]
            static void MakeSapientAnimalsColonists(Pawn __instance, ref bool __result)
            {
                if (__instance.Faction?.IsPlayer != true) return;
                if (!__instance.RaceProps.Animal) return;
                if (__instance.MentalStateDef != null) return;
                if (!__instance.Spawned) return;
                if (__instance.HostFaction != null) return; 
                var formerHumanHediff = __instance.health.hediffSet.GetFirstHediffOfDef(TfHediffDefOf.TransformedHuman);
                __result = formerHumanHediff?.CurStageIndex == 2; 
            }
        }

        [HarmonyPatch(typeof(Pawn_NeedsTracker)), HarmonyPatch("ShouldHaveNeed")]
        internal static class NeedsTracker_ShouldHaveNeedPatch
        {
            [HarmonyPostfix]
            static void GiveSapientAnimalsNeeds(Pawn_NeedsTracker __instance, Pawn ___pawn, NeedDef nd, ref bool __result)
            {
                if (__result) return;
                if (___pawn.GetFormerHumanStatus() != FormerHumanStatus.Sapient) return; 
                //var isColonist = ___pawn.Faction?.IsPlayer == true;
                if (nd.defName == "Mood")
                {
                    __result = true; 
                }

            }
        }
    }
}
#endif 