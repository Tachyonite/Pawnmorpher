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
using Pawnmorph.Utilities;
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
            if ((pawn.IsPrisoner && PMNeedDefOf.Joy.neverOnPrisoner) || (pawn.IsSlave && PMNeedDefOf.Joy.neverOnSlave)) return false; 

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

                bool isColonist = (___pawn.Faction?.IsPlayer == true);

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


        [HarmonyPatch(typeof(Pawn_FilthTracker), nameof(Pawn_FilthTracker.Notify_EnteredNewCell))]

        static class PawnFilthTrackerPatches
        {
            static void Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var rpFilthMethod = typeof(FormerHumanUtilities).GetMethod(nameof(FormerHumanUtilities.GetFilthStat),
                                                                    BindingFlags.Static | BindingFlags.Public);

                var filthField =
                    typeof(StatDefOf).GetField(nameof(StatDefOf.FilthRate), BindingFlags.Public | BindingFlags.Static);
                var filthCall =
                    typeof(StatExtension).GetMethod(nameof(StatExtension.GetStatValue),
                                                    BindingFlags.Public | BindingFlags.Static);

                if (filthCall == null)
                {
                    Log.Error($"unable to find {nameof(StatExtension)}.{nameof(StatExtension.GetStatValue)}");
                    return;
                }

                
                var instArr = instructions.ToArray();

                const int len = 3;
                for (int i = 0; i < instArr.Length - len; i++)
                {
                    CodeInstruction  inst1, inst2, inst3;
                    inst1 = instArr[i + 1];
                    inst2 = instArr[i + 2];
                    inst3 = instArr[i + 3]; 

                    if (inst1.opcode != OpCodes.Ldsfld || (FieldInfo) inst1.operand != filthField)  continue;
                    if(inst2.opcode != OpCodes.Ldc_I4_1) continue;
                    if(inst3.opcode != OpCodes.Call || (MethodInfo) inst3.operand != filthCall) continue;

                    inst1.opcode = OpCodes.Call;
                    inst1.operand = rpFilthMethod;
                    inst2.opcode = OpCodes.Nop;
                    inst3.opcode = OpCodes.Nop; 


                    break;
                }

            }
        }


        [HarmonyPatch(typeof(Pawn_PlayerSettings), nameof(Pawn_PlayerSettings.ExposeData)) ]
        static class PawnSettingsTranspiler
        {
            static void Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instArr = instructions.ToArray();
                const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                var firstField = typeof(Thing).GetField(nameof(Thing.def), flags);
                var secondField = typeof(ThingDef).GetField(nameof(ThingDef.race),flags);
                var thirdField = typeof(RaceProperties).GetProperty(nameof(RaceProperties.Roamer), flags).GetMethod;


                const int len = 3;
                for (int i = 0; i < instArr.Length - len; i++)
                {
                    var inst0 = instArr[i];
                    var inst1 = instArr[i + 1];
                    var inst2 = instArr[i + 2];


                    if (inst0.opcode != OpCodes.Ldfld || (FieldInfo) inst0.operand != firstField) continue; 
                    if(inst1.opcode != OpCodes.Ldfld  || (FieldInfo) inst1.operand != secondField) continue;
                    if(inst2.opcode != OpCodes.Callvirt || (MethodInfo) inst2.operand != thirdField) continue;

                    inst0.opcode = OpCodes.Call;
                    inst0.operand = PatchUtilities.RoamerMethod;
                    inst1.opcode = OpCodes.Nop;
                    inst2.opcode = OpCodes.Nop; 

                    break;
                }

            }
        }

        [HarmonyPatch(typeof(Pawn_PlayerSettings), nameof(Pawn_PlayerSettings.SupportsAllowedAreas), MethodType.Getter)]
        static class PawnSettingsSupportsAllowedAreasPatch
        {
            static bool Prefix(Pawn ___pawn, ref bool __result)
            { 
                __result  = !___pawn.IsRoamer();
                return false; 
            }
        }
    }
}
#endif