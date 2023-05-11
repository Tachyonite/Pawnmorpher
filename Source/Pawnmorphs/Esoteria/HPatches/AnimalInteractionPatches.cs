// AnimalInteractionPatches.cs created by Iron Wolf for Pawnmorph on 09/13/2020 9:37 AM
// last updated 09/13/2020  9:37 AM

using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.HPatches
{
	static class AnimalInteractionPatches
	{
		[HarmonyPatch(typeof(JobDriver_Nuzzle))]
		static class NuzzlePatches
		{
			[HarmonyPatch("MakeNewToils"), HarmonyPostfix]
			static IEnumerable<Toil> MakeNewToilsPostfix([NotNull] IEnumerable<Toil> __result, [NotNull] JobDriver_Nuzzle __instance)
			{
				foreach (Toil toil in __result)
				{
					yield return toil;
				}

				var newToil = Toils_General.Do(() =>
				{
					Pawn nuzzlee = (__instance.pawn?.CurJob?.targetA.Thing) as Pawn;
					MutagenicNuzzle(__instance.pawn, nuzzlee);

				});
				yield return newToil;
			}


			static void MutagenicNuzzle(Pawn nuzzler, Pawn nuzzlee)
			{
				if (nuzzlee == null || nuzzler == null) return;

				var asT = nuzzler.GetAspectTracker();

				var mutInfused = asT?.GetAspect(AspectDefOf.MutagenInfused);
				if (mutInfused?.StageIndex != 0) return;

				HediffDef hediffToAdd = null;
				if (nuzzler.IsChaomorph())
				{
					hediffToAdd = MorphTransformationDefOf.FullRandomTF;
				}
				var bestMorph = MorphUtilities.TryGetBestMorphOfAnimal(nuzzler.def);
				if (bestMorph != null)
				{
					hediffToAdd = bestMorph.partialTransformation;
				}

				if (hediffToAdd != null)
				{
					var health = nuzzlee.health;
					health?.AddHediff(hediffToAdd);
				}

			}
		}
	}
}