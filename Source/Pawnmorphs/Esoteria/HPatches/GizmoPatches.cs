using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(Gizmo))]
	static class GizmoPatches
	{
		// List of gizmos to monitor and whether or not they have been merged.
		static Dictionary<Gizmo, bool> _registeredGizmosMerged;
		static Dictionary<Gizmo, Pawn> _gizmoPawnMap;

		public static void HideGizmoOnMerged(Gizmo gizmo, Pawn pawn)
		{
			if (_registeredGizmosMerged == null)
			{
				_registeredGizmosMerged = new Dictionary<Gizmo, bool>();
				_gizmoPawnMap = new Dictionary<Gizmo, Pawn>();
			}


			if (_registeredGizmosMerged.ContainsKey(gizmo) == false)
			{
				_registeredGizmosMerged.Add(gizmo, false);
				_gizmoPawnMap.Add(gizmo, pawn);
			}
		}

		[HarmonyPatch(nameof(Gizmo.GroupsWith)), HarmonyPostfix]
		static bool GizmoGroupsWithPatch(bool __result, Gizmo other, Gizmo __instance)
		{
			if (_registeredGizmosMerged == null)
				return __result;

			// Reset merged state
			if (_registeredGizmosMerged.ContainsKey(__instance))
				_registeredGizmosMerged[__instance] = false;

			return __result;
		}



		[HarmonyPatch(nameof(Gizmo.MergeWith)), HarmonyPostfix]
		static void GizmoMergeWithPostPatch(Gizmo other, Gizmo __instance)
		{
			if (_registeredGizmosMerged == null)
				return;

			// Are both merged and merged with registered?
			if (_registeredGizmosMerged.ContainsKey(__instance) && _registeredGizmosMerged.ContainsKey(other))
			{
				// Are they from the same pawn? Gizmos should only be hidden if they are merged with an identical gizmo of the same pawn.
				if (_gizmoPawnMap[__instance] == _gizmoPawnMap[other])
				{
					_registeredGizmosMerged[other] = true;
				}
			}
		}


		[HarmonyPatch(nameof(Gizmo.Visible), MethodType.Getter), HarmonyPostfix]
		static bool GetGizmoVisible(bool __result, Gizmo __instance)
		{
			if (_registeredGizmosMerged == null)
				return __result;

			if (_registeredGizmosMerged.TryGetValue(__instance, out bool merged) && merged)
			{
				__instance.Disable();
				return false;
			}

			return __result;
		}
	}
}
