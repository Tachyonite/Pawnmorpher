using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.HPatches
{
    [HarmonyPatch(typeof(Gizmo))]
    static class GizmoPatches
    {
        // List of gizmos to monitor and whether or not they have been merged.
        static Dictionary<Gizmo, bool> _registeredGizmosMerged;

        public static void HideGizmoOnMerged(Gizmo gizmo)
        {
            if (_registeredGizmosMerged == null)
                _registeredGizmosMerged = new Dictionary<Gizmo, bool>();


            if (_registeredGizmosMerged.ContainsKey(gizmo) == false)
            {
                _registeredGizmosMerged.Add(gizmo, false);
            }
        }

        [HarmonyPatch(nameof(Gizmo.GroupsWith)), HarmonyPostfix]
        static bool GizmoGroupsWithPatch(bool __result, Gizmo other)
        {
            if (_registeredGizmosMerged == null)
                return __result;

            // Reset merged state
            if (_registeredGizmosMerged.ContainsKey(other))
                _registeredGizmosMerged[other] = false;

            return __result;
        }



        [HarmonyPatch(nameof(Gizmo.MergeWith)), HarmonyPostfix]
        static void GizmoMergeWithPostPatch(Gizmo other)
        {
            if (_registeredGizmosMerged == null)
                return;

            // Reset merged state
            if (_registeredGizmosMerged.ContainsKey(other))
                _registeredGizmosMerged[other] = true;
        }


        [HarmonyPatch(nameof(Gizmo.Visible), MethodType.Getter), HarmonyPostfix]
        static bool GetGizmoVisible(bool __result, Gizmo __instance)
        {
            if (_registeredGizmosMerged == null)
                return __result;

            if (_registeredGizmosMerged.TryGetValue(__instance, out bool merged) && merged)
            {
                __instance.disabled = true;
                return false;
            }

            return __result;
        }
    }
}
