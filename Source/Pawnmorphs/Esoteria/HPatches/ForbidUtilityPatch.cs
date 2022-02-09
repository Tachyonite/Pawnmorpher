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
    [HarmonyPatch(typeof(ForbidUtility))]
    internal class ForbidUtilityPatch
    {
        [HarmonyPatch(nameof(ForbidUtility.SetForbidden)), HarmonyPrefix]
        static void SetForbidden(Thing t, ref bool value, ref bool warnOnFail)
        {
            warnOnFail = false;
        }


    }
}
