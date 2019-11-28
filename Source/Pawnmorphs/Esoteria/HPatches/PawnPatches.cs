// PawnPatches.cs created by Iron Wolf for Pawnmorph on 11/27/2019 1:16 PM
// last updated 11/27/2019  1:16 PM

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using RimWorld;
using Verse;

#pragma warning disable 01591
#if false
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

            private static readonly MethodInfo _getGizmoMethod;

            static GetGizmoPatches()
            {
                _getGizmoMethod = typeof(Pawn_DraftController).GetMethod("DrawGizmos"); 
            }

            [HarmonyPostfix]
            internal static void AddDraftingGizmo(Pawn __instance, ref IEnumerable<Gizmo> __result)
            {
                var  pawn= __instance;

                
                if (pawn.RaceProps.Animal && pawn.Faction?.IsPlayer == true)
                {
                    var lst = __result.ToList();
                    if (pawn.drafter != null && 
                       pawn.health?.hediffSet?.GetFirstHediffOfDef(TfHediffDefOf.TransformedHuman)?.CurStageIndex == 2) //sapient is the 3'rd stage 
                    {
                        
                        lst.AddRange( (IEnumerable<Gizmo>)_getGizmoMethod.Invoke(pawn.drafter, new object[]{}));
                         
                    }
                    __result = lst;
                }
            }
        }
    }
}
#endif 