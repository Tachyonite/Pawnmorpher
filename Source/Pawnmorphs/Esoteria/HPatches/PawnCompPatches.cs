// PawnPatches.cs created by Iron Wolf for Pawnmorph on 11/27/2019 1:16 PM
// last updated 11/27/2019  1:16 PM

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
        private static bool MoodIsEnabled([NotNull] Pawn pawn)
        {
            bool val;
            SapienceLevel? qSapience = pawn.GetQuantizedSapienceLevel();
            if (!pawn.HasSapienceState() || qSapience == null)
                val = pawn.RaceProps.Humanlike;
            else val = qSapience.Value < SapienceLevel.Feral;

            //Log.Message($"mood is enabled for {pawn.LabelShort}: {val}");

            return val;
        }


        [HarmonyPatch(typeof(Pawn_NeedsTracker))]
        [HarmonyPatch("ShouldHaveNeed")]
        internal static class NeedsTracker_ShouldHaveNeedPatch
        {
            [HarmonyPostfix]
            private static void GiveSapientAnimalsNeeds(Pawn_NeedsTracker __instance, Pawn ___pawn, NeedDef nd, ref bool __result)
            {
                if (nd == PMNeedDefOf.SapientAnimalControl)
                {
                    __result = Need_Control.IsEnabledFor(___pawn);
                    return;
                }

                bool isColonist = ___pawn.Faction?.IsPlayer == true;

                bool moodIsEnabled = MoodIsEnabled(___pawn);
                if (nd == PMNeedDefOf.Joy && isColonist)
                    __result = moodIsEnabled;

                if (nd == PMNeedDefOf.Mood || nd == PMNeedDefOf.Comfort || nd == PMNeedDefOf.Beauty)
                    __result = moodIsEnabled;

                if (!__result)
                {
                    
                }


                if (__result) __result = nd.IsValidFor(___pawn);
            }
        }

        [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.BodyAngle))]
        private static class PawnRenderAnglePatch
        {
            private static bool Prefix(ref float __result, [NotNull] Pawn ___pawn)
            {
                if (___pawn.IsSapientFormerHuman() && ___pawn.GetPosture() == PawnPosture.LayingInBed)
                {
                    Building_Bed buildingBed = ___pawn.CurrentBed();
                    Rot4 rotation = buildingBed.Rotation;
                    rotation.AsInt += Rand.ValueSeeded(___pawn.thingIDNumber) > 0.5 ? 1 : 3;
                    __result = rotation.AsAngle;
                    return false;
                }

                return true;
            }
        }
    }
}
#endif