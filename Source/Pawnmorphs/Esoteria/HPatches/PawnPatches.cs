// PawnPatches.cs created by Iron Wolf for Pawnmorph on 11/27/2019 1:16 PM
// last updated 11/27/2019  1:16 PM

using System.Collections.Generic;
using System.Linq;
using Harmony;
using Verse;

#pragma warning disable 01591
namespace Pawnmorph.HPatches
{
    public static class PawnPatches
    {
        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch(nameof(Pawn.GetGizmos))]
        public static class GetGizmoPatches
        {
            private const string DRAFT_ANIMAL_LABEL = "DraftSapientAnimalLabel";
            private const string DRAFT_ANIMAL_DESCRIPTION = "DraftSapientAnimalDescription"; 
            internal static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
            {

                
                if (__instance.RaceProps.Animal)
                {
                    if(__instance.drafter != null && 
                       __instance.health?.hediffSet?.GetFirstHediffOfDef(TfHediffDefOf.TransformedHuman)?.CurStageIndex == 2) //sapient is the 3'rd stage 
                    {
                        var draftGizmo = new Command_Action
                        {
                            action = () => __instance.drafter.Drafted = !__instance.drafter.Drafted,
                            defaultLabel = DRAFT_ANIMAL_LABEL.Translate(),
                            defaultDesc = DRAFT_ANIMAL_DESCRIPTION.Translate()
                        };
                        var lst = __result.ToList();
                        lst.Insert(0, draftGizmo);
                        __result = lst; 
                    }
                }
            }
        }
    }
}