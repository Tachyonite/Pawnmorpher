using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.FormerHumans;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Pawnmorph
{

	/// <summary>
	/// stock generator for morph traders 
	/// </summary>
	/// <seealso cref="RimWorld.StockGenerator" />
	public class StockGenerator_MorphSlaves : StockGenerator
	{
		[UsedImplicitly(ImplicitUseKindFlags.Assign)]
		private bool respectPopulationIntent = default;

		/// <summary>
		/// Generates the things.
		/// </summary>
		/// <param name="forTile">For tile.</param>
		/// <param name="forFaction">the faction this is being generated for</param>
		/// <returns></returns>
		public override IEnumerable<Thing> GenerateThings(int forTile, Faction forFaction = null)
		{
			if (respectPopulationIntent && Rand.Value > StorytellerUtilityPopulation.PopulationIntent)
			{
				yield break;
			}
			int count = countRange.RandomInRange;
			for (int i = 0; i < count; i++)
			{
				if (!(from fac in Find.FactionManager.AllFactionsVisible
					  where fac != Faction.OfPlayer && fac.def.defName == "PawnmorpherEnclave"
					  select fac).TryRandomElement(out Faction slaveFaction))
				{
					break;
				}
				PawnKindDef slave = PawnKindDef.Named("PawnmorpherSlave");
				PawnGenerationRequest request = new PawnGenerationRequest(slave, slaveFaction, PawnGenerationContext.NonPlayer, forTile, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, !trader.orbital);
				yield return PawnGenerator.GeneratePawn(request);
			}
		}

		/// <summary>
		/// checks if this generator handles the given thingDef
		/// </summary>
		/// <param name="thingDef">The thing definition.</param>
		/// <returns></returns>
		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef.category == ThingCategory.Pawn && thingDef.race.Humanlike && thingDef.tradeability != Tradeability.None;
		}
	}

	/// <summary>
	/// stock generator for morph traders 
	/// </summary>
	/// <seealso cref="RimWorld.StockGenerator" />
	public class StockGenerator_Morphs : StockGenerator
	{
		[NoTranslate] private List<string> tradeTagsSell = new List<string>();
		private IntRange kindCountRange = new IntRange(1, 1);
		private float minWildness = 0f;
		private float maxWildness = 1f;
		private bool checkTemperature = true;

		private static readonly SimpleCurve SelectionChanceFromWildnessCurve = new SimpleCurve
		{
			new CurvePoint(0f, 100f),
			new CurvePoint(0.25f, 60f),
			new CurvePoint(0.5f, 30f),
			new CurvePoint(0.75f, 12f),
			new CurvePoint(1f, 2f)
		};

		/// <summary>
		/// Generates the things for the given forTile.
		/// </summary>
		/// <param name="forTile">For tile.</param>
		/// <param name="forFaction">For faction.</param>
		/// <returns></returns>
		[NotNull]
		IEnumerable<Thing> GenerateThingEnumer(int forTile, Faction forFaction)
		{
			int numKinds = kindCountRange.RandomInRange;
			int count = countRange.RandomInRange;
			List<PawnKindDef> kinds = new List<PawnKindDef>();
			for (int j = 0; j < numKinds; j++)
			{
				if (!(from k in DefDatabase<PawnKindDef>.AllDefs
					  where !kinds.Contains(k) && PawnKindAllowed(k, forTile)
					  select k).TryRandomElementByWeight((PawnKindDef k) => SelectionChance(k), out PawnKindDef result))
				{
					break;
				}
				kinds.Add(result);
			}
			for (int i = 0; i < count; i++)
			{
				if (!kinds.TryRandomElement(out PawnKindDef kind))
				{
					break;
				}
				PawnKindDef kind2 = kind;
				int tile = forTile;

				Pawn pawnOriginal = Find.WorldPawns.AllPawnsAlive.Where(p => !p.IsPlayerControlledCaravanMember() && (PawnUtility.ForSaleBySettlement(p) || p.kindDef == PawnKindDefOf.Slave || (PawnUtility.IsKidnappedPawn(p) && p.RaceProps.Humanlike) && !PawnUtility.IsFactionLeader(p))).RandomElementWithFallback();

				PawnGenerationRequest request = new PawnGenerationRequest(kind2, null, PawnGenerationContext.NonPlayer, tile);
				Pawn pawn = PawnGenerator.GeneratePawn(request); //Generate the animal!




				if (pawnOriginal == null)
				{
					pawnOriginal = FormerHumanPawnGenerator.GenerateRandomHumanForm(pawn);
					pawn.Name = pawnOriginal.Name;
				}
				else
				{
					pawn.Name = pawnOriginal.Name;
					Find.WorldPawns.RemovePawn(pawnOriginal);
				}

				var pm = TfSys.TransformedPawn.Create(pawnOriginal, pawn); //pawnOriginal is human, pawn is animal
				FormerHumanUtilities.MakeAnimalSapient(pawnOriginal, pawn, Rand.Range(0.5f, 1f));
				Find.World.GetComponent<PawnmorphGameComp>().AddTransformedPawn(pm);

				yield return pawn;
			}
		}

		/// <summary>
		/// Generates the things that can be sold
		/// </summary>
		/// <param name="forTile">For tile.</param>
		/// <param name="forFaction">For faction.</param>
		/// <returns></returns>
		public override IEnumerable<Thing> GenerateThings(int forTile, Faction forFaction = null)
		{
			var enumer = GenerateThingEnumer(forTile, forFaction).ToList();

			foreach (Pawn pawn in enumer.OfType<Pawn>())
			{
				if (!pawn.IsFormerHuman()) continue;
				RelatedFormerHumanUtilities.ForSaleNotifyIfRelated(pawn);
			}
			return enumer;

		}

		private float SelectionChance(PawnKindDef k)
		{
			return SelectionChanceFromWildnessCurve.Evaluate(k.RaceProps.wildness);
		}

		/// <summary>checks if this generator handles the given thingDef.</summary>
		/// <param name="thingDef">The thing definition.</param>
		/// <returns></returns>
		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef.category == ThingCategory.Pawn && thingDef.race.Animal && thingDef.tradeability != 0 && (tradeTagsSell.Any((string tag) => thingDef.tradeTags != null && thingDef.tradeTags.Contains(tag)));
		}

		private bool PawnKindAllowed(PawnKindDef kind, int forTile)
		{
			if (!kind.RaceProps.Animal || kind.RaceProps.wildness < minWildness || kind.RaceProps.wildness > maxWildness || kind.RaceProps.wildness > 1f)
			{
				return false;
			}
			if (checkTemperature)
			{
				int num = forTile;
				if (num == -1 && Find.AnyPlayerHomeMap != null)
				{
					num = Find.AnyPlayerHomeMap.Tile;
				}
				if (num != -1 && !Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(num, kind.race))
				{
					return false;
				}
			}
			if (kind.race.tradeTags == null)
			{
				return false;
			}
			if (!tradeTagsSell.Any((string x) => kind.race.tradeTags.Contains(x)))
			{
				return false;
			}
			if (!kind.race.tradeability.TraderCanSell())
			{
				return false;
			}
			return true;
		}
		/// <summary>Logs the animal chances.</summary>
		public void LogAnimalChances()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (PawnKindDef allDef in DefDatabase<PawnKindDef>.AllDefs)
			{
				stringBuilder.AppendLine(allDef.defName + ": " + SelectionChance(allDef).ToString("F2"));
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
