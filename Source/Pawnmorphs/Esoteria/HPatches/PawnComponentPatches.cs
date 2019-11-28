// PawnComponentPatches.cs created by Iron Wolf for Pawnmorph on 11/27/2019 1:00 PM
// last updated 11/27/2019  1:00 PM

using Harmony;
using RimWorld;
using Verse;

#pragma warning disable 01591

#if false
namespace Pawnmorph.HPatches
{
    public static class PawnComponentPatches
    {
        [HarmonyPatch(typeof(PawnComponentsUtility))]
        [HarmonyPatch("AddAndRemoveDynamicComponents")]
        public static class AddRemoveComponentsPatch
        {
            internal static void Postfix(Pawn pawn)
            {
                if (pawn.RaceProps.Animal && pawn.Faction?.IsPlayer == true)
                {
                    Hediff formerHumanHediff = pawn.health.hediffSet.GetFirstHediffOfDef(TfHediffDefOf.TransformedHuman);
                    if (formerHumanHediff?.CurStageIndex == 2 && pawn.drafter == null)
                    {
                        //add the drafter and equipment components 
                        //if 
                        pawn.drafter = new Pawn_DraftController(pawn);
                        pawn.equipment = new Pawn_EquipmentTracker(pawn);
                    }

                    //else if (formerHumanHediff?.CurStageIndex < 2 && pawn.drafter != null)
                    //{
                    //    //remove the drafter component if the animal is now feral 
                    //    pawn.drafter.Drafted = false;
                    //    pawn.drafter = null;
                    //    if(pawn.MapHeld != null)
                    //        pawn.equipment?.DropAllEquipment(pawn.PositionHeld, pawn.Faction?.IsPlayer != true);
                    //    else 
                    //        pawn.equipment?.DestroyAllEquipment();
                    //    pawn.equipment = null; 
                    //}
                }
            }
        }
    }
}
#endif