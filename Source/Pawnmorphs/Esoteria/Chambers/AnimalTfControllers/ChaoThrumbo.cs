// Chaothrumbo.cs created by Iron Wolf for Pawnmorph on 06/25/2021 9:45 PM
// last updated 06/25/2021  9:45 PM

using RimWorld;
using Verse;

namespace Pawnmorph.Chambers.AnimalTfControllers
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Pawnmorph.Chambers.ChamberAnimalTfController" />
	public class ChaoThrumbo : ChamberAnimalTfController
	{


		/// <summary>
		///     Determines whether this instance with the specified pawn can initiate the transformation into the specified animal
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="targetAnimal">The target animal.</param>
		/// <param name="chamber">The chamber.</param>
		/// <returns></returns>
		public override ChamberTfInitiationReport CanInitiateTransformation(Pawn pawn, PawnKindDef targetAnimal, MutaChamber chamber)
		{

			var hasThing = chamber.Map.listerThings.ThingsOfDef(PMThingDefOf.PM_ChaoThrumboGenome)?.Count > 0;


			if (!hasThing)
			{
				return new ChamberTfInitiationReport(false,
													 "PMChamberMissingSpecialThing".Translate(PMThingDefOf.PM_ChaoThrumboGenome));
			}
			else return ChamberTfInitiationReport.True;

		}

		/// <summary>
		///     Initiates the transformation of the specified pawn in the given chamber into the target animal
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="targetAnimal">The target animal.</param>
		/// <param name="chamber">The chamber.</param>
		/// <returns>struct containing the pawnkindDef the pawn will turn into and the duration of the transformation</returns>
		public override ChamberAnimalTfInitStruct InitiateTransformation(Pawn pawn, PawnKindDef targetAnimal, MutaChamber chamber)
		{
			return new ChamberAnimalTfInitStruct(targetAnimal, 2, PMThingDefOf.PM_ChaoThrumboGenome);
		}

		/// <summary>
		/// Called when the pawn is ejected either in a full tf or an aborted transformation 
		/// </summary>
		/// <param name="original">The original.</param>
		/// <param name="transformedPawn">The transformed pawn. null if the chamber ejected the pawn before the transformation finished</param>
		/// <param name="chamber">The chamber.</param>
		public override void OnPawnEjects(Pawn original, Pawn transformedPawn, MutaChamber chamber)
		{
			GenExplosion.DoExplosion(chamber.Position, chamber.Map, 7, PMDamageDefOf.MutagenCloud_Large, null, postExplosionSpawnThingDef: PMThingDefOf.PM_Filth_Slurry, postExplosionSpawnChance: 0.35f, postExplosionSpawnThingCount: 2);
			chamber.TakeDamage(new DamageInfo(DamageDefOf.Bomb, chamber.HitPoints * 0.7f, 1));
		}
	}
}