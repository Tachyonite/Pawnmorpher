// InteractionPatches.cs modified by Iron Wolf for Pawnmorph on 12/10/2019 6:31 PM
// last updated 12/10/2019  6:31 PM

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
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


        [HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.TryInteractWith))]
        private static class TryInteractWithPatch
        {
            [HarmonyPostfix]
            private static void AddInteractionThoughts(Pawn ___pawn, [NotNull] Pawn recipient, [NotNull] InteractionDef intDef, bool __result)
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
                    
                        string msg = $"substituting {alt.defName} for {intDef.defName} on {___pawn.Name} -> {recipient.Name}";
                        Log.Message(msg);
                    


                    intDef = alt;
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

        [NotNull]
        private static readonly List<Pawn> _workingList = new List<Pawn>();

#if true

       

        [NotNull]
        private static readonly StringBuilder _dbgBuilder = new StringBuilder();
        [HarmonyPatch(typeof(Pawn_InteractionsTracker), "TryInteractRandomly")]
        static class DebugInteractionPatch
        {
            static void Postfix(Pawn_InteractionsTracker __instance, Pawn ___pawn,ref  bool __result)
            {
                _workingList.Clear();
                _dbgBuilder.Clear();
                if (___pawn?.IsFormerHuman() == true)
                {
                    if (InteractionUtility.CanInitiateRandomInteraction(___pawn) && !__instance.InteractedTooRecentlyToInteract())
                    {
                        List<Pawn> collection = ___pawn.Map?.mapPawns?.SpawnedPawnsInFaction(___pawn.Faction);
                        if ((collection?.Count ?? 0) == 0)
                        {
                            return; 
                        }

                        _workingList.AddRange(collection);
                        List<InteractionDef> allDefsListForReading = DefDatabase<InteractionDef>.AllDefsListForReading;
                        for (int i = 0; i < _workingList.Count; i++)
                        {
                            Pawn p = _workingList[i];
                            if (p != ___pawn)  
                            {
                                if (__instance.CanInteractNowWith(p))
                                {
                                    if (InteractionUtility.CanReceiveRandomInteraction(p))
                                    {
                                        if (!___pawn.HostileTo(p))
                                        {
                                            var tups = allDefsListForReading
                                                      .Select((InteractionDef x) => (x, GetWeight(x, p)))
                                                      .Where(t => t.Item2 > 0); 

                                            var elem = tups.TryRandomElementByWeight(t => t.Item2, out var selTup);

                                            if (elem)
                                            {
                                                __result = __instance.TryInteractWith(p, selTup.x); 
                                                break;
                                            }

                                        }
                                        else { }
                                    }
                                    else
                                    {
                                    }
                                }
                                else
                                {
                                }
                            }
                        }

                        float GetWeight(InteractionDef x, Pawn p)
                        {
                            return (!__instance.CanInteractNowWith(p, x)) ? 0f : x.Worker.RandomSelectionWeight(___pawn, p);
                        }

             
                        
                        if(_dbgBuilder.Length > 0)
                            Log.Message(_dbgBuilder.ToString());

                    }

                    if(__result) Log.Warning($"{___pawn.Name} interacted!");
                }


            }
        }
#endif

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