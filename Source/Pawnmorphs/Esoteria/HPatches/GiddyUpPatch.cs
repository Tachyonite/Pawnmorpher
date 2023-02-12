// GiddyUpPatch.cs created by Iron Wolf for Pawnmorph on 01/02/2021 10:19 AM
// last updated 01/02/2021  10:19 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
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
				//PatchGiddyUpCore(harmonyInstance);

				if (LoadedModManager.RunningMods.Any(m => m.PackageId == "roolo.giddyupcaravan"))
				{
					PatchGiddyUpCaravan(harmonyInstance);
				}

			}
			catch (Exception e)
			{
				Log.Error($"PM:while patching giddyup caught {e.GetType().Name} \n{e}");
			}
		}

		private static void PatchGiddyUpCaravan([NotNull] Harmony harmonyInstance)
		{
			var patchType = GenTypes.GetTypeInAnyAssembly("GiddyUpCaravan.Harmony.TransferableOneWayWidget_DoRow");
			if (patchType == null)
			{
				Log.Error($"PM: unable to patch \"GiddyUpCaravan.Harmony.TransferableOneWayWidget_DoRow\" in GiddyUp Caravan!");
				return;
			}

			var patchMethod = patchType.GetMethod("handleAnimal", BindingFlags.Static | BindingFlags.NonPublic);

			if (patchMethod == null)
			{
				Log.Error("PM: unable to patch \"handleAnimal\" in GiddyUpCaravan!");
				return;
			}

			var prefix = typeof(GiddyUpPatch).GetMethod(nameof(HandleAnimalPrefix), BindingFlags.Static | BindingFlags.NonPublic);

			harmonyInstance.Patch(patchMethod, new HarmonyMethod(prefix));
		}


		private static void HandleAnimalPrefix(Pawn animal, List<Pawn> pawns)
		{
			if (pawns != null && animal != null) pawns.Remove(animal); //remove the animal from the selection of riders 
		}

		//make all former humans non mountable, hacky solution but it'll do for now 
		private static bool IsMountablePatch(Pawn pawn, ref bool __result)
		{
			if (pawn.IsSapientFormerHuman())
			{
				__result = false;
				return false;
			}

			return true;
		}

		private static void PatchGiddyUpCore([NotNull] Harmony harmonyInstance)
		{
			Type isMountableUtils = GenTypes.GetTypeInAnyAssembly("GiddyUpCore.Utilities.IsMountableUtility");

			if (isMountableUtils == null)
			{
				Log.Error("Unable to find type IsMountableUtilities!");
				return;
			}

			Type enumTyp = isMountableUtils.GetNestedType("Reason");

			if (enumTyp == null)
			{
				Log.Error("unable to find type \"IsMountableUtility.Reason\"");
				return;
			}

			MethodInfo methodToPatch = isMountableUtils.GetMethod("isMountable", new[] { typeof(Pawn), enumTyp.MakeByRefType() });

			if (methodToPatch == null)
				Log.Error("PM: unable to patch isMountable! ");
			else
				harmonyInstance.Patch(methodToPatch,
									  new HarmonyMethod(typeof(GiddyUpPatch).GetMethod(nameof(IsMountablePatch),
																					   BindingFlags.Static
																					 | BindingFlags.NonPublic)));
		}
	}
}