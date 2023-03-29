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
		[HarmonyPatch(nameof(Pawn.CombinedDisabledWorkTags), MethodType.Getter), HarmonyPostfix]
		static void FixCombinedDisabledWorkTags(ref WorkTags __result, [NotNull] Pawn __instance)
		{
			var hediffs = __instance.health?.hediffSet?.hediffs;
			if (hediffs == null) return;

			foreach (Hediff hediff in hediffs)
			{
				if (hediff is IWorkModifier wM)
				{
					__result |= ~wM.AllowedWorkTags;
				}
				else
				{
					foreach (HediffStage hediffStage in hediff.def.stages.MakeSafe())
					{
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

		[HarmonyPatch(nameof(Pawn.IsColonist), MethodType.Getter), HarmonyPrefix]
		static bool FixIsColonist(ref bool __result, [NotNull] Pawn __instance)
		{
			// This should probably be replaced with a transpiler.

			// Don't do additional checks for pawns of other factions.
			if (__instance.Faction?.def.isPlayer == true)
			{
				__result = __instance.GetIntelligence() == Intelligence.Humanlike;
				if (__result && __instance.IsSlave == true)
					__result = __instance.guest.SlaveIsSecure;

				return false;
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
		[HarmonyPatch(nameof(Pawn.Tick)), HarmonyPostfix]
		static void RunRaceCompCheck(Pawn __instance)
		{
			if (_waitingQueue.Count > 0)
			{
				try
				{
					var node = _waitingQueue.First;

					while (node != null)
					{
						if (node.Value == __instance) 
							break;

						var next = node.Next;
						if (node.Value == null || node.Value.Destroyed)
							_waitingQueue.Remove(node);

						node = next;
					}

					if (node != null)
					{
						_waitingQueue.Remove(node);
						RaceShiftUtilities.AddRemoveDynamicRaceComps(__instance, __instance.def);

					}
				}
				catch (Exception e)
				{
					Log.Error($"unable to perform race check on pawn {__instance?.Name?.ToStringFull ?? "NULL"}\ncaught {e.GetType().Name}");
					throw;
				}
			}

			if (_queuedPostTickActions.Count > 0)
			{
				for (int i = _queuedPostTickActions.Count - 1; i >= 0; i--)
				{
					if (_queuedPostTickActions[i].Item1 == __instance)
					{
						try
						{
							_queuedPostTickActions[i].Item2();
						}
						finally
						{
							_queuedPostTickActions.RemoveAt(i);
						}
						break;
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

		[HarmonyPatch(nameof(Pawn.BodySize), MethodType.Getter), HarmonyPostfix]
		static float GetBodySizePatch(float __result, [NotNull] Pawn __instance)
		{
			float? bodySizeModifier = StatsUtility.GetStat(__instance, PMStatDefOf.PM_BodySize, 300);
			if (bodySizeModifier.HasValue && bodySizeModifier.Value > 0)
				return __result * bodySizeModifier.Value;

			return __result;
		}
	}
}