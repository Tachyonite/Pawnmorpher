// StockGenerator_Injectors.cs modified by Iron Wolf for Pawnmorph on 01/22/2020 5:45 PM
// last updated 01/22/2020  5:45 PM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// stock generator for getting all injectors of a specific morph or under an animal class 
	/// </summary>
	/// <seealso cref="RimWorld.StockGenerator" />
	public class StockGenerator_Injectors : StockGenerator
	{
		/// <summary>
		/// The animal class or morph to get the injectors of 
		/// </summary>
		public AnimalClassBase animalClass;



		[Unsaved] private List<ThingDef> _injectors;
		[NotNull]
		List<ThingDef> Injectors
		{
			get
			{
				if (_injectors == null)
				{
					IEnumerable<ThingDef> allThingsInCat = PMThingCategoryDefOf.Injector.DescendantThingDefs; //only grab injectors 

					//get all full transformation hediffs in the given morph category 
					IEnumerable<HediffDef> allHediffs = (animalClass?.GetAllMorphsInClass()).MakeSafe().Select(m => m.fullTransformation);
					_injectors = new List<ThingDef>();

					foreach (ThingDef thingDef in allThingsInCat)
					{
						IEnumerable<IngestionOutcomeDoer> outcomeDoers = thingDef.ingestible?.outcomeDoers;
						if (outcomeDoers == null)
							continue;

						IEnumerable<HediffDef> rGivers = outcomeDoers.OfType<IngestionOutcomeDoer_GiveHediff>().Select(g => g.hediffDef);

						foreach (HediffDef hediffDef in rGivers)
						{
							if (allHediffs.Contains(hediffDef)) //if any ingestion outcome doer adds a hediff we're looking for add it to the list and stop looking
							{
								_injectors.Add(thingDef);
								break;
							}
						}
					}
				}

				return _injectors;
			}
		}

		/// <summary>
		/// Generates the things.
		/// </summary>
		/// <param name="forTile">For tile.</param>
		/// <param name="factionFor">The faction the things are being generated for.</param>
		/// <returns></returns>
		public override IEnumerable<Thing> GenerateThings(int forTile, Faction factionFor = null)
		{
			if (Injectors.Count == 0)
			{
				Log.Error($"{nameof(StockGenerator_Injectors)} in {trader.defName} could not get any injectors! make sure {nameof(animalClass)} is set to something that has morphs!");
				yield break;
			}
			var max = countRange.RandomInRange;

			for (int i = 0; i < max; i++)
			{
				var rInjector = Injectors.RandElement();
				yield return ThingMaker.MakeThing(rInjector);
			}
		}

		/// <summary>
		/// checks if this handles the specified thing definition.
		/// </summary>
		/// <param name="thingDef">The thing definition.</param>
		/// <returns></returns>
		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return Injectors.Contains(thingDef);
		}
	}
}