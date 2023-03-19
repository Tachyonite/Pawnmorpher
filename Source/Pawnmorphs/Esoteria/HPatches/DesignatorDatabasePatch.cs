// DesignatorPatch.cs created by Iron Wolf for Pawnmorph on 03/15/2020 3:30 PM
// last updated 03/15/2020  3:30 PM

using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.Designations;
using Verse;

namespace Pawnmorph.HPatches
{

	[HarmonyPatch(typeof(ReverseDesignatorDatabase), "InitDesignators")]
	static class DesignatorDatabasePatch
	{
		[HarmonyPostfix]
		static void Postfix([NotNull] List<Designator> ___desList)
		{
			___desList.Add(new RecruitSapientFormerHuman());
		}
	}
}