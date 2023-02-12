// DrugPolicyPatches.cs modified by Iron Wolf for Pawnmorph on 02/14/2020 8:39 PM
// last updated 02/14/2020  8:39 PM

using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	static class DrugPolicyPatches
	{
		[HarmonyPatch(typeof(Pawn_DrugPolicyTracker))]
		[HarmonyPatch(nameof(Pawn_DrugPolicyTracker.ExposeData))]
		static class DrugPolicyFix
		{
			/// <summary>
			/// simple fix to drug policy to account for thing defs being removed 
			/// </summary>
			/// <param name="___drugTakeRecords">The drug take records.</param>
			[HarmonyPostfix]
			static void Postfix([CanBeNull] List<DrugTakeRecord> ___drugTakeRecords)
			{
				if (Scribe.mode == LoadSaveMode.PostLoadInit)
				{
					if (___drugTakeRecords == null) return;
					for (int i = ___drugTakeRecords.Count - 1; i >= 0; i--)
					{
						var record = ___drugTakeRecords[i];
						if (record?.drug == null) //remove any null records or records with null drugs 
						{
							___drugTakeRecords.RemoveAt(i);
						}
						else if (!record.drug.IsDrug)
							___drugTakeRecords.RemoveAt(i);
					}
				}
			}
		}

	}
}