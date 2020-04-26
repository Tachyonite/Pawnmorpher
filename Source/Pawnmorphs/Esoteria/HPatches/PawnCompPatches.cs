// PawnPatches.cs created by Iron Wolf for Pawnmorph on 11/27/2019 1:16 PM
// last updated 11/27/2019  1:16 PM

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using RimWorld;
using Verse;

#pragma warning disable 01591
#if true
namespace Pawnmorph.HPatches
{
    public static class PawnCompPatches
    {
       

        [HarmonyPatch(typeof(Pawn_NeedsTracker)), HarmonyPatch("ShouldHaveNeed")]
        internal static class NeedsTracker_ShouldHaveNeedPatch
        {
            [HarmonyPostfix]
            static void GiveSapientAnimalsNeeds(Pawn_NeedsTracker __instance, Pawn ___pawn, NeedDef nd, ref bool __result)
            {
                if (nd == PMNeedDefOf.SapientAnimalControl)
                {
                    __result = Need_Control.IsEnabledFor(___pawn);
                    return;
                }


                if (__result)
                {
                    __result = nd.IsValidFor(___pawn);
                    return;
                }
                if (___pawn?.IsFormerHuman() != true || ___pawn.GetIntelligence() == Intelligence.Animal) return;
              
                
                var isColonist = ___pawn.Faction?.IsPlayer == true;
                if (nd.defName == "Mood")
                {
                    __result = true; 
                }else if (nd.defName == "Joy" && isColonist)
                    __result = true; 

            }
        }

        [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.BodyAngle))]
        static class PawnRenderAnglePatch
        {
            static bool Prefix(ref float __result, [NotNull] Pawn ___pawn)
            {
                if (___pawn.IsSapientFormerHuman() && ___pawn.GetPosture() == PawnPosture.LayingInBed)
                {
                    Building_Bed buildingBed = ___pawn.CurrentBed();
                    Rot4 rotation = buildingBed.Rotation;
                    rotation.AsInt += Rand.ValueSeeded(___pawn.thingIDNumber) > 0.5 ?  1 : 3;
                    __result = rotation.AsAngle;
                    return false; 
                }

                return true; 
            }
        }
    }


}
#endif