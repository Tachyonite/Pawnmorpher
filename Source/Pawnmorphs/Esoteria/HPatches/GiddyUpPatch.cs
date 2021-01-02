// GiddyUpPatch.cs created by Iron Wolf for Pawnmorph on 01/02/2021 10:19 AM
// last updated 01/02/2021  10:19 AM

using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using HugsLib.Utils;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.HPatches
{
    internal static class GiddyUpPatch 
    {
        internal static void PatchGiddyUp([NotNull] Harmony harmonyInstance)
        {
            try
            {
                var isMountableUtils = GenTypes.GetTypeInAnyAssembly("GiddyUpCore.Utilities.IsMountableUtility");

                if (isMountableUtils==null)
                {
                    Log.Error("Unable to find type IsMountableUtilities!");
                    return;
                }

                var enumTyp = isMountableUtils.GetNestedType("Reason");

                if (enumTyp == null)
                {
                    Log.Error($"unable to find type \"IsMountableUtility.Reason\"");return;
                }



                var methodToPatch = isMountableUtils.GetMethod("isMountable", new Type[] {typeof(Pawn), enumTyp.MakeByRefType()});

                if (methodToPatch == null)
                {
                    Log.Error($"PM: unable to patch isMountable! ");
                }
                else
                {
                    harmonyInstance.Patch(methodToPatch,
                                          prefix: new HarmonyMethod(typeof(GiddyUpPatch).GetMethod(nameof(IsMountablePatch),
                                                                                                    BindingFlags.Static
                                                                                                  | BindingFlags.NonPublic)));
                }

            }
            catch (Exception e)
            {
                Log.Error($"PM:while patching giddyup caught {e.GetType().Name} \n{e}");
            }
        }

        //make all former humans non mountable, hacky solution but it'll do for now 
        static bool IsMountablePatch(Pawn pawn, ref bool __result)
        {
            if (pawn.IsSapientFormerHuman())
            {
                __result = false;
                return false; 
            }

            return true; 

        }

    }
}