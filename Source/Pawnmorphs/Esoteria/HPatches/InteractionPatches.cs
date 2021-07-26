// InteractionPatches.cs modified by Iron Wolf for Pawnmorph on 12/10/2019 6:31 PM
// last updated 12/10/2019  6:31 PM

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.DefExtensions;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
    [StaticConstructorOnStartup]
    internal static class InteractionPatches
    {
        private static readonly FieldInfo _pawnNeedField;

        static InteractionPatches()
        {
            _pawnNeedField = typeof(Need).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private static float GetEffectiveNeed(Need_Suppression need)
        {
            var p = (Pawn) _pawnNeedField.GetValue(need);
            float? sLevel = p.GetSapienceLevel();

            if (sLevel == null) return need.MaxLevel;
            return (FormerHumanUtilities.GetSapienceWillDebuff(sLevel.Value) + 1) * need.MaxLevel;
        }

        private static float GetEffectiveResistance(Pawn p, float mulVal)
        {
            float? qSapience = p.GetSapienceLevel();
            if (qSapience == null) return p.guest.resistance - mulVal;

            float s = FormerHumanUtilities.GetSapienceWillDebuff(qSapience.Value) + 1;

            return p.guest.resistance - mulVal * s;
        }


        [HarmonyPatch(typeof(Pawn_InteractionsTracker))]
        [HarmonyPatch(nameof(Pawn_InteractionsTracker.TryInteractWith))]
        private static class TryInteractWithPatch
        {
            [HarmonyPostfix]
            private static void AddInteractionThoughts([NotNull] Pawn recipient, [NotNull] InteractionDef intDef, bool __result)
            {
                if (!__result) return;
                if ((recipient.IsFormerHuman() || recipient.GetSapienceState()?.StateDef == SapienceStateDefOf.Animalistic)
                 && recipient.needs?.mood != null)
                {
                    ThoughtDef
                        memory = intDef.GetModExtension<InstinctEffector>()
                                      ?.thought; //hacky, should come up with a better solution eventually 
                    if (memory == null) return;

                    if (DebugLogUtils.ShouldLog(LogLevel.Messages))
                    {
                        string msg = $"giving {recipient.Name} memory {memory.defName}";
                        Log.Message(msg);
                    }


                    //social thoughts to? 
                    recipient.TryGainMemory(memory);
                }
            }

            [HarmonyPrefix]
            private static bool SubstituteInteraction(Pawn recipient, ref InteractionDef intDef, Pawn ___pawn)
            {
                var ext = intDef.GetModExtension<InteractionGroupExtension>();
                InteractionDef alt = ext?.TryGetAlternativeFor(___pawn, recipient);
                if (alt != null)
                {
                    if (DebugLogUtils.ShouldLog(LogLevel.Messages))
                    {
                        string msg = $"substituting {alt.defName} for {intDef.defName} on {___pawn.Name} -> {recipient.Name}";
                        Log.Message(msg);
                    }


                    intDef = alt;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(InteractionUtility))]
        [HarmonyPatch(nameof(InteractionUtility.CanReceiveRandomInteraction))]
        private static class SapientAnimalsRandomInteractionPatch
        {
            [HarmonyPrefix]
            private static bool SapientAnimalPatch([NotNull] Pawn p, ref bool __result)
            {
                if (p.IsFormerHuman() && p.needs?.mood != null)
                {
                    __result = InteractionUtility.CanReceiveInteraction(p) && !p.Downed && !p.InAggroMentalState;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(InteractionWorker_RecruitAttempt), "Interacted")]
        private static class ResistanceSapienceInfluenceP
        {
            [HarmonyTranspiler]
            private static IEnumerable<CodeInstruction> ResistanceSapienceInfluenceTranspilerPatch(
                IEnumerable<CodeInstruction> inst)
            {
                CodeInstruction[] instArr = inst.ToArray();

                const int len = 4;
                var sliceArr = new CodeInstruction[len];
                FieldInfo guestField = typeof(Pawn).GetField(nameof(Pawn.guest), BindingFlags.Public | BindingFlags.Instance);
                FieldInfo resField =
                    typeof(Pawn_GuestTracker).GetField(nameof(Pawn_GuestTracker.resistance),
                                                       BindingFlags.Public | BindingFlags.Instance);
                MethodInfo subMethod =
                    typeof(InteractionPatches).GetMethod("GetEffectiveResistance", BindingFlags.NonPublic | BindingFlags.Static);

                for (var i = 0; i < instArr.Length - len; i++)
                {
                    for (var j = 0; j < len; j++) sliceArr[j] = instArr[i + j];


                    if (sliceArr[0].opcode != OpCodes.Ldfld || (FieldInfo) sliceArr[0].operand != guestField) continue;
                    if (sliceArr[1].opcode != OpCodes.Ldfld || (FieldInfo) sliceArr[1].operand != resField) continue;
                    if (sliceArr[2].opcode != OpCodes.Ldloc_S) continue;
                    if (sliceArr[3].opcode != OpCodes.Sub) continue;

                    sliceArr[0].opcode = OpCodes.Nop;
                    sliceArr[1].opcode = OpCodes.Nop;
                    sliceArr[3].opcode = OpCodes.Call;
                    sliceArr[3].operand = subMethod;
                    break;
                }

                return instArr;
            }
        }


        [HarmonyPatch(typeof(InteractionWorker_Suppress), "Interacted")]
        private static class ResistanceSapienceInfluenceSuppression
        {
            [HarmonyTranspiler]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> inst)
            {
                CodeInstruction[] codeArr = inst.ToArray();
                const int len = 5;
                var sliceArr = new CodeInstruction[len];

                MethodInfo callVirtMInfo = typeof(Need)
                                          .GetProperty(nameof(Need.MaxLevel), BindingFlags.Public | BindingFlags.Instance)
                                          .GetMethod;
                MethodInfo replaceMethod =
                    typeof(InteractionPatches).GetMethod("GetEffectiveNeed", BindingFlags.Static | BindingFlags.NonPublic);


                for (var i = 0; i < codeArr.Length - len; i++)
                {
                    for (var j = 0; j < len; j++) sliceArr[j] = codeArr[i + j];

                    if (sliceArr[0].opcode != OpCodes.Mul) continue;
                    if (sliceArr[1].opcode != OpCodes.Ldloc_0) continue;
                    if (sliceArr[2].opcode != OpCodes.Callvirt || (MethodInfo) sliceArr[2].operand != callVirtMInfo) continue;
                    if (sliceArr[3].opcode != OpCodes.Mul) continue;


                    sliceArr[2].opcode = OpCodes.Call;
                    sliceArr[2].operand = replaceMethod;
                }

                return codeArr;
            }
        }
    }
}