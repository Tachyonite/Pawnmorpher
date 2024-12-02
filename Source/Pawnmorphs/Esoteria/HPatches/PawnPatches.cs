// PawnPatches.cs created by Iron Wolf for Pawnmorph on 02/19/2020 5:41 PM
// last updated 04/26/2020  9:22 AM

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.Hybrids;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(Pawn))]
	static class PawnPatches
	{
		static PawnPatches()
		{
			PawnmorphGameComp.OnClear += OnClear;
		}

		private static void OnClear(PawnmorphGameComp obj)
		{
			_workModifierCache.Clear();
		}


		[HarmonyPatch(nameof(Pawn.Destroy)), HarmonyPostfix]
		static void PawnDestroyedPostfix(Pawn __instance)
		{
			if (__instance.Discarded == false && __instance.RaceProps.Humanlike)
				UnregisterMutations(__instance);
		}


		[HarmonyPatch(nameof(Pawn.Discard)), HarmonyPostfix]
		static void PawnDiscardPostfix(Pawn __instance)
		{
			if (__instance.Destroyed == false && __instance.RaceProps.Humanlike)
				UnregisterMutations(__instance);
		}


		private static void UnregisterMutations(Pawn pawn)
		{
			IList<Hediff_AddedMutation> mutations = pawn.GetMutationTracker()?.AllMutations;
			if (mutations == null)
				return;

			for (int i = mutations.Count - 1; i >= 0; i--)
				PawnmorpherMod.WorldComp.UnregisterMutation(mutations[i]);
		}

		[HarmonyPatch(nameof(Pawn.CombinedDisabledWorkTags), MethodType.Getter), HarmonyPostfix]
		static void FixCombinedDisabledWorkTags(ref WorkTags __result, [NotNull] Pawn __instance)
		{
			var hediffs = __instance.health?.hediffSet?.hediffs;
			if (hediffs == null) 
				return;

			for (int i = hediffs.Count - 1; i >= 0; i--)
			{
				Hediff hediff = hediffs[i];
				if (hediff is IWorkModifier wM)
				{
					__result |= ~wM.AllowedWorkTags;
				}
				else
				{
					if (hediff.def?.stages == null)
						continue;

					for (int stageIndex = hediff.def.stages.Count - 1; stageIndex >= 0; stageIndex--)
					{
						HediffStage hediffStage = hediff.def.stages[stageIndex];
						if (hediffStage is IWorkModifier sWM)
						{
							__result |= ~sWM.AllowedWorkTags;
						}
					}
				}
			}
		}

		[HarmonyPatch(nameof(Pawn.ThreatDisabledBecauseNonAggressiveRoamer)), HarmonyPrefix]
		static bool FixNonAggressiveRoamer(Pawn __instance, ref bool __result)
		{
			if (__instance.IsHumanlike())
			{
				__result = false;
				return false;
			}

			return true;
		}


		[HarmonyPatch("CheckForDisturbedSleep"), HarmonyPrefix]
		static bool FixDisturbedSleep(Pawn source, Pawn __instance)
		{
			var morph = __instance.def.GetMorphOfRace();
			if (morph != null)
			{
				var sourceMorph = source.def.GetMorphOfRace();
				if (morph.@group == sourceMorph?.@group) return false;
			}

			return true;
		}

		[NotNull]
		private static Dictionary<Pawn, TimedCache<List<IWorkModifier>>> _workModifierCache = new Dictionary<Pawn, TimedCache<List<IWorkModifier>>>();

		[HarmonyPatch(nameof(Pawn.GetDisabledWorkTypes)), HarmonyPostfix]
		static void FixGetAllDisabledWorkTypes(List<WorkTypeDef> __result, Pawn __instance, bool permanentOnly)
		{
			if (__result == null) return;



			if (_workModifierCache.TryGetValue(__instance, out TimedCache<List<IWorkModifier>> cachedModifierList) == false)
			{
				cachedModifierList = new TimedCache<List<IWorkModifier>>(() =>
				{
					var hediffs = __instance?.health?.hediffSet?.hediffs;
					if (hediffs == null) 
						return null;

					List<IWorkModifier> modifiers = new List<IWorkModifier>();
					
					foreach (Hediff hediff in hediffs)
					{
						if (hediff is IWorkModifier wmH)
						{
							modifiers.Add(wmH);
						}

						var stages = (hediff.def?.stages).MakeSafe().OfType<IWorkModifier>();
						modifiers.AddRange(stages);
					}

					return modifiers;
				}, new List<IWorkModifier>());

				_workModifierCache[__instance] = cachedModifierList;
				cachedModifierList.Update();
				cachedModifierList.Offset(-_workModifierCache.Count);
			}

			List<IWorkModifier> modifiers = cachedModifierList.GetValue(200);
			if (modifiers == null || modifiers.Count == 0)
				return;

			List<WorkTypeDef> types = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			for (int typeIndex = types.Count - 1; typeIndex >= 0; typeIndex--)
			{
				WorkTypeDef workTypeDef = types[typeIndex];

				if (__result.Contains(workTypeDef))
					continue; // Already disabled.

				for (int modifierIndex = modifiers.Count - 1; modifierIndex >= 0; modifierIndex--)
				{
					IWorkModifier modifier = modifiers[modifierIndex];

					if ((modifier.AllowedWorkTags & workTypeDef.workTags) == 0)
					{
						__result.Add(workTypeDef);
						break;
					}

					if (modifier.WorkTypeFilter?.PassesFilter(workTypeDef) == false)
					{
						__result.Add(workTypeDef);
						break;
					}
				}
			}
		}

		//hacky way to make sure the race comp check always happens after all comps have finished ticking 
		internal static void QueueRaceCheck(Pawn p)
		{
			_waitingQueue.AddLast(p);
		}


		[NotNull]
		private static readonly LinkedList<Pawn> _waitingQueue = new LinkedList<Pawn>(); //need to use a list because unspawned pawns may be queued 


		private static readonly List<(Pawn, Action)> _queuedPostTickActions = new List<(Pawn, Action)>();
		internal static void QueuePostTickAction(Pawn p, Action action)
		{
			_queuedPostTickActions.Add((p, action));
		}


		//this is a post fix and not in a comp because we need to make sure comps aren't added or removed while they are being iterated over
		[HarmonyPatch(typeof(Verse.TickManager), nameof(Verse.TickManager.DoSingleTick)), HarmonyPostfix]
		static void RunRaceCompCheck()
		{
			if (_waitingQueue.Count > 0)
			{
				var node = _waitingQueue.First;
				Pawn pawn = null;
				while (node != null)
				{
					try
					{
						pawn = node.Value;

						if (pawn != null && node.Value.Destroyed == false)
							RaceShiftUtilities.AddRemoveDynamicRaceComps(pawn, pawn.def);

						var next = node.Next;
						_waitingQueue.Remove(node);
						node = next;
					}
					catch (Exception e)
					{
						Log.Error($"unable to perform race check on pawn {pawn?.Name?.ToStringFull ?? "NULL"}\ncaught {e.GetType().Name}");
						throw;
					}
				}
			}

			if (_queuedPostTickActions.Count > 0)
			{
				for (int i = _queuedPostTickActions.Count - 1; i >= 0; i--)
				{
					try
					{
						_queuedPostTickActions[i].Item2();
					}
					finally
					{
						_queuedPostTickActions.RemoveAt(i);
					}
				}
			}
		}


		[HarmonyPatch(nameof(Pawn.GetGizmos))]
		[HarmonyPostfix]
		static IEnumerable<Gizmo> GetGizmosPatch(IEnumerable<Gizmo> __result, [NotNull] Pawn __instance)
		{
			foreach (Gizmo gizmo in __result)
			{
				yield return gizmo;
			}

			var peq = __instance.equipment?.Primary;
			if (peq != null)
			{
				foreach (IEquipmentGizmo eqGizmo in peq.AllComps.OfType<IEquipmentGizmo>())
				{
					foreach (Gizmo gizmo in eqGizmo.GetGizmos())
					{
						yield return gizmo;
					}
				}
			}
		}


		[ThreadStatic]
		private static (Pawn, float) _bodySizeCache;


		[HarmonyPatch(nameof(Pawn.BodySize), MethodType.Getter), HarmonyPostfix]
		static float GetBodySizePatch(float __result, [NotNull] Pawn __instance)
		{
			float bodySizeModifier = 0;
			if (_bodySizeCache.Item1 == __instance)
				bodySizeModifier = _bodySizeCache.Item2;
			else
			{
				if (__instance.def.race.Animal == false)
					bodySizeModifier = StatsUtility.GetStat(__instance, PMStatDefOf.PM_BodySize, 1000).GetValueOrDefault();

				_bodySizeCache.Item1 = __instance;
				_bodySizeCache.Item2 = bodySizeModifier;
			}

			if (bodySizeModifier > 0)
				return __result * bodySizeModifier;

			return __result;
		}
	}
}