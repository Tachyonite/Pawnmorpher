// WorkSettingsPatches.cs modified by Iron Wolf for Pawnmorph on 12/24/2019 1:34 PM
// last updated 12/24/2019  1:34 PM

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	static class WorkSettingsPatches
	{
		[HarmonyPatch(typeof(Pawn_WorkSettings)), HarmonyPatch(nameof(Pawn_WorkSettings.EnableAndInitialize))]
		static class InitializationPatch
		{
			[HarmonyPostfix]
			static void InitializeForFormerHumans([NotNull] Pawn_WorkSettings __instance, [NotNull] Pawn ___pawn)
			{
				if (___pawn.IsFormerHuman() && ___pawn.workSettings != null)
				{

					FormerHumanUtilities.InitializeWorkSettingsFor(___pawn, __instance);


				}
			}
		}
	}
}