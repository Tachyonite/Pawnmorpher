// DrupPolicyPatches.cs created by Iron Wolf for Pawnmorph on 12/06/2021 4:46 PM
// last updated 12/06/2021  4:46 PM

using System.Collections.Generic;
using HarmonyLib;
using RimWorld;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(DrugPolicy), "InitializeIfNeeded")]
	static class DrupPolicyPatches
	{
		static void Prefix(bool overwriteExisting, ref List<DrugPolicyEntry> ___entriesInt)
		{
			if (overwriteExisting && ___entriesInt != null)
			{
				for (int i = ___entriesInt.Count - 1; i >= 0; i--)
				{
					if (___entriesInt[i]?.drug?.IsDrug != true) //fix for broken drug policies 
					{
						___entriesInt.RemoveAt(i);
					}
				}
			}
		}
	}
}