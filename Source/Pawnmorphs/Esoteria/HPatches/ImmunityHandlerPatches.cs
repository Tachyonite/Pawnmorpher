// ImmunityHandlerPatches.cs modified by Iron Wolf for Pawnmorph on 02/14/2020 8:18 PM
// last updated 02/14/2020  8:18 PM

using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.HPatches
{
	static class ImmunityHandlerPatches
	{
		[HarmonyPatch(typeof(ImmunityHandler))]
		[HarmonyPatch(nameof(ImmunityHandler.ExposeData))]
		static class FixImmunityHandler
		{

			/// <summary>
			/// fix for the immunity handler to remove records on load that are null or have null hediff defs .
			/// </summary>
			/// <param name="__instance">The instance.</param>
			/// <param name="___immunityList">The immunity list.</param>
			[HarmonyPostfix]
			static void Postfix([NotNull] ImmunityHandler __instance, [CanBeNull] List<ImmunityRecord> ___immunityList)
			{
				if (Scribe.mode == LoadSaveMode.PostLoadInit)
				{
					if (___immunityList == null) return;
					for (int i = ___immunityList.Count - 1; i >= 0; i--)
					{


						var imR = ___immunityList[i];


						if (imR?.hediffDef == null) //remove all null immunity record or records with null hediffDefs 
						{
							___immunityList.RemoveAt(i);
						}
					}
				}
			}
		}
	}
}