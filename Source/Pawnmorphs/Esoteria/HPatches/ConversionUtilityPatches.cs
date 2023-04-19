// ConversionUtilityPatches.cs created by Iron Wolf for Pawnmorph on 07/21/2021 8:53 PM
// last updated 07/21/2021  8:53 PM

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	/// <summary>
	/// class for conversion utility patches 
	/// </summary>
	public static class ConversionUtilityPatches
	{
		/// <summary>
		/// Preforms the patches.
		/// </summary>
		/// <param name="harInst">The har inst.</param>
		public static void PreformPatches([NotNull] Harmony harInst)
		{
			try
			{
				var mainPatchType = typeof(ConversionUtility);
				var mainPatcherType = typeof(ConversionUtilityPatches);
				var OffsetDelegate = mainPatchType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
											 .FirstOrDefault(m => m.HasAttribute<CompilerGeneratedAttribute>()
															   && m.Name.Contains("OffsetFromIdeo"));

				var OffsetPrefix =
					mainPatcherType.GetMethod(nameof(OffsetDelegateBugFix), BindingFlags.Static | BindingFlags.NonPublic);

				if (OffsetDelegate == null)
				{
					Log.Error($"unable to find delegate \"OffsetFromIdeo\" in ConversionUtilityPatches");
					return;
				}

				harInst.Patch(OffsetDelegate, new HarmonyMethod(OffsetPrefix));


			}
			catch (Exception e)
			{
				Log.Error($"unable to preform Conversion utility patches, caught \n{e}");
			}
		}


		static bool OffsetDelegateBugFix(Pawn pawn, bool invert, ref float __result)
		{
			if (pawn?.Ideo == null)
			{
				__result = 0;
				return false;
			}

			return true;
		}
	}
}