// NativeVerbPatches.cs created by Iron Wolf for Pawnmorph on 03/01/2020 1:21 PM
// last updated 03/01/2020  1:21 PM

using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

#pragma warning disable 1591
namespace Pawnmorph.HPatches
{

	public class NativeVerbPatches
	{
		[HarmonyPatch(typeof(Pawn_NativeVerbs), "CheckCreateVerbProperties")]
		static class AddFireVerbsToFormerHuman
		{
			[HarmonyPostfix]

			static void Postfix([NotNull] ref List<VerbProperties> ___cachedVerbProperties, [NotNull] Pawn ___pawn)
			{
				if (___pawn.IsFormerHuman())

				{
					___cachedVerbProperties = ___cachedVerbProperties ?? new List<VerbProperties>();

					if (!___cachedVerbProperties.Any(v => v?.category == VerbCategory.BeatFire))
					{
						___cachedVerbProperties.Add(NativeVerbPropertiesDatabase.VerbWithCategory(VerbCategory.BeatFire));
					}
					if (!___cachedVerbProperties.Any(v => v?.category == VerbCategory.Ignite))
						___cachedVerbProperties.Add(NativeVerbPropertiesDatabase.VerbWithCategory(VerbCategory.Ignite));
				}
			}
		}
	}
}