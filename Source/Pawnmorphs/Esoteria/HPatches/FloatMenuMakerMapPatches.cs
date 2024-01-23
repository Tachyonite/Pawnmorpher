// FloatMenuMakerMapPatches.cs modified by Iron Wolf for Pawnmorph on //2019 
// last updated 08/25/2019  7:11 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.Chambers;
using Pawnmorph.Utilities;
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
			private static ValueTuple<Pawn, bool, string, string> _pawnCanTransformCache = (null, false, string.Empty, string.Empty);

			[HarmonyPrefix]
			private static bool Prefix_AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
			{
				if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
				{
					IEnumerable<LocalTargetInfo> targets = GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), true);
					if (targets != null)
					{
						foreach (LocalTargetInfo localTargetInfo3 in targets)
						{
							LocalTargetInfo localTargetInfo4 = localTargetInfo3;
							var victim = (Pawn)localTargetInfo4.Thing;
							MutagenDef mutagen = MutagenDefOf.MergeMutagen;

							// As long as the target remains unchanged, then cache mutability state.
							if (_pawnCanTransformCache.Item1 != victim)
								_pawnCanTransformCache = (victim, 
														  mutagen.CanTransform(victim) && pawn.CanReserveAndReach(victim, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, true),
														  "CarryToChamber".Translate(victim.LabelCap, victim),
														  "PMCarryToChamberMerge".Translate(victim.LabelCap, victim)
														  );

							if (_pawnCanTransformCache.Item2)
							{
								CarryToChamber(pawn, opts, victim, _pawnCanTransformCache.Item3, ValueTuple.Create(ChamberUse.Mutation, ChamberUse.Tf));
								CarryToChamber(pawn, opts, victim, _pawnCanTransformCache.Item4, ValueTuple.Create(ChamberUse.Merge, ChamberUse.Merge));
							}
						}
					}
				}

				return true;
			}

			private static void CarryToChamber(Pawn pawn, List<FloatMenuOption> opts, Pawn victim, string label, ValueTuple<ChamberUse, ChamberUse> useType)
			{
				// Is there any chambers including reserved that fit the requirements?
				var mergeChamber = MutaChamber.FindMutaChamberFor(victim, pawn, true, useType);
				if (mergeChamber != null)
				{
					Action action = delegate
					{
						// Is there any chambers that isn't already reserved? then prioritise that.
						var building_chamber = MutaChamber.FindMutaChamberFor(victim, pawn, false, useType);

						// If none is found then check for any reserved again.
						// Chambers might no longer be avaiable between right clicking and selecting the option.
						if (building_chamber == null)
							building_chamber = MutaChamber.FindMutaChamberFor(victim, pawn, true, useType); 
						
						if (building_chamber == null)
						{
							Messages.Message("CannotCarryToChamber".Translate() + ": " + "NoChamber".Translate(), victim, MessageTypeDefOf.RejectInput, false);
							return;
						}

						var job = new Job(Mutagen_JobDefOf.CarryToMutagenChamber, victim, building_chamber);
						job.count = 1;
						pawn.jobs.TryTakeOrderedJob(job);
					};
					opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, MenuOptionPriority.Default, null, victim),
														 pawn, victim));
				}
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


				__result = pawn.MentalState == null;
			}
		}

#endif

		[HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders", typeof(Vector3), typeof(Pawn), typeof(List<FloatMenuOption>))]
		static class AddHumanlikeOrders
		{
			[NotNull]
			private static readonly MethodInfo originalMethod = AccessTools.DeclaredPropertyGetter(typeof(Thing), nameof(Thing.IngestibleNow));

			[NotNull]
			private static readonly MethodInfo proxyMethod = typeof(AddHumanlikeOrders).GetMethod(nameof(IngestibleNowProxy));

			public static bool IngestibleNowProxy(Thing thing, Pawn pawn)
			{
				bool result = thing.IngestibleNow;
				if (result && thing is Corpse corpse)
				{
					if (corpse.IsNotFresh() && StatsUtility.GetStat(pawn, PMStatDefOf.RottenFoodSensitivity, 300) >= 1.0f)
						return false;
				}

				return result;
			}

			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var codes = instructions.ToList();

				for (var i = 3; i < codes.Count - 1; i++)
				{
					if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == originalMethod)
					{
						codes[i].opcode = OpCodes.Call;
						codes[i].operand = proxyMethod;
						codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_1));
						break;
					}
				}
				return codes;
			}
		}
	}
}