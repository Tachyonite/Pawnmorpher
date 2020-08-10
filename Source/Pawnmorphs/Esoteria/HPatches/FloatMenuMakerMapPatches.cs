// FloatMenuMakerMapPatches.cs modified by Iron Wolf for Pawnmorph on //2019 
// last updated 08/25/2019  7:11 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
#pragma warning disable 1591
namespace Pawnmorph
{
    internal static class FloatMenuMakerMapPatches
    {
        [HarmonyPatch(typeof(FloatMenuMakerMap))]
        [HarmonyPatch("AddHumanlikeOrders")]
        internal static class AddHumanlikeOrdersPatch
        {
            [HarmonyPrefix]
            private static bool Prefix_AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
            {
                if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                    foreach (LocalTargetInfo localTargetInfo3 in GenUI.TargetsAt_NewTemp(clickPos, TargetingParameters.ForRescue(pawn), true))
                    {
                        LocalTargetInfo localTargetInfo4 = localTargetInfo3;
                        var victim = (Pawn) localTargetInfo4.Thing;
                        MutagenDef mutagen = MutagenDefOf.MergeMutagen;
                        if (mutagen.CanTransform(victim)
                         && pawn.CanReserveAndReach(victim, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, true)
                         && Building_MutagenChamber.FindCryptosleepCasketFor(victim, pawn, true) != null)
                        {
                            string text4 = "CarryToChamber".Translate(localTargetInfo4.Thing.LabelCap, localTargetInfo4.Thing);
                            JobDef jDef = Mutagen_JobDefOf.CarryToMutagenChamber;
                            Action action3 = delegate
                            {
                                Building_MutagenChamber building_chamber =
                                    Building_MutagenChamber.FindCryptosleepCasketFor(victim, pawn);
                                if (building_chamber == null)
                                    building_chamber = Building_MutagenChamber.FindCryptosleepCasketFor(victim, pawn, true);
                                if (building_chamber == null)
                                {
                                    Messages.Message("CannotCarryToChamber".Translate() + ": " + "NoChamber".Translate(), victim,
                                                     MessageTypeDefOf.RejectInput, false);
                                    return;
                                }

                                var job = new Job(jDef, victim, building_chamber);
                                job.count = 1;
                                pawn.jobs.TryTakeOrderedJob(job);
                            };
                            string label = text4;
                            Action action2 = action3;
                            Pawn revalidateClickTarget = victim;
                            opts.Add(FloatMenuUtility
                                        .DecoratePrioritizedTask(new FloatMenuOption(label, action2, MenuOptionPriority.Default, null, revalidateClickTarget),
                                                                 pawn, victim));
                        }
                    }

                return true;
            }
        }
#if true
        [HarmonyPatch(typeof(FloatMenuMakerMap))]
        [HarmonyPatch("CanTakeOrder")]
        internal static class CanTakeOrderPatch
        {
            [HarmonyPostfix]
            private static void MakePawnControllable(Pawn pawn, ref bool __result)
            {
                if (pawn?.Faction?.IsPlayer != true) return;

                if (!pawn.RaceProps.Animal) return;
                var sTracker = pawn.GetSapienceTracker();
                if (sTracker == null) return;

                switch (sTracker.CurrentIntelligence)
                {
                    case Intelligence.Animal:
                        return;
                    case Intelligence.ToolUser:
                    case Intelligence.Humanlike:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


                __result = true;
            }
        }
   
#endif
        [HarmonyPatch(typeof(FloatMenuMakerMap), nameof(FloatMenuMakerMap.ChoicesAtFor))]
        static class AddHumanlikeOrdersToSA
        {

            [NotNull]
            private static readonly MethodInfo _isToolUser = typeof(FormerHumanUtilities).GetMethod(nameof(FormerHumanUtilities.IsHumanlike), new [] {typeof(Pawn)});

            [NotNull] private static readonly MethodInfo _targetMethodSig =
                typeof(Pawn).GetProperty(nameof(Pawn.RaceProps)).GetGetMethod(); 
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList(); //convert the code instructions to a list so we can do 2 at a time 

                for (var i = 0; i < codes.Count - 1; i++)
                {
                    int j = i + 1;
                    CodeInstruction instI = codes[i];
                    //need to be more specific because the patched method is longer, don't want to patch stuff we don't intend to 
                    if (instI.opcode == OpCodes.Callvirt  && (MethodInfo) codes[i].operand == _targetMethodSig  && codes[j].opcode == OpCodes.Callvirt)
                    {
                        instI.opcode =
                            OpCodes.Call; //replace the callVirt to get_RaceProps with call to FormerHumanUtilities.IsToolUser 
                        instI.operand = _isToolUser; //set the method that the call op is going to call 
                        codes[j].opcode = OpCodes.Nop; //replace the second  callVirt to a No op so we don't fuck up the stack 
                    }
                }

                return codes;
            }
        }

    }
}