// PawnPatches.cs created by Iron Wolf for Pawnmorph on 11/27/2019 1:16 PM
// last updated 11/27/2019  1:16 PM

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.Chambers;
using Pawnmorph.DefExtensions;
using RimWorld;
using Verse;

#pragma warning disable 01591
#if true
namespace Pawnmorph.HPatches
{
    public static class PawnCompPatches
    {
        [NotNull]
        readonly 
        private static Dictionary<NeedDef, SapientAnimalNeed> _needLookup = new Dictionary<NeedDef, SapientAnimalNeed>();


        [CanBeNull]
        static SapientAnimalNeed GetSapientAnimalNeed([NotNull] NeedDef need)
        {
            if (_needLookup.TryGetValue(need, out var ext))
            {
                return ext; 
            }

            ext = need.GetModExtension<SapientAnimalNeed>();
            _needLookup[need] = ext;
            return ext; 
        }



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
        private static FieldInfo DefField { get; } = typeof(Thing).GetField(nameof(Thing.def));
        private static FieldInfo PawnField { get; } = typeof(Pawn_TrainingTracker).GetField("pawn");

        private static MethodInfo CanDecayMethod { get; } =
            typeof(TrainableUtility).GetMethod(nameof(TrainableUtility.TamenessCanDecay));

        private static MethodInfo CanDecayReplacementMethod { get; } =
            typeof(FormerHumanUtilities).GetMethod(nameof(FormerHumanUtilities.TamenessCanDecay));


        //patch to disable tameness decay for sapient and mostly sapient former humans 
        [HarmonyPatch(typeof(Pawn_TrainingTracker))]
        internal static class TrainingTrackerPatches
        {
            [HarmonyPatch(nameof(Pawn_TrainingTracker.TrainingTrackerTickRare)), HarmonyPrefix]
            static bool DisableForSapientAnimalsPrefix([NotNull] Pawn ___pawn, ref int ___countDecayFrom)
            {
                if (___pawn.GetIntelligence() == Intelligence.Humanlike)
                {
                    ___countDecayFrom += 250;
                    return false; 
                }

                return true; 
            }
        }



        [HarmonyPatch(typeof(Pawn_NeedsTracker))]
        static class NeedsTrackerPatches
        {
            [HarmonyPatch("ShouldHaveNeed")]
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

                if (moodIsEnabled)
                {
                    var defExt = GetSapientAnimalNeed(nd);
                    if (defExt != null)
                    {
                        __result = !defExt.mustBeColonist || ___pawn.IsColonist;
                    }
                }

                if (__result) __result = nd.IsValidFor(___pawn);
            }

            [HarmonyPatch(nameof(Pawn_NeedsTracker.NeedsTrackerTick)), HarmonyPrefix]
            static bool DisableIfInChamberPatch(Pawn ___pawn)
            {
                if (___pawn?.IsHashIntervalTick(150) != true)
                {
                    return true; 
                }

                //needs should not tick while in the chamber 
                IThingHolder owner = ___pawn.holdingOwner?.Owner;
                if (owner == null) return true; 
                return !(owner is MutaChamber);
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
                    if (buildingBed == null) return true; 
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