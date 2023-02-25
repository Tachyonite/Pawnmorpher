// VeneratedMutation.cs created by Iron Wolf for Pawnmorph on 07/25/2021 4:35 PM
// last updated 07/25/2021  4:35 PM

using System.Linq;
using RimWorld;
using Verse;

namespace Pawnmorph.PreceptComps
{
	/// <summary>
	/// precept comp for giving venerated animal mutation thoughts 
	/// </summary>
	/// <seealso cref="Pawnmorph.PreceptComps.VeneratedAnimalMemory" />
	public class VeneratedMutation : VeneratedAnimalMemory
	{
		/// <summary>
		/// Gets the animal from the given history event .
		/// </summary>
		/// <param name="historyEvent">The history event.</param>
		/// <param name="ideo">The ideo.</param>
		/// <returns>
		/// the animal from the event. if null the thought will not be given
		/// </returns>
		protected override ThingDef GetAnimal(in HistoryEvent historyEvent, Ideo ideo)
		{
			var mut = historyEvent.GetArg<Hediff_AddedMutation>(PMHistoryEventArgsNames.MUTATION);


			foreach (AnimalClassBase animalClass in mut.Def.ClassInfluences)
			{
				if (animalClass is MorphDef morph)
				{
					foreach (ThingDef ideoA in ideo.VeneratedAnimals)
						if (morph.AllAssociatedAnimals.Contains(ideoA))
							return ideoA;
				}
			}


			return null;
		}
	}

	/// <summary>
	/// precept comp for giving venerated morph thoughts 
	/// </summary>
	/// <seealso cref="Pawnmorph.PreceptComps.VeneratedAnimalMemory" />
	public class VeneratedMorph : VeneratedAnimalMemory
	{

		/// <summary>
		/// if this is for reversion 
		/// </summary>
		public bool reversion;

		/// <summary>
		/// Gets the animal from the given history event .
		/// </summary>
		/// <param name="historyEvent">The history event.</param>
		/// <param name="ideo">The ideo.</param>
		/// <returns>
		/// the animal from the event. if null the thought will not be given
		/// </returns>
		protected override ThingDef GetAnimal(in HistoryEvent historyEvent, Ideo ideo)
		{
			var pk = historyEvent.GetArg<MorphDef>(reversion
														  ? PMHistoryEventArgsNames.OLD_MORPH
														  : PMHistoryEventArgsNames.NEW_MORPH);

			if (ideo.IsVeneratedAnimal(pk.race)) return pk.race;
			return null;

		}
	}

	/// <summary>
	/// comp for giving thoughts related to venerated animal transformations 
	/// </summary>
	/// <seealso cref="Pawnmorph.PreceptComps.VeneratedAnimalMemory" />
	public class VeneratedAnimalTransformation : VeneratedAnimalMemory
	{
		/// <summary>
		/// Gets the animal from the given history event .
		/// </summary>
		/// <param name="historyEvent">The history event.</param>
		/// <param name="ideo">The ideo.</param>
		/// <returns>
		/// the animal from the event. if null the thought will not be given
		/// </returns>
		protected override ThingDef GetAnimal(in HistoryEvent historyEvent, Ideo ideo)
		{
			var race = historyEvent.GetArg<Pawn>(PMHistoryEventArgsNames.TRANSFORMED_PAWN)?.def;
			if (!ideo.IsVeneratedAnimal(race)) return null;
			return race;
		}
	}
}