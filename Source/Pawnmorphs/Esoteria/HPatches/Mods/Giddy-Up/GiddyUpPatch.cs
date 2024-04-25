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
				if (LoadedModManager.RunningMods.Any(m => m.PackageId == "roolo.giddyupcaravan" || m.PackageId == "Owlchemist.GiddyUp"))
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
	}
}