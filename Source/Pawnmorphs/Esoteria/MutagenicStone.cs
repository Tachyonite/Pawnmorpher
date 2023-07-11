using System.Collections.Generic;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// class for mutanite 
	/// </summary>
	/// <seealso cref="RimWorld.Mineable" />
	public class MutagenicStone : Mineable
	{
		private static readonly float _p; //only need one of these 
		private const float MTTH_PLANT_MUTATE = 25;


		static MutagenicStone()
		{
			_p = RandUtilities.GetUniformProbability(MTTH_PLANT_MUTATE, 4.16f); //one long tick is ~4.16 seconds
		}

		/// <summary>called every once and a while</summary>
		public override void TickRare()
		{
			if (!Spawned || !LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().enableMutagenMeteor) return;

			IEnumerable<Thing> enumerable = GenRadial.RadialDistinctThingsAround(Position, Map, 2.5f, true);

			var mutagen = MutagenDefOf.PM_MutaniteMutagen;
			foreach (Thing thing in enumerable)
			{
				if (thing is Pawn pawn)
				{
					MutatePawn(pawn, mutagen);
				}
				else if (thing is Plant plant)
				{
					if (Rand.Value >= _p) continue;

					PMPlantUtilities.TryMutatePlant(plant);

				}
			}
		}

		private void MutatePawn(Pawn pawn, MutagenDef mutagen)
		{
			if (!pawn.Spawned || !mutagen.CanInfect(pawn))
				return;

			// If pawn does not have any sensitivity
			float? mutagenSensitivity = Utilities.StatsUtility.GetStat(pawn, PMStatDefOf.MutagenSensitivity, 300);
			if (mutagenSensitivity == null || mutagenSensitivity <= 0)
				return;

			Hediff hediff = pawn.health.AddHediff(MorphTransformationDefOf.FullRandomTF);
			if (hediff is Hediff_MutagenicBase mBase)
				mBase.Causes.TryAddMutagenCause(MutagenDefOf.PM_MutaniteMutagen);

			IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.Map);
		}
	}
}
