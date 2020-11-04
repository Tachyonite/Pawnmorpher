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
            
            [HarmonyPatch(nameof(Pawn_TrainingTracker.TrainingTrackerTickRare))]
            [HarmonyTranspiler]
            private static IEnumerable<CodeInstruction> DisableDecayPatch(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> instructionArr = instructions.ToList();
                //patch cannot happen on the last instruction 
                for (var i = 0; i < instructionArr.Count - 3; i++)
                {
                    // looking for 
                    /* IL_0136: ldarg.0      
    IL_0137: ldfld        class Verse.Pawn RimWorld.Pawn_TrainingTracker::pawn
    IL_013c: ldfld        class Verse.ThingDef Verse.Thing::def
    IL_0141: call         bool RimWorld.TrainableUtility::TamenessCanDecay(class Verse.ThingDef)
    IL_0146: brtrue.s     IL_0159
                     *
                     */


                    CodeInstruction inst = instructionArr[i];
                    if (inst?.opcode != OpCodes.Ldfld || inst.operand as FieldInfo != PawnField) continue;
                    CodeInstruction inst1 = instructionArr[i + 1];
                    if (inst1?.opcode == OpCodes.Ldfld && inst1.operand as FieldInfo == DefField)
                    {
                        CodeInstruction inst2 = instructionArr[i + 2];
                        if (inst2?.opcode == OpCodes.Call && inst2.operand as MethodInfo == CanDecayMethod)
                        {
                            //do the patch 
                            //replace the call to FormerHumanUtilities.TamenessCanDecay
                            //and remove the unneeded instruction 
                            inst1.opcode = OpCodes.Call;
                            inst1.operand = CanDecayReplacementMethod;
                            inst2.opcode = OpCodes.Nop;
                            inst2.operand = null;
                        }
                    }
                }

                return instructionArr;
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

                if (nd == PMNeedDefOf.Mood || nd == PMNeedDefOf.Comfort || nd == PMNeedDefOf.Beauty)
                    __result = moodIsEnabled;


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